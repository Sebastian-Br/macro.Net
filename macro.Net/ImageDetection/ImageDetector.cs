using macro.Net.DebugPrint;
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
    /// <summary>
    /// Exposes functionality to locate images on the screen.
    /// </summary>
    public class ImageDetector
    {
        /// <summary>
        /// The ScreenShotService shared with the OCR class
        /// </summary>
        private ScreenShotService Screenshot_Service { get; set; }

        /// <summary>
        /// The ImageProcessor handles lower level operations, such as actually finding the image (an array of bytes) in another image (another array of bytes) or
        /// converting to the correct pixel format (24bppRgb)
        /// </summary>
        private ImageProcessor ImageProcessor { get; set; }

        /// <summary>
        /// The Image Directory relative to the application directory
        /// </summary>
        private string ImageDirectory { get; set; }

        /// <summary>
        /// Whether or not to print debug information
        /// </summary>
        private bool Debug { get; set; }

        /// <summary>
        /// The screenrecorder instance is supposed to be shared between the ImageDetector and OCR classes.
        /// </summary>
        /// <param name="_screenRecorder">The screenrecorder </param>
        /// <param name="_image_directory">The directory whence images are loaded to be matched</param>
        /// <param name="debug">True: Prints debug information. False: Does not</param>
        public ImageDetector(ScreenShotService _screenRecorder, string _image_directory, bool debug)
        {
            Screenshot_Service = _screenRecorder;
            ImageDirectory = _image_directory.Replace("/", "\\");
            ImageProcessor = new();
            Debug = debug;
        }

        /// <summary>
        /// Untested and currently unused.
        /// </summary>
        /// <param name="search_in"></param>
        /// <param name="search_in_rect"></param>
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

        /// <summary>
        /// Finds the first image search_for, in a larger image, search_in.
        /// </summary>
        /// <param name="search_in">The bytes that make up the search_in image</param>
        /// <param name="search_in_rect">The rectangle of that image</param>
        /// <param name="search_for">The bytes that make up the search_for image</param>
        /// <param name="search_for_width">The width of the search_for image</param>
        /// <param name="search_for_height">The height of the search_for image</param>
        /// <param name="max_difference_per_color_channel">The maximum difference that each R, G and B component of the pixel's color is allowed to deviate from the pixel that is searched for</param>
        /// <returns></returns>
        public ImageMatch FindFirstImageInImage(byte[] search_in, Rectangle search_in_rect, byte[] search_for, int search_for_width, int search_for_height, int max_difference_per_color_channel)
        {
            try
            {
                Stopwatch s = new(); s.Start();
                byte[] search_in_bytes = search_in;
                byte[] searchForBmpBytes = search_for;
                Rectangle? r = ImageProcessor.FindFirstImageInImage_24bppRGB(
                    search_in_bytes, search_in_rect.Width, search_in_rect.Height,
                    searchForBmpBytes, search_for_width, search_for_height, max_difference_per_color_channel);
                if (r != null)
                {
                    Rectangle result_rectangle = new(r.Value.X + search_in_rect.X, r.Value.Y + search_in_rect.Y, r.Value.Width, r.Value.Height);
                    ImageMatch match = new(result_rectangle);
                    s.Stop();
                    Dbg.Print("Found image in " + s.ElapsedMilliseconds + " ms", Debug);
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