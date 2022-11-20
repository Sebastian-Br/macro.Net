using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using macro.Net.Screen;
using System.Diagnostics;

namespace macro.Net.OCR
{
    internal class OCR
    {
        public OCR(string tessDataFolder, int _n_tiles_on_screen)
        {
            tessEngines = new();
            n_tiles_on_screen = _n_tiles_on_screen;
            for (int i = 0; i < n_tiles_on_screen; i++)
            {
                TesseractEngine tessEnginge = new TesseractEngine(tessDataFolder, "lat", EngineMode.TesseractOnly);
                SetEngineFastDefaultConfig(tessEnginge);
                tessEnginge.DefaultPageSegMode = PageSegMode.SparseText;
                tessEngines.Add(tessEnginge);
            }
        }

        public List<TesseractEngine> tessEngines { get; set; }

        private int n_tiles_on_screen { get; set; }

        public async Task<TextMatch> GetFirstWordFromScreenTiles(string wordToMatch, StringComparison stringComparison)
        {
            FrameTime f = new();
            List <ScreenImageTile> image_tiles = f.GetFullScreenAsBmpByteArray_SplitScreen(n_tiles_on_screen);
            int i = 0;
            List<Task<TextMatch>> textMatches = new();
            foreach (ScreenImageTile tile in image_tiles)
            {
                TesseractEngine engine = tessEngines.ElementAt(i); // I think there is a compiler bug that causes this code to crash if the 'tessEngines.ElementAt(i)' part is passed as an argument
                textMatches.Add(Task.Run(()=>GetFirstWordPosition(wordToMatch, stringComparison, tile, engine)));
                i++;
            }

            while(true)
            {
                foreach(Task<TextMatch> textMatch in textMatches)
                {
                    if(textMatch.IsCompleted)
                    {
                        if(textMatch.Result != null)
                        {
                            return textMatch.Result;
                        }
                    }
                }
                await Task.Delay(1);
            }

            return null;
        }

        /// <summary>
        /// Gets the position of the first matched word and returns a rectangle that contains said word.
        /// </summary>
        /// <param name="wordToMatch"></param>
        /// <returns></returns>
        private async Task<TextMatch> GetFirstWordPosition(string wordToMatch, StringComparison stringComparisonType, ScreenImageTile tile, TesseractEngine tessEngine)
        {
            Pix img = Pix.LoadFromMemory(tile.Image);
            //tessEnginge.SetVariable("tessedit_char_whitelist", wordToMatch); --deprecated
            Page page = tessEngine.Process(img);
            Stopwatch clock = new(); clock.Start();
            ResultIterator iter = page.GetIterator();
            clock.Stop();
            Console.WriteLine("GetFirstWordPosition() GetIterator exec time: " + clock.ElapsedMilliseconds);
            string currentWordToMatch = wordToMatch;
            TextMatch match = GetSingleTextMatchFromPage(iter, currentWordToMatch, wordToMatch, stringComparisonType);
            page.Dispose();
            if (match == null)
            {
                return null;
            }
            else
            {
                match.MatchRect = new(match.MatchRect.X, match.MatchRect.Y + tile.anchor_y, match.MatchRect.Width, match.MatchRect.Height);
                return match;
            }
        }

        private TextMatch GetSingleTextMatchFromPage(ResultIterator iter, string currentWordToMatch, string wordToMatch, StringComparison stringComparisonType)
        {
            try
            {
                PageIteratorLevel myLevel = PageIteratorLevel.Word;
                iter.Begin();
                do
                {
                    string currentWord = iter.GetText(myLevel);
                    if (String.Equals(currentWordToMatch, currentWord, stringComparisonType))
                    {
                        //Console.WriteLine("Found: " + currentWord);
                        if (iter.TryGetBoundingBox(myLevel, out var rect))
                        {
                            TextMatch result = new();
                            result.SearchText = wordToMatch;
                            result.MatchedText = currentWord;
                            result.NativeComparisonMethod = stringComparisonType;
                            result.SetMatchRect(rect);
                            return result;
                        }
                    }
                } while (iter.Next(myLevel));
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        private void SetEngineFastDefaultConfig (TesseractEngine engine)
        {
            engine.SetVariable("language_model_penalty_punc", "0");
            engine.SetVariable("language_model_ngram_on", "0");
            engine.SetVariable("load_bigram_dawg", "0");
            engine.SetVariable("load_system_dawg", "0");
            engine.SetVariable("load_freq_dawg", "0");
            engine.SetVariable("load_punc_dawg", "0");
            engine.SetVariable("load_number_dawg", "0");
            engine.SetVariable("load_unambig_dawg", "0");
            engine.SetVariable("load_fixed_length_dawgs", "0"); //takes abt 2.4-2.5s with these settings

            engine.SetVariable("tessedit_enable_doc_dict", "0");
            engine.SetVariable("enable_noise_removal", "0");
            engine.SetVariable("paragraph_text_based", "0");
            engine.SetVariable("tessedit_create_txt", "0");
            engine.SetVariable("textord_tabfind_vertical_text", "0");
            engine.SetVariable("classify_enable_learning", "0");
            engine.SetVariable("wordrec_enable_assoc", "0");
            engine.SetVariable("OMP_THREAD_LIMIT", "1"); // take care with this setting
        }
    }
}
