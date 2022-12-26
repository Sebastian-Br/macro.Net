using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.ImageDetection
{
    public class ImageMatch
    {
        public ImageMatch(Rectangle _rectangle)
        {
            rectangle = _rectangle;
        }

        /// <summary>
        /// The rectangle that corresponds to the SearchFor image.
        /// </summary>
        private Rectangle rectangle { get; set; }


        /// <summary>
        /// The Rectangle that is used when issuing an Action via an ActionTemplate, such as a mouse click.
        /// A random point within this rectangle is chosen.
        /// </summary>
        public Rectangle ActionRectangle { get; set; }

        public Rectangle GetRectangle()
        {
            return rectangle;
        }
    }
}