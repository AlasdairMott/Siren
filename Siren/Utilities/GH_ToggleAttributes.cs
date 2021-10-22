using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Siren.Utilities
{
    /// <summary>
    /// Draws icons inline and below the component; manages selection state and callbacks via icon index
    /// </summary>
    public class GH_ToggleAttributes : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        private Action<int> iconClickHander;
        public int IndexOfSelectedIcon { get; set; }
        private const int iconDimensions = 9; // Actually 24px; but larger when rendered (despite rect size being accurate?)
        private const int iconPadding = 2; // 24px base; including 2px minimum padding
        private const float unselectedOpacity = 0.35F;
        private const int iconOffset = 15;
        List<System.Drawing.Bitmap> iconImages;

        private System.Drawing.Rectangle iconStripBounds; // Overall icon area
        System.Drawing.Rectangle[] iconBounds; // Track per-icon boundaries to ID click events

        public GH_ToggleAttributes(GH_Component owner, Action<int> callback, 
                               List<System.Drawing.Bitmap> icons, int activeIndex) : base(owner)
        {
            this.IndexOfSelectedIcon = activeIndex;
            this.iconClickHander = callback;
            this.iconImages = icons;
            this.iconBounds = new System.Drawing.Rectangle[icons.Count];
        }

        protected override void Layout()
        {
            base.Layout();

            var componentRect = GH_Convert.ToRectangle(Bounds);
            var iconsRect = new Rectangle();
            iconsRect.Height = (iconDimensions + iconPadding * 2) * iconBounds.Length;
            iconsRect.Width = iconDimensions + iconPadding * 2;
            iconsRect.X = componentRect.X + componentRect.Width - (12 + iconsRect.Width);
            iconsRect.Y = componentRect.Y + (componentRect.Height - iconsRect.Height) / 2;

            iconStripBounds = iconsRect;
        }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != Grasshopper.GUI.Canvas.GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
            }

            RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true);
            var iconSpacing = GH_Convert.ToRectangle(iconStripBounds).Height / iconImages.Count;

            var iconBox = new Rectangle();
            iconBox.X = iconStripBounds.X + iconPadding - iconOffset;
            iconBox.Width = iconDimensions;
            iconBox.Height = iconDimensions;

            var toggleSize = new Size(iconStripBounds.Width, iconDimensions + iconPadding * 2);
            var toggleLocation = new Point(iconStripBounds.Location.X, iconStripBounds.Location.Y + IndexOfSelectedIcon * iconSpacing);
            var toggle = new Rectangle(toggleLocation, toggleSize);
            
            DrawToggle(graphics, iconStripBounds, toggle);

            for (var i = 0; i < iconImages.Count; i++)
            {
                iconBox.Y = iconStripBounds.Y + iconPadding + (i * iconSpacing);
                iconBounds[i] = iconBox;
                var imageForState = GetImageForState(iconImages[i], i == IndexOfSelectedIcon);
                graphics.DrawImage(imageForState, iconBounds[i]);
            }

        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return base.RespondToMouseDown(sender, e);
             
            for (var i = 0; i < iconImages.Count; i++)
            {
                System.Drawing.RectangleF iconRec = new System.Drawing.RectangleF(iconBounds[i].Location.X + iconOffset, iconBounds[i].Location.Y, iconStripBounds.Width, iconDimensions + iconPadding * 2); /*(iconBounds[i]);*/

                if (iconRec.Contains(e.CanvasLocation))
                {
                    this.IndexOfSelectedIcon = i;
                    this.iconClickHander(i);
                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }
        private System.Drawing.Bitmap GetImageForState(System.Drawing.Bitmap image, bool isSelected)
        {
            if (isSelected)
                return image;

            // Thanks Jack Marchetti, https://stackoverflow.com/a/2201233
            var colorMatrix = new ColorMatrix();
            colorMatrix.Matrix33 = unselectedOpacity;

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

        private void DrawToggle(System.Drawing.Graphics graphics, Rectangle bounds, Rectangle toggle)
        {
            using (var brush = new SolidBrush(Color.FromArgb(30, 30, 30)))
            using (var penDark = new Pen(Color.Black, 6.6f) { LineJoin = LineJoin.Round})
            {
                var rectangleSmall = bounds;
                rectangleSmall.Inflate(-2, -2);
                graphics.DrawRectangle(penDark, rectangleSmall); //use this inset thick line to get rounder edges
                graphics.FillRectangle(brush, bounds);
            }

            using (var brush = new SolidBrush(Color.FromArgb(150, 150, 150)))
            using (var penLight = new Pen(Color.FromArgb(10, 220, 220, 220), 2f) { LineJoin = LineJoin.Round, Alignment = PenAlignment.Inset })
            using (var penMid = new Pen(Color.FromArgb(70, 70, 70), 1.4f))
            using (var penDark = new Pen(Color.Black, 1.0f) { LineJoin = LineJoin.Round})
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
