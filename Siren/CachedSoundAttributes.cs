using System.Drawing;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using System.Linq;

namespace Siren
{
    public class CachedSoundAttributes : GH_FloatingParamAttributes
    {
        public CachedSoundAttributes(CachedSoundParameter owner) : base(owner) { }

        protected override void Layout()
        {
            base.Layout();
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if ((Owner as CachedSoundParameter).Gain != 0)
            {
                graphics.DrawEllipse(Pens.White, Bounds);
            }
        }
    }
}
