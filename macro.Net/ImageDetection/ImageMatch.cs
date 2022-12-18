using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.ImageDetection
{
    internal class ImageMatch
    {
        public ImageMatch(Rectangle _rectangle)
        {
            rectangle= _rectangle;
        }

        private Rectangle rectangle { get; set; }

        public Rectangle GetRectangle()
        {
            return rectangle;
        }
    }
}