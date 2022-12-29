using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using System.Windows.Forms;

namespace macro.Net.Screen
{
    /*
     * g.FillRectangle should not be used because the drawn rectangle can not be removed
     * Application.Run(form) should not be used because it has a minimum size
     */
    internal class Paint
    {
        public Paint(double _opacity, int _draw_duration_ms)
        {
            Opacity = _opacity;
            DrawDurationMs = _draw_duration_ms;

            LineWidthForContainingRectangle = 5;
            DrawCenterForm = false;
        }

        /// <summary>
        /// The opacity of the rectangles. A higher value means it will be more visible but less transparent.
        /// </summary>
        private double Opacity {  get; set; }

        public int DrawDurationMs { get; set; }

        public int LineWidthForContainingRectangle { get; set; }

        /// <summary>
        /// Whether to fill the original rectangle (true) or to only draw around it (false).
        /// False is the default.
        /// </summary>
        public bool DrawCenterForm { get; set; }

        public async Task DrawContainingRectangle(Rectangle rectangle)
        {
            Task.Run(() => T_DrawContainingRectangle(rectangle));
        }

        /// <summary>
        /// Draws 4 rectangles around the input rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle around which to draw</param>
        /// <returns></returns>
        private async Task T_DrawContainingRectangle(Rectangle rectangle)
        {
            int line_width = LineWidthForContainingRectangle;
            Rectangle top_rect = new Rectangle(rectangle.X, rectangle.Y - line_width, rectangle.Width, line_width);
            Rectangle bottom_rect = new Rectangle(rectangle.X, rectangle.Y + rectangle.Height, rectangle.Width, line_width);
            Rectangle right_rect = new Rectangle(rectangle.X + rectangle.Width, rectangle.Y - line_width, line_width, rectangle.Height + line_width * 2);
            Rectangle left_rect = new Rectangle(rectangle.X - line_width, rectangle.Y - line_width, line_width, rectangle.Height + line_width * 2);
            Form center_form = null;
            Form top_form = await DrawFormAsync(Color.Green, top_rect);
            Form bottom_form = await DrawFormAsync(Color.Green, bottom_rect);
            Form right_form = await DrawFormAsync(Color.Green, right_rect);
            Form left_form = await DrawFormAsync(Color.Green, left_rect);
            if(DrawCenterForm)
            {
                center_form = await DrawFormAsync(Color.Blue, rectangle);
            }

            Task.Delay(DrawDurationMs).Wait();

            DisableForm(top_form);
            DisableForm(bottom_form);
            DisableForm(right_form);
            DisableForm(left_form);
            if(center_form != null)
            {
                DisableForm(center_form);
            }
        }

        /// <summary>
        /// Draws a form, resizes and recolors it to match the parameters
        /// </summary>
        /// <param name="color">The fill-color of the form</param>
        /// <param name="rectangle">The rectangle specifying where the form should be drawn</param>
        /// <returns></returns>
        private async Task<Form> DrawFormAsync(Color color, Rectangle rectangle)
        {
            Form form = new Form();
            form.BackColor = color;
            form.FormBorderStyle = FormBorderStyle.None;
            form.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            //Rectangle position = TranslatePosition(rectangle, form);
            //form.SetBounds(position.X, position.Y, position.Width, position.Height);
            form.TopMost = true;
            form.Opacity = Opacity;
            form.Visible = false;
            form.ShowInTaskbar = false;
            form.ShowIcon = false;
            form.UseWaitCursor = false;
            form.Show();
            form.SetDesktopBounds(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            form.Visible = true;
            return form;
        }

        private void DisableForm (Form f)
        {
            f.Enabled = false;
            f.Dispose();
            f.Hide();
        }
    }
}