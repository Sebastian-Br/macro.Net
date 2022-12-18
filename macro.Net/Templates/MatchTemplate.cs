using macro.Net.ImageDetection;
using macro.Net.ImageProcessing;
using macro.Net.OCR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.Templates
{
    internal class MatchTemplate
    {
        /// <summary>
        /// Constructs a MatchTemplate to match an image inside the specified region of interest on the screen.
        /// </summary>
        /// <param name="_image_detector">The ImageDetector class shared between different MatchTemplates</param>
        /// <param name="_ocr">The OCR class shared between different MatchTemplates</param>
        /// <param name="_default_region_of_interest">The rectangle within which to search</param>
        /// <param name="_image_file_path_in_images_directory">The path to the image file; this file must be in either 24bppRgb or 32bppArgb format</param>
        /// <param name="_only_search_for_first_occurrence">Uses the FindFirstImageInImage() and FindFirstWordInImage() functions respectively</param>
        public MatchTemplate(ImageDetector _image_detector, OCR.OCR _ocr, Rectangle _default_region_of_interest, string _image_file_path_in_images_directory, bool _only_search_for_first_occurrence)
        {
            Image_Detector = _image_detector;
            ImagePathInImageDirectory = (_image_detector.GetImageDirectory() + "\\" + _image_file_path_in_images_directory).Replace("/", "\\");
            SearchForBitmap = new Bitmap(ImagePathInImageDirectory);
            SearchForImageBytes = ImageProcessor.BmpToByteArray_24bpp(SearchForBitmap);
            RegionsOfInterestList = new();
            RegionsOfInterestList.Add(_default_region_of_interest);
            OpticalCharacterRecognition = _ocr;
            Conditions = null;
            OnlySearchForFirstOccurrence = _only_search_for_first_occurrence;
        }

        /// <summary>
        /// Constructs a MatchTemplate to match an image anywhere on the screen (full-screen mode).
        /// </summary>
        /// <param name="_image_detector">The ImageDetector class shared between different MatchTemplates</param>
        /// <param name="_ocr">The OCR class shared between different MatchTemplates</param>
        /// <param name="_default_region_of_interest">The rectangle within which to search</param>
        /// <param name="_image_file_path_in_images_directory">The path to the image file; this file must be in either 24bppRgb or 32bppArgb format</param>
        /// <param name="_only_search_for_first_occurrence">Uses the FindFirstImageInImage() and FindFirstWordInImage() functions respectively</param>
        public MatchTemplate(ImageDetector _image_detector, OCR.OCR _ocr, string _image_file_path_in_images_directory, bool _only_search_for_first_occurrence)
        {
            Image_Detector = _image_detector;
            ImagePathInImageDirectory = (_image_detector.GetImageDirectory() + "\\" + _image_file_path_in_images_directory).Replace("/", "\\");
            SearchForBitmap = new Bitmap(ImagePathInImageDirectory);
            SearchForImageBytes = ImageProcessor.BmpToByteArray_24bpp(SearchForBitmap);
            RegionsOfInterestList.Add(new Rectangle(0, 0, _image_detector.GetScreenShotService().GetScreenWidth(), _image_detector.GetScreenShotService().GetScreenHeight()));
            OpticalCharacterRecognition = _ocr;
            Conditions = null;
            OnlySearchForFirstOccurrence = _only_search_for_first_occurrence;
        }

        /// <summary>
        /// Constructs a MatchTemplate to match a word inside the specified region of interest on the screen.
        /// If a region of interest other than ScreenWidthxScreenHeight is specified, it should be significantly smaller than that size because the resulting image is not split up.
        /// For now, it is recommended that you just search the full screen.
        /// </summary>
        /// <param name="_image_detector">The ImageDetector class shared between different MatchTemplates</param>
        /// <param name="_ocr">The OCR class shared between different MatchTemplates</param>
        /// <param name="_default_region_of_interest">The rectangle within which to search</param>
        /// <param name="_word_to_match">The word to match/search for</param>
        /// <param name="_string_comparison_type">The string comparison method used to compare words found by Tesseract OCR with the _word_to_search</param>
        /// /// <param name="_only_search_for_first_occurrence">Uses the FindFirstImageInImage() and FindFirstWordInImage() functions respectively</param>
        /// <exception cref="ArgumentException"></exception>
        public MatchTemplate(ImageDetector _image_detector, OCR.OCR _ocr, Rectangle _default_region_of_interest, string _word_to_match, StringComparison _string_comparison_type, bool _only_search_for_first_occurrence)
        {
            if(SearchForWord == "")
            {
                throw new ArgumentException("_word_to_match must not be empty!");
            }

            Image_Detector = _image_detector;
            RegionsOfInterestList = new();
            RegionsOfInterestList.Add(_default_region_of_interest);
            OpticalCharacterRecognition = _ocr;
            Conditions = null;
            SearchForWord = _word_to_match;
            StringComparisonMethod = _string_comparison_type;
            OnlySearchForFirstOccurrence = _only_search_for_first_occurrence;
        }

        public async Task<bool> Test()
        {
            if(SearchForImageBytes != null)
            {
                foreach(Rectangle region_of_interest in RegionsOfInterestList)
                {
                    if(OnlySearchForFirstOccurrence)
                    {
                        ImageMatch imgmatch = Image_Detector.FindFirstImageOnScreenArea(region_of_interest, SearchForImageBytes, SearchForBitmap.Width, SearchForBitmap.Height, 5);
                        if (imgmatch != null)
                        {
                            if (Conditions == null)
                            {
                                ImageMatches = new();
                                ImageMatches.Add(imgmatch); // only adding a single element, since we are just searching for the first occurrence.
                                return true;
                            }
                            else if (Conditions.Count == 0)
                            {
                                ImageMatches = new();
                                ImageMatches.Add(imgmatch); // in case conditions may at some point be dynamically added/removed
                                return true;
                            }
                            else
                            {
                                foreach (MatchTemplate condition in Conditions)
                                {
                                    condition.ImportFromParent(this, imgmatch, null);
                                    if(!await condition.Test())
                                    {
                                        goto location_end_of_imagesearch_loop; // condition not satisfied, skip this match
                                    }
                                }

                                ImageMatches = new();
                                ImageMatches.Add(imgmatch);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        // TODO
                    }

                location_end_of_imagesearch_loop:;
                }

                return false;

            }
            else if(SearchForWord != "")
            {
                foreach (Rectangle region_of_interest in RegionsOfInterestList)
                {
                    if (OnlySearchForFirstOccurrence)
                    {
                        TextMatch txtmatch = await OpticalCharacterRecognition.GetFirstWordFromScreenArea(region_of_interest, SearchForWord, StringComparisonMethod);
                        if (txtmatch != null)
                        {
                            if (Conditions == null)
                            {
                                TextMatches = new();
                                TextMatches.Add(txtmatch); // only adding a single element, since we are just searching for the first occurrence.
                                return true;
                            }
                            else if (Conditions.Count == 0)
                            {
                                TextMatches = new();
                                TextMatches.Add(txtmatch); // in case conditions may at some point be dynamically added/removed
                                return true;
                            }
                            else
                            {
                                foreach (MatchTemplate condition in Conditions)
                                {
                                    condition.ImportFromParent(this, null, txtmatch);
                                    if (!await condition.Test())
                                    {
                                        goto location_end_of_wordsearch_loop; // condition not satisfied, skip this match
                                    }
                                }

                                TextMatches = new();
                                TextMatches.Add(txtmatch);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        // TODO
                    }

                location_end_of_wordsearch_loop:;
                }

                return false;
            }
            else
            {
                throw new ArgumentException("This is neither a valid image-match nor a text-match template");
            }

            return false;
        }

        public void ImportFromParent(MatchTemplate parent, ImageMatch imgMatch, TextMatch txtMatch)
        {
            Rectangle parentPosition;
            if(imgMatch != null)
            {
                parentPosition = imgMatch.GetRectangle();
            }
            else if(txtMatch != null)
            {
                parentPosition = txtMatch.MatchRect;
            }
            else
            {
                throw new InvalidOperationException("Attempted to process child conditions when parent's matches were both null");
            }

            int screen_width = Image_Detector.GetScreenShotService().GetScreenWidth();
            int screen_height = Image_Detector.GetScreenShotService().GetScreenHeight();

            int max_permissible_width = screen_width - parentPosition.X - 1;
            int max_permissible_height = screen_height - parentPosition.Y - 1;

            int width = this.Default_Width;
            int height = this.Default_Height;

            if(Default_dX + width > max_permissible_width)
            {
                width = max_permissible_width;
            }

            if(Default_dY + height > max_permissible_height)
            {
                height = max_permissible_height;
            }

            int roi_X = parentPosition.X + Default_dX;
            int roi_Y = parentPosition.Y + Default_dY;

            if(roi_X > screen_width - 1)
            {
                roi_X = screen_width - width - 1;
            }
            else if (roi_X < 0) {
                roi_X = 0;
            }

            if (roi_Y > height - 1)
            {
                roi_Y = screen_height - height - 1;
            }
            else if (roi_X < 0)
            {
                roi_Y = 0;
            }

            this.RegionsOfInterestList = new();
            this.RegionsOfInterestList.Add(new Rectangle(parentPosition.X + Default_dX, parentPosition.Y + Default_dY, width, height));
        }

        private ImageDetector Image_Detector { get; set; }

        private OCR.OCR OpticalCharacterRecognition { get; set; }

        private string ImagePathInImageDirectory { get; set; }

        private Bitmap SearchForBitmap { get; set; }

        private byte[] SearchForImageBytes { get; set; }

        public List<ImageMatch> ImageMatches { get; set; }

        private string SearchForWord { get; set; }

        private StringComparison StringComparisonMethod { get; set; }

        public List<TextMatch> TextMatches { get; set; }

        public List<MatchTemplate> Conditions { get; set; }

        public List<Rectangle> RegionsOfInterestList { get; set; }

        private bool OnlySearchForFirstOccurrence { get; set; }

        /// <summary>
        /// The number of times this node was passed and has failed
        /// </summary>
        private int ConsecutiveFailedPasses { get; set; }


        private int Default_dX { get; set; }

        private int Default_dY { get; set; }

        private int Default_Height { get; set; }

        private int Default_Width { get; set; }
    }
}