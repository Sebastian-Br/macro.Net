using macro.Net.ImageDetection;
using macro.Net.Screen;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.ImageProcessing
{
    public class ImageProcessor
    {
        public ImageProcessor() { }

        /// <summary>
        /// Searches in an image 'searchIn' for another image 'searchFor'
        /// </summary>
        /// <param name="searchIn">The byte representation of the image you intend to search</param>
        /// <param name="searchInWidth">The width of that image</param>
        /// <param name="searchInHeight">The height of that image</param>
        /// <param name="searchFor">The byte representation of the image you want to search for</param>
        /// <param name="searchForWidth">The width of the searchFor image</param>
        /// <param name="searchForHeight">The height of the searchFor image</param>
        /// <returns>A rectangle detailing the position of </returns>
        public Rectangle? FindFirstImageInImage_24bppRGB(byte[] searchIn, int searchInWidth, int searchInHeight, byte[] searchFor, int searchForWidth, int searchForHeight, int max_difference_per_px)
        {
            try
            {
                for (int searchInY = 0; searchInY < searchInHeight - (searchForHeight - 1); searchInY++)
                {
                    for(int searchInX = 0; searchInX < searchInWidth - (searchForWidth - 1); searchInX++)
                    {
                        if (Compare3Bytes(searchIn, (searchInY * searchInWidth + searchInX) * 3, searchFor, 0, max_difference_per_px)) // test if the current pixel matches the upper left corner of the searchFor image
                        {
                            for(int compare_images_y = 0; compare_images_y < searchForHeight; compare_images_y++)
                            {
                                for(int compare_images_x = 0; compare_images_x < searchForWidth; compare_images_x++)
                                {
                                    if (!Compare3Bytes(searchIn, ((searchInY + compare_images_y) * searchInWidth + searchInX + compare_images_x) *3, searchFor, (compare_images_y * searchForWidth + compare_images_x) *3, max_difference_per_px))
                                    {
                                        goto location_next_pixel;
                                    }
                                }
                            }

                            return new Rectangle(searchInX, searchInY, searchForWidth, searchForHeight);
                        }

                    location_next_pixel:;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        /// <summary>
        /// Searches in an image 'searchIn' for another image 'searchFor'
        /// </summary>
        /// <param name="searchIn">The byte representation of the image you intend to search</param>
        /// <param name="searchInWidth">The width of that image</param>
        /// <param name="searchInHeight">The height of that image</param>
        /// <param name="searchFor">The byte representation of the image you want to search for</param>
        /// <param name="searchForWidth">The width of the searchFor image</param>
        /// <param name="searchForHeight">The height of the searchFor image</param>
        /// <returns>A list of rectangles detailing the positions of where the image searchFor was found.</returns>
        public List<Rectangle> FindAllImagesInImage_24bppRGB(byte[] searchIn, int searchInWidth, int searchInHeight, byte[] searchFor, int searchForWidth, int searchForHeight, int max_difference_per_px)
        {
            List<Rectangle> results = new();
            try
            {
                for (int searchInY = 0; searchInY < searchInHeight - (searchForHeight - 1); searchInY++)
                {
                    for (int searchInX = 0; searchInX < searchInWidth - (searchForWidth - 1); searchInX++)
                    {
                        if (Compare3Bytes(searchIn, (searchInY * searchInWidth + searchInX) * 3, searchFor, 0, max_difference_per_px)) // test if the current pixel matches the upper left corner of the searchFor image
                        {
                            for (int compare_images_y = 0; compare_images_y < searchForHeight; compare_images_y++)
                            {
                                for (int compare_images_x = 0; compare_images_x < searchForWidth; compare_images_x++)
                                {
                                    if (!Compare3Bytes(searchIn, ((searchInY + compare_images_y) * searchInWidth + searchInX + compare_images_x) * 3, searchFor, (compare_images_y * searchForWidth + compare_images_x) * 3, max_difference_per_px))
                                    {
                                        goto location_next_pixel;
                                    }
                                }
                            }

                            Rectangle rectangle = new Rectangle(searchInX, searchInY, searchForWidth, searchForHeight);
                            results.Add(rectangle);
                        }

                    location_next_pixel:;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            if(results.Count > 0)
            {
                return results;
            }

            return null;
        }

        private bool CompareDWORD(byte[] a, int offsetA, byte[] b, int offsetB, int max_difference_per_pixel)
        {
            int diff1 = Abs(a[offsetA] - b[offsetB]);
            int diff2 = Abs(a[offsetA+1] - b[offsetB+1]);
            int diff3 = Abs(a[offsetA+2] - b[offsetB+2]);
            int diff4 = Abs(a[offsetA + 3] - b[offsetB + 3]);
            return (diff1 <= max_difference_per_pixel) && (diff2 <= max_difference_per_pixel) && (diff3 <= max_difference_per_pixel) && (diff4 <= max_difference_per_pixel);
        }

        private bool Compare3Bytes(byte[] a, int offsetA, byte[] b, int offsetB, int max_difference_per_pixel)
        {
            int diff1 = Abs(a[offsetA] - b[offsetB]);
            int diff2 = Abs(a[offsetA + 1] - b[offsetB + 1]);
            int diff3 = Abs(a[offsetA + 2] - b[offsetB + 2]);
            return (diff1 <= max_difference_per_pixel) && (diff2 <= max_difference_per_pixel) && (diff3 <= max_difference_per_pixel);
        }

        private bool DBG_Compare3Bytes(byte[] a, int offsetA, byte[] b, int offsetB, int max_difference_per_pixel)
        {
            int diff1 = Abs(a[offsetA] - b[offsetB]);
            int diff2 = Abs(a[offsetA + 1] - b[offsetB + 1]);
            int diff3 = Abs(a[offsetA + 2] - b[offsetB + 2]);
            Console.WriteLine("Comparing:");
            Print3Bytes(a, offsetA, "searchIn(" + offsetA/3 + ")");
            Print3Bytes(b, offsetB, "searchFor(" + offsetB / 3 + ")");
            Console.WriteLine("Result: " + ((diff1 <= max_difference_per_pixel) && (diff2 <= max_difference_per_pixel) && (diff3 <= max_difference_per_pixel)));
            return (diff1 <= max_difference_per_pixel) && (diff2 <= max_difference_per_pixel) && (diff3 <= max_difference_per_pixel);
        }

        private bool DBG_CompareDWORD(byte[] a, int offsetA, byte[] b, int offsetB, int max_difference_per_pixel)
        {
            int diff1 = Abs(a[offsetA] - b[offsetB]);
            int diff2 = Abs(a[offsetA + 1] - b[offsetB + 1]);
            int diff3 = Abs(a[offsetA + 2] - b[offsetB + 2]);
            int diff4 = Abs(a[offsetA + 3] - b[offsetB + 3]);
            Console.WriteLine("Comparing:");
            Print4Bytes(a, offsetA, "searchIn");
            Print4Bytes(b, offsetB, "searchFor");
            Console.WriteLine("Result: " + ((diff1 <= max_difference_per_pixel) && (diff2 <= max_difference_per_pixel) && (diff3 <= max_difference_per_pixel) && (diff4 <= max_difference_per_pixel)));
            return (diff1 <= max_difference_per_pixel) && (diff2 <= max_difference_per_pixel) && (diff3 <= max_difference_per_pixel) && (diff4 <= max_difference_per_pixel);
        }

        private void Print4Bytes(byte[] bs, int offset, string prefix)
        {
            Console.WriteLine(prefix + ":" + bs[offset]+ "-" + bs[offset+1] + "-" + bs[offset + 2] + "-" + bs[offset + 3]);
        }

        private void Print3Bytes(byte[] bs, int offset, string prefix)
        {
            Console.WriteLine(prefix + ":" + bs[offset] + "-" + bs[offset + 1] + "-" + bs[offset + 2]);
        }

        private void Print3BytesHex(byte[] bs, int offset, string prefix)
        {
            Console.WriteLine(prefix + ":" + bs[offset].ToString("x") + "-" + bs[offset + 1].ToString("x") + "-" + bs[offset + 2].ToString("x"));
        }

        /// <summary>
        /// Returns the supplied image as an array of bytes.
        /// </summary>
        /// <param name="image">e.g. a Bitmap object</param>
        /// <param name="format">e.g. ImageFormat.Bmp</param>
        /// <returns></returns>
        public static byte[] ToByteArray_AnyFormat(System.Drawing.Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                byte[] bytes = ms.ToArray();
                return bytes;
            }
        }

        /// <summary>
        /// Returns the supplied image as an array of bytes.
        /// https://learn.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.lockbits?view=dotnet-plat-ext-7.0
        /// </summary>
        /// <param name="image">a Bitmap object</param>
        /// <param name="format">e.g. ImageFormat.Bmp</param>
        /// <returns></returns>
        public static byte[] BmpToByteArray_24bpp(Bitmap in_bitmap)
        {
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, in_bitmap.Width, in_bitmap.Height);
            Bitmap bmp;
            if (in_bitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                return Convert32To24bppByteArray(in_bitmap);
            }
            else
            {
                bmp = in_bitmap;
            }

            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Width * bmp.Height * 3;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);
            return rgbValues;
        }

        public static byte[] Bmp32BppToByteArray(Bitmap in_bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, in_bitmap.Width, in_bitmap.Height);

            System.Drawing.Imaging.BitmapData bmpData =
                in_bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                in_bitmap.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = bmpData.Width * in_bitmap.Height * 4;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            in_bitmap.UnlockBits(bmpData);
            return rgbValues;
        }

        private static byte[] Convert32To24bppByteArray(System.Drawing.Bitmap img)
        {
            /*var bmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;*/
            //return img.Clone(new Rectangle(0, 0, img.Width, img.Height), PixelFormat.Format24bppRgb); // the default code for cloning a bitmap in another pixel format is incorrect.
            Bitmap newBmp = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            byte[] bmp32bppArgb = Bmp32BppToByteArray(img);
            int bytes = (bmp32bppArgb.Length / 4) * 3;
            byte[] bmp24bppRgb = new byte[bytes];
            for(int i = 0; i*4 < bmp32bppArgb.Length; i++)
            {
                int offset_bmp32bppArgb = i * 4;
                int offset_bmp24bppRgb = i * 3;
                bmp24bppRgb[offset_bmp24bppRgb] = bmp32bppArgb[offset_bmp32bppArgb];
                bmp24bppRgb[offset_bmp24bppRgb + 1] = bmp32bppArgb[offset_bmp32bppArgb + 1];
                bmp24bppRgb[offset_bmp24bppRgb + 2] = bmp32bppArgb[offset_bmp32bppArgb + 2];
            }

            return bmp24bppRgb;
        }

        private static int Abs(int x)
        {
            if (x < 0)
                return -x;
            return x;
        }

        public static Rectangle CropRectangleToScreenBoundaries(Rectangle rectangle_src)
        {
            int screen_width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int screen_height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            Rectangle rectangle = new(rectangle_src.X, rectangle_src.Y, rectangle_src.Width, rectangle_src.Height);

            int result_rectangle_X;
            int result_rectangle_Y;
            int result_rectangle_width;
            int result_rectangle_height;

            if (rectangle.X < 0)
            {
                result_rectangle_X = 0;
                rectangle.X = 0;
            }
            else
            {
                result_rectangle_X = rectangle.X;
            }

            if (rectangle.Y < 0)
            {
                result_rectangle_Y = 0;
                rectangle.Y = 0;
            }
            else
            {
                result_rectangle_Y = rectangle.Y;
            }

            if (rectangle.Right > screen_width)
            {
                result_rectangle_width = screen_width - rectangle.X + 1;
            }
            else
            {
                result_rectangle_width = rectangle.Width;
            }

            if (rectangle.Bottom > screen_height)
            {
                result_rectangle_height = screen_height - rectangle.Y + 1;
            }
            else
            {
                result_rectangle_height = rectangle.Height;
            }

            Rectangle result = new(result_rectangle_X, result_rectangle_Y, result_rectangle_width, result_rectangle_height);
            return result;
        }

        public static Rectangle CropRectangleToRectangle(Rectangle contained_rectangle, Rectangle containing_rectangle)
        {
            int containing_rect_width = containing_rectangle.Width;
            int containing_rect_height = containing_rectangle.Height;

            Rectangle rectangle = new(contained_rectangle.X, contained_rectangle.Y, contained_rectangle.Width, contained_rectangle.Height);

            int result_rectangle_X;
            int result_rectangle_Y;
            int result_rectangle_width;
            int result_rectangle_height;

            if (rectangle.X < containing_rectangle.X)
            {
                result_rectangle_X = containing_rectangle.X;
                rectangle.X = containing_rectangle.X;
            }
            else
            {
                result_rectangle_X = rectangle.X;
            }

            if (rectangle.Y < containing_rectangle.Y)
            {
                result_rectangle_Y = containing_rectangle.Y;
                rectangle.Y = containing_rectangle.Y;
            }
            else
            {
                result_rectangle_Y = rectangle.Y;
            }

            if (rectangle.Right > containing_rect_width)
            {
                result_rectangle_width = containing_rect_width - rectangle.X + 1;
            }
            else
            {
                result_rectangle_width = rectangle.Width;
            }

            if (rectangle.Bottom > containing_rect_height)
            {
                result_rectangle_height = containing_rect_height - rectangle.Y + 1;
            }
            else
            {
                result_rectangle_height = rectangle.Height;
            }

            Rectangle result = new(result_rectangle_X, result_rectangle_Y, result_rectangle_width, result_rectangle_height);
            return result;
        }
    }
}