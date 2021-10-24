using System;
using System.Drawing;

namespace Siren.Utilities
{
    public class GH_Knob
    {
        private readonly string text;

        private readonly Font font = new Font("Segoe UI", 2.5f);
        private readonly SolidBrush whiteBrush = new SolidBrush(Color.White);
        private readonly SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0));
        private readonly SolidBrush blackBrush = new SolidBrush(Color.Black);
        private readonly Pen grey = new Pen(Color.FromArgb(80, 255, 255, 255), 1f);
        private readonly Pen white = new Pen(Color.White, 3f);

        public GH_Knob(string text)
        {
            this.text = text;
        }

        public void Draw(Graphics graphics, RectangleF bounds, float angle)
        {
            var pt1 = new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
            var pt2 = new PointF(
                (float)Math.Cos(angle - Math.PI / 2) * bounds.Width * 0.5f + pt1.X,
                (float)Math.Sin(angle - Math.PI / 2) * bounds.Width * 0.5f + pt1.Y);

            var textLocation = pt1;
            textLocation.Y -= 28f;
            using (var format = new StringFormat() { Alignment = StringAlignment.Center })
            {
                graphics.DrawString(text, font, blackBrush, textLocation, format);
            }

            var shadow = bounds;
            shadow.Inflate(1f, 1f);
            shadow.Y += 2f;

            graphics.FillEllipse(shadowBrush, shadow);
            graphics.FillEllipse(blackBrush, bounds);

            bounds.Inflate(-4f, -4f);
            graphics.DrawEllipse(grey, bounds);

            bounds.Inflate(-4f, -4f);
            graphics.FillEllipse(whiteBrush, bounds);

            graphics.DrawLine(white, pt1, pt2);
        }
    }
}
