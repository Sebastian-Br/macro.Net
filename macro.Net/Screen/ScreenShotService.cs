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
    public class ScreenShotService
    {
        private Size ScreenSize { get; set; }

        public bool Debug { get; set; }

        public ScreenShotService(int _oversize_Y)
        {
            ScreenSize = new Size(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            Debug = false;
            Oversize_Y = _oversize_Y; //[px] the image-tiles will overlap by this number. increase when intending to recognize large characters.
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
            for(int i = 0; i < nr_of_screenshots; i++)
            {
                bool skip_last_split = false;
                int upper_left_corner_Y = split_image_height * i;
                int image_height = split_image_height + Oversize_Y;
                int next_image_height_without_overlap = ScreenSize.Height - (upper_left_corner_Y + image_height);
                int image_width = ScreenSize.Width;
                if(next_image_height_without_overlap < 0.5 * Oversize_Y)
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
                    Paint p = new(0.37, 5000);
                    p.DrawContainingRectangle(screenshot_tile_rectangle);
                    screenshot.Save(DateTime.Now.ToFileTime() + "_DBG_" + i + ".png", ImageFormat.Bmp);
                }

                ScreenImageTile tile = new();
                tile.anchor_y = upper_left_corner_Y; //Tesseract can only know the position of text relative to the screenshot that is passed to it. The absolute position on the screen has to be restored
                tile.Image = ImageProcessor.ToByteArray_AnyFormat(screenshot, ImageFormat.Bmp);
                ScreenshotTiles.Add (tile);
                if (skip_last_split)
                {
                    break;
                }
            }

            return ScreenshotTiles;
        }

        public Bitmap GetFullScreenAsBmp_24bppRgb()
        {
            Bitmap Screenshot = new Bitmap(ScreenSize.Width, ScreenSize.Height, PixelFormat.Format24bppRgb); // Graphics.CopyFromScreen copies the Screen content into this variable, too
            Graphics gScreenshot = Graphics.FromImage(Screenshot);
            gScreenshot.CopyFromScreen(0, 0, 0, 0, ScreenSize);
            //Screenshot.Save("Screen" + DateTime.Now.Year + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + DateTime.Now.Millisecond + ".png", ImageFormat.Png);
            return Screenshot;
        }

        public Bitmap GetScreenAreaAsBmp_24bppRgb(Rectangle rectangle)
        {
            Bitmap Screenshot = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format24bppRgb); // Graphics.CopyFromScreen copies the Screen content into this variable, too
            Graphics gScreenshot = Graphics.FromImage(Screenshot);
            gScreenshot.CopyFromScreen(rectangle.X, rectangle.Y, 0, 0, new Size() { Height = rectangle .Height, Width = rectangle.Width});
            return Screenshot;
        }

        public byte[] GetScreenAreaAsBmpByteArray_24bppRgb(Rectangle rectangle)
        {
            Bitmap Screenshot = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format24bppRgb); // Graphics.CopyFromScreen copies the Screen content into this variable, too
            Graphics gScreenshot = Graphics.FromImage(Screenshot);
            gScreenshot.CopyFromScreen(rectangle.X, rectangle.Y, 0, 0, new Size() { Height = rectangle.Height, Width = rectangle.Width });
            return ImageProcessor.BmpToByteArray_24bpp(Screenshot);
        }

        public byte[] GetFullScreenAsBmpByteArray_24bppRgb()
        {
            Bitmap Screenshot = new Bitmap(ScreenSize.Width, ScreenSize.Height, PixelFormat.Format24bppRgb);
            Graphics gScreenshot = Graphics.FromImage(Screenshot);
            Size s = new(ScreenSize.Width, ScreenSize.Height);
            gScreenshot.CopyFromScreen(0, 0, 0, 0, s);
            //Screenshot.Save("FullScreen.JPG");
            return ImageProcessor.BmpToByteArray_24bpp(Screenshot);
        }
        public byte[] DbgGetFileAsByteArray_24BppArgb(string filePath)
        {
            Bitmap bmpFromFile = new Bitmap(filePath);
            Color c = bmpFromFile.GetPixel(0, 0);
            //byte[] b = ImageProcessor.ToByteArray(bmpFromFile, ImageFormat.Bmp);
            byte[] b = ImageProcessor.BmpToByteArray_24bpp(bmpFromFile);
            return b;
        }

        private int Oversize_Y { get; set; }

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