using macro.Net.Engine;
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
    public class MatchTemplate
    {
        /// <summary>
        /// Constructs a MatchTemplate to match an image inside the specified region of interest on the screen.
        /// </summary>
        /// <param name="_image_detector">The ImageDetector class shared between different MatchTemplates</param>
        /// <param name="_ocr">The OCR class shared between different MatchTemplates</param>
        /// <param name="_default_region_of_interest">The rectangle within which to search</param>
        /// <param name="_image_file_path_in_images_directory">The path to the image file; this file must be in either 24bppRgb or 32bppArgb format</param>
        /// <param name="_only_search_for_first_occurrence">Uses the FindFirstImageInImage() and FindFirstWordInImage() functions respectively</param>
        public MatchTemplate(MacroEngine _macro_engine, Rectangle _default_region_of_interest, string _image_file_path_in_images_directory, bool _only_search_for_first_occurrence, string _dictionary_key)
        {
            Image_Detector = _macro_engine.GetImageDetector();
            ImagePathInImageDirectory = (Image_Detector.GetImageDirectory() + "\\" + _image_file_path_in_images_directory).Replace("/", "\\");
            SearchForBitmap = new Bitmap(ImagePathInImageDirectory);
            SearchForImageBytes = ImageProcessor.BmpToByteArray_24bpp(SearchForBitmap);
            RegionsOfInterestList = new();
            RegionsOfInterestList.Add(_default_region_of_interest);
            OpticalCharacterRecognition = _macro_engine.GetOCR();
            Conditions = null;
            OnlySearchForFirstOccurrence = _only_search_for_first_occurrence;
            DictionaryKey = _dictionary_key;
            CycleCheck_Visited = false;
            ActionRectangle = new Rectangle(0, 0, SearchForBitmap.Width, SearchForBitmap.Height);
        }

        /// <summary>
        /// Constructs a MatchTemplate to match an image anywhere on the screen (full-screen mode).
        /// </summary>
        /// <param name="_image_detector">The ImageDetector class shared between different MatchTemplates</param>
        /// <param name="_ocr">The OCR class shared between different MatchTemplates</param>
        /// <param name="_default_region_of_interest">The rectangle within which to search</param>
        /// <param name="_image_file_path_in_images_directory">The path to the image file; this file must be in either 24bppRgb or 32bppArgb format</param>
        /// <param name="_only_search_for_first_occurrence">Uses the FindFirstImageInImage() and FindFirstWordInImage() functions respectively</param>
        public MatchTemplate(MacroEngine _macro_engine, string _image_file_path_in_images_directory, bool _only_search_for_first_occurrence, string _dictionary_key)
        {
            Image_Detector = _macro_engine.GetImageDetector();
            ImagePathInImageDirectory = (Image_Detector.GetImageDirectory() + "\\" + _image_file_path_in_images_directory).Replace("/", "\\");
            SearchForBitmap = new Bitmap(ImagePathInImageDirectory);
            SearchForImageBytes = ImageProcessor.BmpToByteArray_24bpp(SearchForBitmap);
            RegionsOfInterestList.Add(new Rectangle(0, 0, Image_Detector.GetScreenShotService().GetScreenWidth(), Image_Detector.GetScreenShotService().GetScreenHeight()));
            OpticalCharacterRecognition = _macro_engine.GetOCR();
            Conditions = null;
            OnlySearchForFirstOccurrence = _only_search_for_first_occurrence;
            DictionaryKey = _dictionary_key;
            CycleCheck_Visited = false;
            ActionRectangle = new Rectangle(0, 0, SearchForBitmap.Width, SearchForBitmap.Height);
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
        public MatchTemplate(MacroEngine _macro_engine, Rectangle _default_region_of_interest, string _word_to_match, StringComparison _string_comparison_type, bool _only_search_for_first_occurrence, string _dictionary_key)
        {
            if(SearchForWord == "")
            {
                throw new ArgumentException("_word_to_match must not be empty!");
            }

            Image_Detector = _macro_engine.GetImageDetector();
            RegionsOfInterestList = new();
            RegionsOfInterestList.Add(_default_region_of_interest);
            OpticalCharacterRecognition = _macro_engine.GetOCR();
            Conditions = null;
            SearchForWord = _word_to_match;
            StringComparisonMethod = _string_comparison_type;
            OnlySearchForFirstOccurrence = _only_search_for_first_occurrence;
            DictionaryKey = _dictionary_key;
            CycleCheck_Visited = false;
        }

        public async Task<bool> Test()
        {
            if(SearchForImageBytes != null)
            {
                foreach(Rectangle region_of_interest in RegionsOfInterestList)
                {
                    if (this.Condition_Default_Offset.IsEmpty) // this is not a condition - update SearchInImageBytes. If this was a condition, these bytes have already been imported
                    {
                        SearchInImageBytes = Image_Detector.GetScreenShotService().GetScreenAreaAsBmpByteArray_24bppRgb(region_of_interest);
                    }

                    if (OnlySearchForFirstOccurrence)
                    {
                        ImageMatch imgmatch = Image_Detector.FindFirstImageInImage(SearchInImageBytes, region_of_interest, SearchForImageBytes, SearchForBitmap.Width, SearchForBitmap.Height, 5); ;
                        if (imgmatch != null)
                        {
                            Rectangle img_match_rect = imgmatch.GetRectangle();
                            imgmatch.ActionRectangle = ImageProcessor.CropRectangleToScreenBoundaries(new Rectangle(img_match_rect.X + ActionRectangle.X, img_match_rect.Y + ActionRectangle.Y, ActionRectangle.Width, ActionRectangle.Height));
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
                                    condition.ImportFromParent(this, imgmatch, null, region_of_interest);
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
                        TextMatch txtmatch = await OpticalCharacterRecognition.GetFirstWordFromScreenAreaBytes(SearchInImageBytes, region_of_interest, SearchForWord, StringComparisonMethod);
                        if (txtmatch != null)
                        {
                            Rectangle textmatch_rect = txtmatch.MatchRect;
                            if(ActionRectangle.IsEmpty) // by default, click on the bounding box that Tesseract specified
                            {
                                txtmatch.ActionRectangle = textmatch_rect;
                            }
                            else // or if an ActionRectangle inside that bounding box is specified, click on that one.
                            {
                                txtmatch.ActionRectangle = ImageProcessor.CropRectangleToScreenBoundaries(new Rectangle(textmatch_rect.X + ActionRectangle.X, textmatch_rect.Y + ActionRectangle.Y, ActionRectangle.Width, ActionRectangle.Height));
                            }

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
                                    condition.ImportFromParent(this, null, txtmatch, region_of_interest);
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
                return false;

            }
        }

        public void SetActionRectangle(Rectangle rect)
        {
            if(SearchForImageBytes != null) // this is an ImageMatch
            {
                ActionRectangle = rect;
            }
            else if (SearchForWord != "")
            {
                ActionRectangle = rect;
            }
            else
            {
                throw new InvalidOperationException("This was neither a Image- nor a Text-Match!");
            }
        }

        /// <summary>
        /// Configures the Condition and its region of interest according to the Match it depends on
        /// Called by the Condition itself.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="imgMatch"></param>
        /// <param name="txtMatch"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void ImportFromParent(MatchTemplate parent, ImageMatch imgMatch, TextMatch txtMatch, Rectangle current_roi)
        {
            this.SearchInImageBytes = parent.SearchInImageBytes;
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

            int roi_width = this.Condition_Default_Offset.Width;
            int roi_height = this.Condition_Default_Offset.Height;
            int roi_X = parentPosition.X + this.Condition_Default_Offset.X;
            int roi_Y = parentPosition.Y + this.Condition_Default_Offset.Y;
            Rectangle uncropped_roi = new(roi_X, roi_Y, roi_width, roi_height);
            Rectangle cropped_roi = ImageProcessor.CropRectangleToRectangle(uncropped_roi, current_roi);
            this.RegionsOfInterestList = new();
            this.RegionsOfInterestList.Add(cropped_roi);
        }

        private ImageDetector Image_Detector { get; set; }

        private OCR.OCR OpticalCharacterRecognition { get; set; }

        private string ImagePathInImageDirectory { get; set; }

        private Bitmap SearchForBitmap { get; set; }

        private byte[] SearchForImageBytes { get; set; }

        private byte[] SearchInImageBytes { get; set; } // size is stored in the region-of-interest

        public List<ImageMatch> ImageMatches { get; set; }

        private string SearchForWord { get; set; }

        private StringComparison StringComparisonMethod { get; set; }

        public List<TextMatch> TextMatches { get; set; }

        public List<MatchTemplate> Conditions { get; set; }

        public List<Rectangle> RegionsOfInterestList { get; set; }

        /// <summary>
        /// The rectangle (relative to the match-rectangle) on which to perform an action (with an ActionTemplate), such as a mouse click
        /// </summary>
        public Rectangle ActionRectangle { get; set; }

        private bool OnlySearchForFirstOccurrence { get; set; }

        /// <summary>
        /// The number of times this node was passed and has failed
        /// </summary>
        private int ConsecutiveFailedPasses { get; set; }

        /// <summary>
        /// A condition MatchTemplate may be offset from the initial match by a certain amount.
        /// This rectangle describes the relative position of this rectangle in relation to the initial match.
        /// </summary>
        private Rectangle Condition_Default_Offset { get; set; }

        private ActionTemplate ChildAction { get; set; }

        private string DictionaryKey { get; set; }

        public string GetDictionaryKey () { return DictionaryKey; }

        public bool CycleCheck_Visited { get; set; }

        public void SetChildAction(ActionTemplate action_template)
        {
            ChildAction = action_template;
        }

        public ActionTemplate GetChildAction()
        {
            return ChildAction;
        }
    }
}