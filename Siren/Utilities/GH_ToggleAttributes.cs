using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace Siren.Utilities
{
    /// <summary>
    /// Draws icons inline and below the component; manages selection state and callbacks via icon index
    /// </summary>
    public class GH_ToggleAttributes : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        private readonly Action<int> _iconClickHander;
        private const int IconDimensions = 9; // Actually 24px; but larger when rendered (despite rect size being accurate?)
        private const int IconPadding = 2; // 24px base; including 2px minimum padding
        private const float UnselectedOpacity = 0.35F;
        private const int IconOffset = 15;
        private readonly List<Bitmap> _iconImages;
        private Rectangle _iconStripBounds; // Overall icon area
        private readonly Rectangle[] _iconBounds; // Track per-icon boundaries to ID click events

        public int IndexOfSelectedIcon { get; set; }

        public GH_ToggleAttributes(GH_Component owner, Action<int> callback,
                               List<Bitmap> icons, int activeIndex) : base(owner)
        {
            IndexOfSelectedIcon = activeIndex;
            _iconClickHander = callback;
            _iconImages = icons;
            _iconBounds = new Rectangle[icons.Count];
        }

        protected override void Layout()
        {
            base.Layout();

            var componentRect = GH_Convert.ToRectangle(Bounds);
            var iconsRect = new Rectangle
            {
                Height = (IconDimensions + IconPadding * 2) * _iconBounds.Length,
                Width = IconDimensions + IconPadding * 2
            };
            iconsRect.X = componentRect.X + componentRect.Width - (12 + iconsRect.Width);
            iconsRect.Y = componentRect.Y + (componentRect.Height - iconsRect.Height) / 2;

            _iconStripBounds = iconsRect;
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
            }

            RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true);
            var iconSpacing = GH_Convert.ToRectangle(_iconStripBounds).Height / _iconImages.Count;

            var iconBox = new Rectangle
            {
                X = _iconStripBounds.X + IconPadding - IconOffset,
                Width = IconDimensions,
                Height = IconDimensions
            };

            var toggleSize = new Size(_iconStripBounds.Width, IconDimensions + IconPadding * 2);
            var toggleLocation = new Point(_iconStripBounds.Location.X, _iconStripBounds.Location.Y + IndexOfSelectedIcon * iconSpacing);
            var toggle = new Rectangle(toggleLocation, toggleSize);

            DrawToggle(graphics, _iconStripBounds, toggle);

            for (var i = 0; i < _iconImages.Count; i++)
            {
                iconBox.Y = _iconStripBounds.Y + IconPadding + (i * iconSpacing);
                _iconBounds[i] = iconBox;
                var imageForState = GetImageForState(_iconImages[i], i == IndexOfSelectedIcon);
                graphics.DrawImage(imageForState, _iconBounds[i]);
            }

        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return base.RespondToMouseDown(sender, e);

            for (var i = 0; i < _iconImages.Count; i++)
            {
                var iconRec = new RectangleF(_iconBounds[i].Location.X + IconOffset, _iconBounds[i].Location.Y, _iconStripBounds.Width, IconDimensions + IconPadding * 2); /*(iconBounds[i]);*/

                if (iconRec.Contains(e.CanvasLocation))
                {
                    IndexOfSelectedIcon = i;
                    _iconClickHander(i);
                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }
        private Bitmap GetImageForState(Bitmap image, bool isSelected)
        {
            if (isSelected)
                return image;

            // Thanks Jack Marchetti, https://stackoverflow.com/a/2201233
            var colorMatrix = new ColorMatrix
            {
                Matrix33 = UnselectedOpacity
            };

            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            var output = new Bitmap(image.Width, image.Height);
            using (var gfx = Graphics.FromImage(output))
            {
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                                                   0, 0, image.Width, image.Height,
                                                   GraphicsUnit.Pixel, imageAttributes);
            }
            return output;
        }

        private void DrawToggle(Graphics graphics, Rectangle bounds, Rectangle toggle)
        {
            using (var brush = new SolidBrush(Color.FromArgb(30, 30, 30)))
            using (var penDark = new Pen(Color.Black, 6.6f) { LineJoin = LineJoin.Round })
            {
                var rectangleSmall = bounds;
                rectangleSmall.Inflate(-2, -2);
                graphics.DrawRectangle(penDark, rectangleSmall); //use this inset thick line to get rounder edges
                graphics.FillRectangle(brush, bounds);
            }

            using (var brush = new SolidBrush(Color.FromArgb(150, 150, 150)))
            using (var penLight = new Pen(Color.FromArgb(10, 220, 220, 220), 2f) { LineJoin = LineJoin.Round, Alignment = PenAlignment.Inset })
            using (var penMid = new Pen(Color.FromArgb(70, 70, 70), 1.4f))
            using (var penDark = new Pen(Color.Black, 1.0f) { LineJoin = LineJoin.Round })
            {
                graphics.FillRectangle(brush, toggle);
                for (int i = 1; i < toggle.Height / 2; i++)
                {
                    graphics.DrawLine(penMid, new PointF(toggle.X, -0.3f + toggle.Y + 2.55f * i),
                        new PointF(toggle.X + toggle.Width, -0.3f + toggle.Y + 2.55f * i));
                }

                graphics.DrawRectangle(penLight, bounds);
                graphics.DrawRectangle(penDark, bounds);
            }

        }

    }
}
