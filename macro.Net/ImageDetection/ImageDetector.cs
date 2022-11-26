using AForge.Imaging;
using macro.Net.Screen;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.ImageDetection
{
    internal class ImageDetector
    {
        private ScreenShotService screenRecorder { get; set; }

        private string ImageDirectory { get; set; }
        public ImageDetector(ScreenShotService _screenRecorder, string imageFolder)
        {
            screenRecorder = _screenRecorder;
            ImageDirectory = imageFolder.Replace("/", "\\");
        }

        /// <summary>
        /// 
        /// from: https://stackoverflow.com/questions/2472467/how-to-find-one-image-inside-of-another
        /// </summary>
        /// <param name="searchFor"></param>
        /// <param name="similarity"></param>
        /// <returns></returns>
        public async Task<ImageMatch> FindFirstImageOnFullScreen(Bitmap searchFor, float similarity)
        {
            try
            {
                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(similarity);
                // find all matches with specified above similarity
                Bitmap sourceImage = screenRecorder.GetFullScreenAsBmp();
                TemplateMatch[] matches = tm.ProcessImage(sourceImage, searchFor);
                // highlight found matches

                BitmapData data = sourceImage.LockBits(
                     new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                     ImageLockMode.ReadWrite, sourceImage.PixelFormat);
                foreach (TemplateMatch m in matches)
                {
                    Drawing.Rectangle(data, m.Rectangle, Color.White);

                    MessageBox.Show(m.Rectangle.Location.ToString());
                    // do something else with matching
                }
                sourceImage.UnlockBits(data);
            }
            catch (Exception e) { }

            return null;
        }

        public async Task<ImageMatch> FindFirstImageOnFullScreenFromBmpFile(string fileInImageDirName, float similarity)
        {
            try
            {
                Bitmap image = new(ImageDirectory + "\\" + fileInImageDirName.Replace("/", "\\"));
                return await FindFirstImageOnFullScreen(image, similarity);
            }
            catch (Exception e) { }
            return null;
        }
    }
}