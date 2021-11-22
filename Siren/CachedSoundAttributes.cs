using System.Drawing;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using System.Linq;
using Grasshopper.GUI;

namespace Siren
{
    public class CachedSoundAttributes : GH_FloatingParamAttributes
    {
        private readonly CachedSoundParameter _owner;

        private RectangleF _imageRectangle;

        public CachedSoundAttributes(CachedSoundParameter owner) : base(owner)
        {
            _owner = owner;
        }

        protected override void Layout()
        {
            base.Layout();

            if (_owner != null && _owner.Modified == true)
            {
                var rect = Bounds;
                rect.X -= 22;
                rect.Width += 22;
                Bounds = rect;
            }
        }

        protected override void PrepareForRender(GH_Canvas canvas)
        {
            _imageRectangle = Bounds;
            _imageRectangle.Width = _imageRectangle.Height = 15;
            _imageRectangle.X += 6;
            _imageRectangle.Y += 4;

            base.PrepareForRender(canvas);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (_owner.Modified == true)
            {
                GH_GraphicsUtil.RenderIcon(graphics, _imageRectangle, Properties.Resources.multiplication);
            }
        }
    }
}
