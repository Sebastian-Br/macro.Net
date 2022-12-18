using AForge.Imaging;
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
    internal class ImageDetector
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

        public List<ImageMatch> FindAllImagesInImage_BmpInBmp(Bitmap search_in, Bitmap search_for, int max_difference_per_px)
        {
            try
            {
                List<ImageMatch> results = new();
                byte[] search_in_bmp_bytes = ImageProcessor.BmpToByteArray_24bpp(search_in);
                byte[] search_for_bmp_bytes = ImageProcessor.BmpToByteArray_24bpp(search_for);
                List<Rectangle> rectangles = ImageProcessor.FindAllImagesInImage_24bppRGB(
                    search_in_bmp_bytes, search_in.Width, search_in.Height,
                    search_for_bmp_bytes, search_for.Width, search_for.Height,
                    max_difference_per_px);
                
                foreach (Rectangle rect in rectangles)
                {
                    ImageMatch match = new(rect);
                    results.Add(match);
                }

                if(results.Count > 0)
                {
                    return results;
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public List<ImageMatch> FindAllImagesInImage_BytesInBmp(Bitmap search_in, byte[] search_for, int search_for_width, int search_for_height, int max_difference_per_px)
        {
            try
            {
                List<ImageMatch> results = new();
                byte[] search_in_bmp_bytes = ImageProcessor.BmpToByteArray_24bpp(search_in);
                byte[] search_for_bmp_bytes = search_for;
                List<Rectangle> rectangles = ImageProcessor.FindAllImagesInImage_24bppRGB(
                    search_in_bmp_bytes, search_in.Width, search_in.Height,
                    search_for_bmp_bytes, search_for_width, search_for_height,
                    max_difference_per_px);

                foreach (Rectangle rect in rectangles)
                {
                    ImageMatch match = new(rect);
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

        public List<ImageMatch> FindAllImagesInImage_BytesInBytes(byte[] search_in, int search_in_width, int search_in_height, byte[] search_for, int search_for_width, int search_for_height, int max_difference_per_px)
        {
            try
            {
                List<ImageMatch> results = new();
                byte[] search_in_bmp_bytes = search_in;
                byte[] search_for_bmp_bytes = search_for;
                List<Rectangle> rectangles = ImageProcessor.FindAllImagesInImage_24bppRGB(
                    search_in_bmp_bytes, search_in_width, search_in_height,
                    search_for_bmp_bytes, search_for_width, search_for_height,
                    max_difference_per_px);

                foreach (Rectangle rect in rectangles)
                {
                    ImageMatch match = new(rect);
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

        public List<ImageMatch> DBG_FindAllImagesOnFullScreen(Bitmap searchFor, int max_difference_per_px)
        {
            try
            {
                List<ImageMatch> results = new();
                byte[] screenBmpBytes = Screenshot_Service.GetFullScreenAsBmpByteArray_24bppRgb();
                byte[] searchForBmpBytes = ImageProcessor.BmpToByteArray_24bpp(searchFor);
                List<Rectangle> rectangles = ImageProcessor.FindAllImagesInImage_24bppRGB(
                    screenBmpBytes, Screenshot_Service.GetScreenWidth(), Screenshot_Service.GetScreenHeight(),
                    searchForBmpBytes, searchFor.Width, searchFor.Height, max_difference_per_px);

                foreach (Rectangle rect in rectangles)
                {
                    ImageMatch match = new(rect);
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

        public ImageMatch FindFirstImageInImage_BmpInBmp(Bitmap search_in, Bitmap search_for, int max_difference_per_px)
        {
            try
            {
                byte[] search_in_bmp_bytes = ImageProcessor.BmpToByteArray_24bpp(search_in);
                byte[] search_for_bmp_bytes = ImageProcessor.BmpToByteArray_24bpp(search_for);
                Rectangle? r = ImageProcessor.FindFirstImageInImage_24bppRGB(
                    search_in_bmp_bytes, search_in.Width, search_in.Height,
                    search_for_bmp_bytes, search_for.Width, search_for.Height,
                    max_difference_per_px);
                if (r != null)
                {
                    ImageMatch match = new(r.Value);
                    return match;
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public ImageMatch FindFirstImageOnScreenArea(Rectangle area, byte[] searchFor, int search_for_width, int search_for_height, int max_difference_per_px)
        {
            try
            {
                byte[] screenBmpBytes = Screenshot_Service.GetScreenAreaAsBmpByteArray_24bppRgb(area);
                byte[] searchForBmpBytes = searchFor;
                Rectangle? r = ImageProcessor.FindFirstImageInImage_24bppRGB(
                    screenBmpBytes, area.Width, area.Height,
                    searchForBmpBytes, search_for_width, search_for_height, max_difference_per_px);
                if (r != null)
                {
                    ImageMatch match = new(r.Value);
                    return match;
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public ImageMatch DBG_FindFirstImageOnFullScreen(Bitmap searchFor, int max_difference_per_px)
        {
            try
            {
                byte[] screenBmpBytes = Screenshot_Service.GetFullScreenAsBmpByteArray_24bppRgb();
                byte[] searchForBmpBytes = ImageProcessor.BmpToByteArray_24bpp(searchFor);
                Rectangle? r = ImageProcessor.FindFirstImageInImage_24bppRGB(
                    screenBmpBytes, Screenshot_Service.GetScreenWidth(), Screenshot_Service.GetScreenHeight(),
                    searchForBmpBytes, searchFor.Width, searchFor.Height, max_difference_per_px);
                if(r != null)
                {
                    ImageMatch match = new(r.Value);
                    return match;
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public async Task<ImageMatch> FindFirstImageInImage_AForge(Bitmap search_in_image, Bitmap search_for_image, float similarity)
        {
            try
            {
                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(similarity);
                TemplateMatch[] matches = tm.ProcessImage(search_in_image, search_for_image);

                foreach (TemplateMatch m in matches)
                {
                    return new ImageMatch(m.Rectangle);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("FindFirstImageInImage_AForge(): " + e);
            }

            return null;
        }

        /// <summary>
        /// Fails on 32bpp format.
        /// from: https://stackoverflow.com/questions/2472467/how-to-find-one-image-inside-of-another
        /// </summary>
        /// <param name="search_for_image"></param>
        /// <param name="similarity"></param>
        /// <returns></returns>
        public async Task<ImageMatch> DBG_FindFirstImageOnFullScreen_AForge(Bitmap search_for_image, float similarity)
        {
            try
            {
                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(similarity);
                Bitmap search_in_image = Screenshot_Service.GetFullScreenAsBmp_24bppRgb();

                return await FindFirstImageInImage_AForge(search_in_image, search_for_image, similarity);
            }
            catch (Exception e)
            {
                Console.WriteLine("FindFirstImageOnFullScreen_AForge(): " + e);
            }

            return null;
        }

        /// <summary>
        /// Takes around 3 seconds to find the Windows Home Button logo. Duh.
        /// </summary>
        /// <param name="fileInImageDirName"></param>
        /// <param name="similarity"></param>
        /// <returns></returns>
        public async Task<ImageMatch> DBG_FindFirstImageOnFullScreenFromBmpFile_AForge(string fileInImageDirName, float similarity)
        {
            try
            {
                Bitmap image = new(ImageDirectory + "\\" + fileInImageDirName.Replace("/", "\\"));
                return await DBG_FindFirstImageOnFullScreen_AForge(image, similarity);
            }
            catch (Exception e)
            {
                Console.WriteLine("FindFirstImageOnFullScreenFromBmpFile(): " + e);
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