using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using macro.Net.Screen;
using System.Diagnostics;
using macro.Net.ImageProcessing;
using macro.Net.DebugPrint;

namespace macro.Net.OCR
{
    /// <summary>
    /// Exposes functionality to localize a word (or arbitrary sequence of letters) on the screen.
    /// </summary>
    public class OCR
    {
        /// <summary>
        /// Constructs the class to provide Optical Character Recognition (OCR).
        /// </summary>
        /// <param name="_tessdata_dir">Directory of tesseract files relative to the application directory</param>
        /// <param name="_n_tiles_on_screen">The number of horizontal tiles to split the screen into when searching for Text</param>
        /// <param name="_sssvc">The ScreenShotService class shared with the ImageDetector class</param>
        /// <param name="_debug">Whether to print Debug messages (true) or not (false)</param>
        public OCR(string _tessdata_dir, int _n_tiles_on_screen, ScreenShotService _sssvc, bool _debug)
        {
            TessEngines = new();
            n_tiles_on_screen = _n_tiles_on_screen;
            for (int i = 0; i < n_tiles_on_screen; i++)
            {
                string tessdir = (AppDomain.CurrentDomain.BaseDirectory + _tessdata_dir).Replace("\\", "/");
                TesseractEngine tessEnginge = new TesseractEngine(tessdir, "lat", EngineMode.TesseractOnly);
                SetEngineFastDefaultConfig(tessEnginge);
                tessEnginge.DefaultPageSegMode = PageSegMode.SparseText;
                TessEngines.Add(tessEnginge);
            }

            ScreenShotSvc = _sssvc;
            Debug = _debug;
        }

        /// <summary>
        /// A List of Tesseract Engines used to process the entire screen area simulatenously.
        /// </summary>
        public List<TesseractEngine> TessEngines { get; set; }

        /// <summary>
        /// The number of tiles the screen is split into.
        /// </summary>
        private int n_tiles_on_screen { get; set; }

        /// <summary>
        /// The ScreenShotService instance shared between the OCR and ImageDetector classes.
        /// </summary>
        private ScreenShotService ScreenShotSvc { get; set; }

        /// <summary>
        /// True: Print debug information. False: Print no information.
        /// </summary>
        private bool Debug { get; set; }

        /// <summary>
        /// UNTESTED
        /// Gets the first WordMatch (if any) on a specified screen area.
        /// This function does not split up the screen area like the GetFirstWordFromFullScreenTiles function does.
        /// Thus, the screen area should be significantly smaller than the full screen. Otherwise this function will be slower.
        /// This is why if the specified area is equal to the screen area, this function will call a highly optimized version of the function instead.
        /// </summary>
        /// <param name="area">The area on the screen to search</param>
        /// <param name="word_to_match">The word to match</param>
        /// <param name="string_comparison">The string comparison method used to compare results from </param>
        /// <returns></returns>
        public async Task<TextMatch> GetFirstWordFromScreenAreaBytes(byte[] area_bytes, Rectangle area, string word_to_match, StringComparison string_comparison)
        {
            if (area.Width == System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width && area.Height == System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height)
            {
                return await GetFirstWordFromFullScreenTiles(word_to_match, string_comparison);
            }
            TesseractEngine engine = TessEngines.First();
            string current_word_to_match = word_to_match; // nice-to-have: if the full word is not found, search for sub-strings
            Pix img = Pix.LoadFromMemory(area_bytes);
            Page page = engine.Process(img);
            Stopwatch clock = new(); clock.Start();
            ResultIterator iter = page.GetIterator();
            clock.Stop();
            Dbg.Print("GetFirstWordFromScreenArea() GetIterator exec time: " + clock.ElapsedMilliseconds, Debug);
            TextMatch match = GetSingleTextMatchFromPage(iter, current_word_to_match, word_to_match, string_comparison);
            page.Dispose();

            if (match != null)
            {
                match.MatchRect = new(match.MatchRect.X + area.X, match.MatchRect.Y + area.Y, match.MatchRect.Width, match.MatchRect.Height); // recalculate the position
                return match;
            }

            return null;
        }

