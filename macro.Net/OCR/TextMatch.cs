using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace macro.Net.OCR
{
    /// <summary>
    /// The result of an OCR (optical character recognition) text match.
    /// </summary>
    public class TextMatch
    {
        public TextMatch()
        {
            SearchText = "";
            MatchedText = "";
        }

        /// <summary>
        /// The text that was originally searched for
        /// </summary>
        public string SearchText { get; set; }

        /// <summary>
        /// The text that was found
        /// </summary>
        public string MatchedText { get; set; }

        /// <summary>
        /// The rectangle containing the matched text
        /// </summary>
        public Rectangle MatchRect { get; set; }

        public void SetMatchRect(Tesseract.Rect rect)
        {
            Rectangle newRect = new();
            newRect.X = rect.X1;
            newRect.Y = rect.Y1;
            newRect.Width = rect.Width;
            newRect.Height = rect.Height;
            MatchRect = newRect;
        }

        public StringComparison NativeComparisonMethod { get; set; }

        /// <summary>
        /// The Rectangle that is used when issuing an Action via an ActionTemplate, such as a mouse click.
        /// A random point within this rectangle is chosen.
        /// </summary>
        public Rectangle ActionRectangle { get; set; }

        // idea: develop a 'coarse' comparison method that matches strings even when some characters are missing or misidentified
    }

    /// <summary>
    /// Indicates whether there is an incomplete match, i.e.
    /// there might be more text to the right, left, or none at all (the match is complete).
    /// This is a nice-to-have and currently not implemented.
    /// </summary>
    public enum Incomplete : ushort
    {
        None = 0,
        ToTheRight = 1,
        ToTheLeft = 2
    }
}