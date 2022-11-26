using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;

namespace macro.Net.Screen
{
    public class ScreenShotService
    {
        private Size ScreenSize { get; set; }

        public bool Debug { get; set; }

        public ScreenShotService()
        {
            ScreenSize = new Size(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            Debug = false;
        }

        /// <summary>
        /// file:///E:/Visual%20Studio/Projects/macro.Net/macro.Net/Screen/ScreenShotService.cs.Doc/GetFullScreenAsBmpByteArray_SplitScreen.rtf
        /// </summary>
        /// <param name="nr_of_screenshots"></param>
        /// <returns></returns>
        public List<ScreenImageTile> GetFullScreenAsBmpByteArray_SplitScreen(int nr_of_screenshots = 2)
        {
            List<ScreenImageTile> ScreenshotTiles = new();
            int split_image_height = ScreenSize.Height / nr_of_screenshots;
            int overlap_y = 80; //[px] the image-tiles will overlap by this number. increase when intending to recognize large characters.
            for(int i = 0; i < nr_of_screenshots; i++)
            {
                bool skip_last_split = false;
                int upper_left_corner_Y = split_image_height * i;
                int image_height = split_image_height + overlap_y;
                int next_image_height_without_overlap = ScreenSize.Height - (upper_left_corner_Y + image_height);
                int image_width = ScreenSize.Width;
                if(next_image_height_without_overlap < 0.5 * overlap_y)
                {
                    skip_last_split = true; // the last image will be too small. instead, increase the size of the current image to fit the screen
                    image_height = ScreenSize.Height - upper_left_corner_Y;
                }

                Bitmap screenshot = new Bitmap(image_width, image_height, PixelFormat.Format32bppArgb);
                Graphics g_screenshot = Graphics.FromImage(screenshot);
                g_screenshot.CopyFromScreen(0, upper_left_corner_Y, 0, 0, new Size(image_width, image_height));
                if (Debug)
                {
                    Rectangle screenshot_tile_rectangle = new Rectangle(0, upper_left_corner_Y, image_width, image_height);
                    Paint p = new(0.37, 5000);
                    p.DrawContainingRectangle(screenshot_tile_rectangle);
                    screenshot.Save(DateTime.Now.ToFileTime() + "_DBG_" + i + ".png", ImageFormat.Bmp);
                }

                ScreenImageTile tile = new();
                tile.anchor_y = upper_left_corner_Y; //Tesseract can only know the position of text relative to the screenshot that is passed to it. The absolute position on the screen has to be restored
                tile.Image = ToByteArray(screenshot, ImageFormat.Bmp);
                ScreenshotTiles.Add (tile);
                if (skip_last_split)
                {
                    break;
                }
            }

            return ScreenshotTiles;
        }

        /// <summary>
        /// Returns the supplied image as an array of bytes.
        /// </summary>
        /// <param name="image">e.g. a Bitmap object</param>
        /// <param name="format">e.g. ImageFormat.Bmp</param>
        /// <returns></returns>
        public static byte[] ToByteArray(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        public Bitmap GetFullScreenAsBmp()
        {
            Stopwatch clock = new(); clock.Start();
            Bitmap Screenshot = new Bitmap(ScreenSize.Width, ScreenSize.Height, PixelFormat.Format32bppArgb); // Graphics.CopyFromScreen copies the Screen content into this variable, too.
            Graphics gScreenshot = Graphics.FromImage(Screenshot);
            // Console.WriteLine("GetFrame() Execution-Time1: " + clock.ElapsedMilliseconds + " [ms]."); this is always 0 ms.
            //Size s = new(ScreenWidth, ScreenHeight);
            gScreenshot.CopyFromScreen(0, 0, 0, 0, ScreenSize);
            Console.WriteLine("GetFullScreenAsBmp() Execution-Time: " + clock.ElapsedMilliseconds + " [ms].");
            //Screenshot.Save("Screen" + DateTime.Now.Year + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + DateTime.Now.Millisecond + ".png", ImageFormat.Png);
            clock.Stop();
            return Screenshot;
        }

        public byte[] GetFullScreenAsBmpByteArray()
        {
            Bitmap Screenshot = new Bitmap(ScreenSize.Width, ScreenSize.Height, PixelFormat.Format32bppArgb);
            Graphics gScreenshot = Graphics.FromImage(Screenshot);
            Size s = new(ScreenSize.Width, ScreenSize.Height);
            gScreenshot.CopyFromScreen(0, 0, 0, 0, s);
            return ToByteArray(Screenshot, ImageFormat.Bmp);
        }
    }
}