        private async Task<TextMatch> GetFirstWordFromFullScreenTiles(string word_to_match, StringComparison string_comparison)
        {
            List <ScreenImageTile> image_tiles = ScreenShotSvc.GetFullScreenAsBmpByteArray_SplitScreen(n_tiles_on_screen);
            int i = 0;
            List<Task<TextMatch>> text_match_tasks = new();
            foreach (ScreenImageTile tile in image_tiles)
            {
                TesseractEngine engine = TessEngines.ElementAt(i); // I think there is a compiler bug that causes this code to crash if the 'tessEngines.ElementAt(i)' part is passed as an argument
                text_match_tasks.Add(Task.Run(()=>GetFirstWordPosition(word_to_match, string_comparison, tile, engine)));
                i++;
            }

            while(true)
            {
                int completed_task_count = 0;
                foreach(Task<TextMatch> text_match_task in text_match_tasks)
                {
                    if(text_match_task.IsCompleted)
                    {
                        if(text_match_task.Result != null)
                        {
                            return text_match_task.Result;
                        }

                        completed_task_count++;
                    }
                }

                if(completed_task_count == text_match_tasks.Count)
                {
                    return null;
                }

                await Task.Delay(1);
            }
        }

        /// <summary>
        /// Gets the position of the first matched word and returns a rectangle that contains said word.
        /// Called by GetFirstWordFromFullScreenTiles
        /// </summary>
        /// <param name="word_to_match">The word to be matched</param>
        /// <param name="string_comparison">The string comparison method used to compare all OCR results with the word_to_match</param>
        /// <param name="tess_engine">The Tesseract Engine to use</param>
        /// <param name="tile">The tile of the screen that describes the image position and data</param>
        /// <returns></returns>
        private async Task<TextMatch> GetFirstWordPosition(string word_to_match, StringComparison string_comparison, ScreenImageTile tile, TesseractEngine tess_engine)
        {
            Pix img = Pix.LoadFromMemory(tile.Image);
            //tessEnginge.SetVariable("tessedit_char_whitelist", wordToMatch); --deprecated
            Page page = tess_engine.Process(img);
            Stopwatch clock = new(); clock.Start();
            ResultIterator iter = page.GetIterator();
            clock.Stop();
            Dbg.Print("GetFirstWordPosition() GetIterator exec time: " + clock.ElapsedMilliseconds, Debug);
            string currentWordToMatch = word_to_match;
            TextMatch match = GetSingleTextMatchFromPage(iter, currentWordToMatch, word_to_match, string_comparison);
            page.Dispose();
            if (match != null)
            {
                match.MatchRect = new(match.MatchRect.X, match.MatchRect.Y + tile.anchor_y, match.MatchRect.Width, match.MatchRect.Height);
                return match;
            }

            return null;
        }
        
        /// <summary>
        /// Finds the first word to match and returns it.
        /// </summary>
        /// <param name="iter">The result iterator (the result of the OCR)</param>
        /// <param name="current_word_to_match">The word that is currently being matched. In the future this might not be the same as initial_word_to_match</param>
        /// <param name="initial_word_to_match">Currently the same as current_word_to_match</param>
        /// <param name="string_comparison">The string comparison method to use when comparing OCR results to the word that is to be matched</param>
        /// <returns>The first TextMatch on that page</returns>
        private TextMatch GetSingleTextMatchFromPage(ResultIterator iter, string current_word_to_match, string initial_word_to_match, StringComparison string_comparison)
        {
            try
            {
                PageIteratorLevel myLevel = PageIteratorLevel.Word;
                iter.Begin();
                do
                {
                    string currentWord = iter.GetText(myLevel);
                    if (String.Equals(current_word_to_match, currentWord, string_comparison))
                    {
                        //Console.WriteLine("Found: " + currentWord);
                        if (iter.TryGetBoundingBox(myLevel, out var rect))
                        {
                            TextMatch result = new();
                            result.SearchText = initial_word_to_match;
                            result.MatchedText = currentWord;
                            result.NativeComparisonMethod = string_comparison;
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

        /// <summary>
        /// Optimizes the engine to recognize words as a sequence of arbitrary letters primarily
        /// This speeds up the OCR process and yields increased precision when a sequence of letters is not a word, e.g. when matching abbreviations
        /// </summary>
        /// <param name="engine"></param>
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
            engine.SetVariable("OMP_THREAD_LIMIT", "1"); // take care with this setting. the parallelization inside macro.Net is superior to the native one
        }
    }
}