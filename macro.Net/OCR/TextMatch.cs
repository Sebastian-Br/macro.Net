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
        /// At the moment, this is always equal to the SearchText if a match was found
        /// </summary>
        public string MatchedText { get; set; }

        /// <summary>
        /// The rectangle containing the matched text
        /// </summary>
        public Rectangle MatchRect { get; set; }

        /// <summary>
        /// Converts the Tesseract.Rect structure to a Rectangle object and sets the MatchRect property accordingly
        /// </summary>
        /// <param name="rect"></param>
        public void SetMatchRect(Tesseract.Rect rect)
        {
            Rectangle match_rectangle = new();
            match_rectangle.X = rect.X1;
            match_rectangle.Y = rect.Y1;
            match_rectangle.Width = rect.Width;
            match_rectangle.Height = rect.Height;
            MatchRect = match_rectangle;
        }

        /// <summary>
        /// The StringComparison method used to compare the results of the OCR with the SearchForWord
        /// </summary>
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