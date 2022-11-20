using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.Screen
{
    public class ScreenImageTile
    {
        public ScreenImageTile()
        {
        }
        public byte[] Image { get; set; }

        public int anchor_y { get; set; }
    }
}