using macro.Net.ImageProcessing;
using macro.Net.Screen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.ImageDetection
{
    public class ImageDetector
    {
        private ScreenShotService Screenshot_Service { get; set; }

        private ImageProcessor ImageProcessor { get; set; }

        private string ImageDirectory { get; set; }

        /// <summary>
        /// The screenrecorder instance is supposed to be shared between the ImageDetector and OCR classes.
        /// </summary>
        /// <param name="_screenRecorder">The screenrecorder </param>
        /// <param name="_image_directory">The directory whence images are loaded to be matched</param>
        public ImageDetector(ScreenShotService _screenRecorder, string _image_directory)
        {
            Screenshot_Service = _screenRecorder;
            ImageDirectory = _image_directory.Replace("/", "\\");
            ImageProcessor = new();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="search_in"></param>
        /// <param name="search_in_width"></param>
        /// <param name="search_in_height"></param>
        /// <param name="search_for"></param>
        /// <param name="search_for_width"></param>
        /// <param name="search_for_height"></param>
        /// <param name="max_difference_per_px"></param>
        /// <returns></returns>
        public List<ImageMatch> FindAllImagesInImage_BytesInBytes(byte[] search_in, Rectangle search_in_rect, byte[] search_for, int search_for_width, int search_for_height, int max_difference_per_px)
        {
            try
            {
                List<ImageMatch> results = new();
                byte[] search_in_bmp_bytes = search_in;
                byte[] search_for_bmp_bytes = search_for;
                List<Rectangle> rectangles = ImageProcessor.FindAllImagesInImage_24bppRGB(
                    search_in_bmp_bytes, search_in_rect.Width, search_in_rect.Height,
                    search_for_bmp_bytes, search_for_width, search_for_height,
                    max_difference_per_px);

                foreach (Rectangle rect in rectangles)
                {
                    Rectangle result_rectangle = new(rect.X + search_in_rect.X, rect.Y + search_in_rect.Y, rect.Width, rect.Height);
                    ImageMatch match = new(result_rectangle);
                    results.Add(match);
                }

                if (results.Count > 0)
                {
                    return results;
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public ImageMatch FindFirstImageInImage(byte[] searchIn, Rectangle search_in_rect, byte[] searchFor, int search_for_width, int search_for_height, int max_difference_per_px)
        {
            try
            {
                
                byte[] search_in_bytes = searchIn;
                byte[] searchForBmpBytes = searchFor;
                Rectangle? r = ImageProcessor.FindFirstImageInImage_24bppRGB(
                    search_in_bytes, search_in_rect.Width, search_in_rect.Height,
                    searchForBmpBytes, search_for_width, search_for_height, max_difference_per_px);
                if (r != null)
                {
                    Rectangle result_rectangle = new(r.Value.X + search_in_rect.X, r.Value.Y + search_in_rect.Y, r.Value.Width, r.Value.Height);
                    ImageMatch match = new(result_rectangle);
                    return match;
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public ImageProcessor GetImageProcessor()
        {
            return ImageProcessor;
        }

        public ScreenShotService GetScreenShotService()
        {
            return Screenshot_Service;
        }

        public string GetImageDirectory()
        {
            return ImageDirectory;
        }
    }
}