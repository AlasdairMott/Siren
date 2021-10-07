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
    public class InlineIconStrip : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        private Action<int> iconClickHander;
        private int indexOfSelectedIcon;
        private const int iconDimensions = 18; // Actually 24px; but larger when rendered (despite rect size being accurate?)
        private const int iconPadding = 4; // 24px base; including 2px minimum padding
        private const float unselectedOpacity = 0.35F;
        List<System.Drawing.Bitmap> iconImages;

        private System.Drawing.Rectangle iconStripBounds; // Overall icon area
        System.Drawing.Rectangle[] iconBounds; // Track per-icon boundaries to ID click events

        public InlineIconStrip(GH_Component owner, Action<int> callback, 
                               List<System.Drawing.Bitmap> icons, int activeIndex) : base(owner)
        {
            this.indexOfSelectedIcon = activeIndex;
            this.iconClickHander = callback;
            this.iconImages = icons;
            this.iconBounds = new System.Drawing.Rectangle[icons.Count];
        }

        private int StripMinimumWidth => (iconDimensions + iconPadding * 3) * iconImages.Count;

        protected override void Layout()
        {
            base.Layout();

            System.Drawing.Rectangle componentRect = GH_Convert.ToRectangle(Bounds);
            componentRect.Height += iconDimensions + iconPadding * 2;

            if (componentRect.Width < StripMinimumWidth)
                componentRect.Width = StripMinimumWidth; // Widen component to fit icons

            System.Drawing.Rectangle iconsRect = componentRect;
            iconsRect.Height = iconDimensions + iconPadding * 2;
            iconsRect.Width -= iconPadding;
            iconsRect.X += iconPadding * 2;
            iconsRect.Y = componentRect.Y + componentRect.Height - iconDimensions - iconPadding;

            Bounds = componentRect;
            iconStripBounds = iconsRect;
        }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);
            var iconSpacing = GH_Convert.ToRectangle(iconStripBounds).Width / iconImages.Count; 

            if (channel == GH_CanvasChannel.Objects)
            {
                var iconBox = iconStripBounds;
                iconBox.Width = iconDimensions;
                iconBox.Height = iconDimensions;

                for (var i = 0; i < iconImages.Count; i++)
                {
                    iconBox.X = iconStripBounds.X + (i * iconSpacing);
                    iconBounds[i] = iconBox;
                    var imageForState = GetImageForState(iconImages[i], i == indexOfSelectedIcon);
                    graphics.DrawImage(imageForState, iconBounds[i]);
                }
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return base.RespondToMouseDown(sender, e);
             
            for (var i = 0; i < iconImages.Count; i++)
            {
                System.Drawing.RectangleF iconRec = iconBounds[i];
                if (iconRec.Contains(e.CanvasLocation))
                {
                    this.indexOfSelectedIcon = i;
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

    }
}
