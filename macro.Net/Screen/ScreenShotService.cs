using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using macro.Net.ImageProcessing;

namespace macro.Net.Screen
{
    /// <summary>
    /// Takes screenshots and returns the data as byte array(s)
    /// </summary>
    public class ScreenShotService
    {
        private Size ScreenSize { get; set; }

        public bool Debug { get; set; }

        /// <summary>
        /// The number of pixels by which adjacent tiles (when making split-up screenshots) overlap
        /// This is relevant only for the OCR.
        /// If this value is too small, words comprised of large characters at the border of two tiles will not be identified correctly.
        /// </summary>
        private int Overlap_Y { get; set; }

        /// <summary>
        /// Constructs a new ScreenShotService object
        /// </summary>
        /// <param name="_overlap_y">The number of pixels by which adjacent tiles (when making split-up screenshots) overlap</param>
        /// <param name="_debug">Whether to print debug messages (true) or not (false)</param>
        public ScreenShotService(int _overlap_y, bool _debug)
        {
            ScreenSize = new Size(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            Debug = _debug;
            Overlap_Y = _overlap_y; //[px] the image-tiles will overlap by this number. increase when intending to recognize large characters.
            Paint = new(0.37, 2500);
        }

        private Paint Paint { get; set; }

        /// <summary>
        /// This function takes multiple screenshots, where each screenshot captures a different area of the screen. This is allows us to process them simultaneously.
        /// file:///E:/Visual%20Studio/Projects/macro.Net/macro.Net/Screen/ScreenShotService.cs.Doc/GetFullScreenAsBmpByteArray_SplitScreen.rtf
        /// </summary>
        /// <param name="nr_of_screenshots">The number of screenshots to be taken</param>
        /// <returns>A list of tiles representing the split-up screen area</returns>
        public List<ScreenImageTile> GetFullScreenAsBmpByteArray_SplitScreen(int nr_of_screenshots = 2)
        {
            List<ScreenImageTile> result_screenshot_tiles = new();
            List<Rectangle> screenshots_rectangles = new(); // shown on the screen when debug == true
            int split_image_height = ScreenSize.Height / nr_of_screenshots;
            for(int i = 0; i < nr_of_screenshots; i++)
            {
                bool skip_last_split = false;
                int upper_left_corner_Y = split_image_height * i;
                int image_height = split_image_height + Overlap_Y;
                int next_image_height_without_overlap = ScreenSize.Height - (upper_left_corner_Y + image_height);
                int image_width = ScreenSize.Width;
                if(next_image_height_without_overlap < 0.5 * Overlap_Y)
                {
                    skip_last_split = true; // the last image will be too small. instead, increase the size of the current image to fit the screen
                    image_height = ScreenSize.Height - upper_left_corner_Y;
                }

                Bitmap screenshot = new Bitmap(image_width, image_height, PixelFormat.Format24bppRgb);
                Graphics g_screenshot = Graphics.FromImage(screenshot);
                g_screenshot.CopyFromScreen(0, upper_left_corner_Y, 0, 0, new Size(image_width, image_height));
                if (Debug)
                {
                    Rectangle screenshot_tile_rectangle = new Rectangle(0, upper_left_corner_Y, image_width, image_height);
                    screenshots_rectangles.Add(screenshot_tile_rectangle);
                }

                ScreenImageTile tile = new(upper_left_corner_Y); //Tesseract can only know the position of text relative to the screenshot that is passed to it. The absolute position on the screen has to be restored using this value
                tile.Image = ImageProcessor.ToByteArray_AnyFormat(screenshot, ImageFormat.Bmp);
                result_screenshot_tiles.Add (tile);
                if (skip_last_split)
                {
                    break;
                }
            }

            foreach(Rectangle rectangle in screenshots_rectangles)
            {
                Paint.DrawContainingRectangle(rectangle);
            }

            return result_screenshot_tiles;
        }

        public byte[] GetScreenAreaAsBmpByteArray_24bppRgb(Rectangle rectangle)
        {
            Bitmap bmp_screenshot = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format24bppRgb); // Graphics.CopyFromScreen copies the Screen content into this variable, too
            Graphics g_screenshot = Graphics.FromImage(bmp_screenshot);
            g_screenshot.CopyFromScreen(rectangle.X, rectangle.Y, 0, 0, new Size() { Height = rectangle.Height, Width = rectangle.Width });
            return ImageProcessor.BmpToByteArray_24bpp(bmp_screenshot);
        }

        public byte[] DbgGetFileAsByteArray_24BppArgb(string filePath)
        {
            Bitmap bmp_from_file = new Bitmap(filePath);
            byte[] b = ImageProcessor.BmpToByteArray_24bpp(bmp_from_file);
            return b;
        }


        public int GetScreenWidth()
        {
            return ScreenSize.Width;
        }

        public int GetScreenHeight()
        {
            return ScreenSize.Height;
        }
    }
}