using System;
using System.Drawing;

namespace Siren.Utilities
{
    public class GH_Knob
    {
        private readonly string _text;

        private readonly Font _font = new Font("Segoe UI", 2.5f);
        private readonly SolidBrush _whiteBrush = new SolidBrush(Color.White);
        private readonly SolidBrush _shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0));
        private readonly SolidBrush _blackBrush = new SolidBrush(Color.Black);
        private readonly Pen _grey = new Pen(Color.FromArgb(80, 255, 255, 255), 1f);
        private readonly Pen _white = new Pen(Color.White, 3f);

        public GH_Knob(string text)
        {
            _text = text;
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
                graphics.DrawString(_text, _font, _blackBrush, textLocation, format);
            }

            var shadow = bounds;
            shadow.Inflate(1f, 1f);
            shadow.Y += 2f;

            graphics.FillEllipse(_shadowBrush, shadow);
            graphics.FillEllipse(_blackBrush, bounds);

            bounds.Inflate(-4f, -4f);
            graphics.DrawEllipse(_grey, bounds);

            bounds.Inflate(-4f, -4f);
            graphics.FillEllipse(_whiteBrush, bounds);

            graphics.DrawLine(_white, pt1, pt2);
        }

        public void DisposeGraphics()
        {
            _font.Dispose();
            _whiteBrush.Dispose();
            _shadowBrush.Dispose();
            _blackBrush.Dispose();
            _grey.Dispose();
            _white.Dispose();
        }
    }
}
