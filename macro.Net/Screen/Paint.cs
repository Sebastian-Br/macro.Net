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
            Draw_Duration = _draw_duration_ms;

            LineWidthForContainingRectangle = 5;
            DrawCenterForm = false;
        }
        private double Opacity {  get; set; }

        public int Draw_Duration { get; set; }

        public int LineWidthForContainingRectangle { get; set; }

        public bool DrawCenterForm { get; set; }

        public async Task DrawContainingRectangle(Rectangle rectangle)
        {
            Task.Run(() => T_DrawContainingRectangle(rectangle));
        }

        private async Task T_DrawContainingRectangle(Rectangle rectangle)
        {
            int lineWidth = LineWidthForContainingRectangle;
            Rectangle topRect = new Rectangle(rectangle.X, rectangle.Y - lineWidth, rectangle.Width, lineWidth);
            Rectangle bottomRect = new Rectangle(rectangle.X, rectangle.Y + rectangle.Height, rectangle.Width, lineWidth);
            Rectangle rightRect = new Rectangle(rectangle.X + rectangle.Width, rectangle.Y - lineWidth, lineWidth, rectangle.Height + lineWidth * 2);
            Rectangle leftRect = new Rectangle(rectangle.X - lineWidth, rectangle.Y - lineWidth, lineWidth, rectangle.Height + lineWidth * 2);
            Form centerForm = null;
            Form topForm = await DrawFormAsync(Color.Green, topRect);
            Form bottomForm = await DrawFormAsync(Color.Green, bottomRect);
            Form rightForm = await DrawFormAsync(Color.Green, rightRect);
            Form leftForm = await DrawFormAsync(Color.Green, leftRect);
            if(DrawCenterForm)
            {
                centerForm = await DrawFormAsync(Color.Blue, rectangle);
            }

            Task.Delay(Draw_Duration).Wait();

            DisableForm(topForm);
            DisableForm(bottomForm);
            DisableForm(rightForm);
            DisableForm(leftForm);
            if(centerForm != null)
            {
                DisableForm(centerForm);
            }
        }

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

        /* deprecated code
        /*public void DrawContainingRectangleOld(Rectangle rectangle)
        {
            int lineWidth = LineWidthForContainingRectangle;
            Rectangle topRect = new Rectangle(rectangle.X, rectangle.Y - lineWidth, rectangle.Width, lineWidth);
            Rectangle bottomRect = new Rectangle(rectangle.X, rectangle.Y + rectangle.Height, rectangle.Width, lineWidth);
            Rectangle rightRect = new Rectangle(rectangle.X + rectangle.Width, rectangle.Y - lineWidth, lineWidth, rectangle.Height + lineWidth * 2);
            Rectangle leftRect = new Rectangle(rectangle.X - lineWidth, rectangle.Y - lineWidth, lineWidth, rectangle.Height + lineWidth * 2);
            Task.Run(() => DrawFormAsyncOld(Color.Blue, rectangle)); //starting different tasks is probably suboptimal in terms of resources but still OK bcs only 4 tasks are started
            Task.Run(() => DrawFormAsyncOld(Color.Green, topRect));
            Task.Run(() => DrawFormAsyncOld(Color.Green, bottomRect));
            Task.Run(() => DrawFormAsyncOld(Color.Green, rightRect));
            DrawFormAsyncOld(Color.Green, leftRect);
        }

        private async Task DrawFormAsyncOld(Color color, Rectangle rectangle)
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
            form.Show();
            form.SetDesktopBounds(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            form.Visible = true;
            Task.Delay(DefaultDelay_Ms).Wait();
            DisableForm(form);
        }

        private Rectangle TranslatePosition(Rectangle rectangle, Form form)
        {
            Rectangle translatedPosition = new Rectangle();
            translatedPosition.Width = rectangle.Width;
            translatedPosition.Height = rectangle.Height;
            Point origPoint = new Point(rectangle.X, rectangle.Y);
            Point point = form.PointToClient(origPoint);
            translatedPosition.Location = point;
            return translatedPosition;
        }

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr dc);

        public void DrawAroundRect(Rect rectangle)
        {
        IntPtr desktopPtr = GetDC(IntPtr.Zero);
        Graphics g = Graphics.FromHdc(desktopPtr);
        SolidBrush b = new SolidBrush(Color.FromArgb(22, 0, 255, 0));
        g.FillRectangle(b, new Rectangle(0, 0, 500, 500));
        g.Dispose();
        ReleaseDC(desktopPtr);
        }*/
    }
}
