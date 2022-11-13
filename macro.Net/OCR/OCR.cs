using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using macro.Net.Screen;

namespace macro.Net.OCR
{
    internal class OCR
    {
        public OCR(string tessDataFolder)
        {
            tessEnginge = new TesseractEngine(tessDataFolder, "eng", EngineMode.Default); //"E:/Visual Studio/Projects/DxGameStat/bin/Release/net5.0/tessdata_best";
        }

        public TesseractEngine tessEnginge { get; set; }

        public string GetAllTextOnImage()
        {
            string text = "";
            using (var engine = tessEnginge)
            {
                using (var img = Pix.LoadFromMemory(FrameTime.DbgGetFrameAsBitmapByteArray()))
                {
                    var i = 1;
                    using (var page = engine.Process(img))
                    {
                        text = page.GetText();
                        Console.WriteLine("!!!DBG Text: " + text);
                        Console.WriteLine("!!!DBG Mean confidence: {0}", page.GetMeanConfidence());
                        using (var iter = page.GetIterator()) // don't rly know what this does but it does something!
                        {
                            iter.Begin();
                            do
                            {
                                if (i % 2 == 0)
                                {
                                    Console.WriteLine("!!!DBG Line {0}", i);
                                    do
                                    {
                                        if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                        {
                                            Console.WriteLine("!!!DBG New block");
                                        }
                                        if (iter.IsAtBeginningOf(PageIteratorLevel.Para))
                                        {
                                            Console.WriteLine("!!!DBG New paragraph");
                                        }
                                        if (iter.IsAtBeginningOf(PageIteratorLevel.TextLine))
                                        {
                                            Console.WriteLine("!!!DBG New line");
                                        }
                                        Console.WriteLine("!!!DBG word: " + iter.GetText(PageIteratorLevel.Word));
                                    } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));
                                }
                                i++;
                            } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                        }
                    }
                }
            }

            return text;
        }

        /// <summary>
        /// Gets the position of the first matched word and returns a rectangle that contains said word.
        /// </summary>
        /// <param name="wordToMatch"></param>
        /// <returns></returns>
        public TextMatch GetFirstWordPosition(string wordToMatch, StringComparison stringComparisonType, byte[] image)
        {
            var engine = tessEnginge;
            var img = Pix.LoadFromMemory(image);
            using (var page = engine.Process(img))
            {
                int wordLength = wordToMatch.Length;
                string currentWordToMatch = wordToMatch;
                TextMatch match = GetSingleTextMatchFromPage(page.GetIterator(), currentWordToMatch, wordToMatch, stringComparisonType);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
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
                        if (iter.TryGetBoundingBox(myLevel, out var rect))
                        {
                            TextMatch result = new();
                            result.SearchText = wordToMatch;
                            result.MatchedText = currentWord;
                            result.NativeComparisonMethod = stringComparisonType;
                            result.MatchRect = rect;

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
    }
}
