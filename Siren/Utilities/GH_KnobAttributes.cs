using System;
using System.Drawing;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace Siren.Utilities
{
    public class GH_KnobAttributes : GH_ComponentAttributes
    {
        private float width;
        private float height;
        private float knobDiameter = 36f;

        private float p0;
        public readonly float Min = (float)-Math.PI * 0.75f;
        public readonly float Max = (float)Math.PI * 0.75f;

        private GH_Knob knob;
        private RectangleF knobBounds;
        private PointF canvasLocation;
        private Point systemLocation;

        public float P { get; set; }

        public GH_KnobAttributes(GH_Component owner, string text, float width = 0, float height = 0) : base(owner)
        {
            knob = new GH_Knob(text);

            this.width = width;
            this.height = height;
        }

        protected override void Layout()
        {
            base.Layout();
            if (width == 0) width = Bounds.Width;
            if (height == 0) height = Bounds.Height;

            var bounds = new RectangleF(Bounds.X, Bounds.Y, width, height);
            LayoutInputParams(Owner, bounds);
            LayoutOutputParams(Owner, bounds);
            Bounds = LayoutBounds(Owner, bounds);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
            }

            RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true); // Standard UI

            knobBounds = Bounds;
            knobBounds.X += (Bounds.Width - knobDiameter) * 0.5f;
            knobBounds.Y += (Bounds.Height - knobDiameter) * 0.5f;
            knobBounds.Width = knobBounds.Height = knobDiameter;

            knob.Draw(graphics, knobBounds, P);
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (!Owner.Locked && e.Button == System.Windows.Forms.MouseButtons.Left && knobBounds.Contains(e.CanvasLocation))
            {
                sender.MouseMove += Sender_MouseMove;
                sender.MouseUp += Sender_MouseUp;

                canvasLocation = e.CanvasLocation;
                systemLocation = System.Windows.Forms.Cursor.Position;

                System.Windows.Forms.Cursor.Hide();

                return GH_ObjectResponse.Handled;
            }

            return base.RespondToMouseDown(sender, e);
        }

        private void Sender_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var canvas = sender as GH_Canvas;

            p0 = P;
            canvas.MouseMove -= Sender_MouseMove;
            canvas.MouseUp -= Sender_MouseUp;

            System.Windows.Forms.Cursor.Show();
            System.Windows.Forms.Cursor.Position = systemLocation;

            Owner.ExpireSolution(true);
        }

        private void Sender_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var canvas = sender as GH_Canvas;
            var dp = (canvasLocation.Y - canvas.CursorCanvasPosition.Y) * 0.01f;

            P = SirenUtilities.Clamp(p0 + dp, Min, Max);

            ExpireLayout();
            canvas.Refresh();
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (!Owner.Locked && e.Button == System.Windows.Forms.MouseButtons.Left && knobBounds.Contains(e.CanvasLocation))
            {
                P = 0;

                ExpireLayout();
                sender.Refresh();
                Owner.ExpireSolution(true);

                return GH_ObjectResponse.Handled;
            }

            return base.RespondToMouseDoubleClick(sender, e);
        }
    }
}

