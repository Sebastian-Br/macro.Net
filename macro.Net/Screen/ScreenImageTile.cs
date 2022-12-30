using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.Screen
{
    /// <summary>
    /// A (horizontal) tile of the Screen is a subsection of the screen.
    /// </summary>
    public class ScreenImageTile
    {
        /// <summary>
        /// Constructs a (horizontal) tile of the Screen which is a subsection of the screen.
        /// </summary>
        /// <param name="_anchor_y">anchor_y is y coordinate on the screen where this tile starts. the x coordinate is always 0 and the width is always equal to the screen width.</param>
        public ScreenImageTile(int _anchor_y)
        {
            anchor_y = _anchor_y;
        }
        public byte[] Image { get; set; }

        /// <summary>
        /// anchor_y is y coordinate on the screen where this tile starts. the x coordinate is always 0 and the width is always equal to the screen width.
        /// How Tesseract figures out the height of the image is beyond me!; but it does.
        /// </summary>
        public int anchor_y { get; set; }
    }
}