using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.ImageDetection
{
    internal class ImageMatch
    {
        public ImageMatch(Bitmap _ImageToMatch)
        {
            ImageToMatch = _ImageToMatch;
        }

        public ImageMatch(string FilePath)
        {
            ImageToMatch = new Bitmap(FilePath);
        }
        private Bitmap ImageToMatch { get; set; }
    }
}