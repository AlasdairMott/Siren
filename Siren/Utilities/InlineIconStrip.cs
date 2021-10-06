using System;
using System.Collections.Generic;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace Siren.Utilities
{
    public class InlineIconStrip : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        private Action<int> iconClickHander;
        private const int iconDimensions = 24; // 24px base; including 2px minimum padding
        private const int iconPadding = 4; // 24px base; including 2px minimum padding
        List<System.Drawing.Bitmap> iconImages;

        private System.Drawing.Rectangle iconStripBounds; // Overall icon area
        System.Drawing.Rectangle[] iconBounds; // Track per-icon boundaries to ID click events

        public InlineIconStrip(GH_Component owner, Action<int> callback, List<System.Drawing.Bitmap> icons) : base(owner)
        {
            this.iconClickHander = callback;
            this.iconImages = icons;
            this.iconBounds = new System.Drawing.Rectangle[icons.Count];
        }

        private int StripMinimumWidth => (iconDimensions * iconImages.Count) + (iconPadding * iconImages.Count);

        protected override void Layout()
        {
            base.Layout();

            System.Drawing.Rectangle componentRect = GH_Convert.ToRectangle(Bounds);
            componentRect.Height += iconDimensions;

            if (componentRect.Width < StripMinimumWidth)
                componentRect.Width = StripMinimumWidth; // Widen component to fit icons

            System.Drawing.Rectangle iconsRect = componentRect;
            iconsRect.Height = iconDimensions;
            iconsRect.Y = componentRect.Y + componentRect.Height - iconDimensions;
            iconsRect.Inflate(iconPadding * -1, iconPadding * -1); // Shrink button boundary so it isn't flush with component edges

            Bounds = componentRect;
            iconStripBounds = iconsRect;
        }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);
            var iconSpacing = GH_Convert.ToRectangle(Bounds).Width / iconImages.Count; 

            if (channel == GH_CanvasChannel.Objects)
            {
                var iconBox = iconStripBounds;
                iconBox.Width = iconDimensions;

                for (var i = 0; i < iconImages.Count; i++)
                {
                    iconBox.X = iconStripBounds.X + (i * iconSpacing);
                    iconBounds[i] = iconBox;

                    GH_Capsule icon = GH_Capsule.CreateCapsule(iconBox, GH_Palette.Normal);
                    icon.Render(graphics, Selected, Owner.Locked, false);
                    icon.Dispose();
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
                    this.iconClickHander(i);
                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }
    }
}
