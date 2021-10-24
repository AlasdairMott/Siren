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
        private float _width;
        private float _height;
        private readonly float _knobDiameter = 36f;

        private float p0;
        public readonly float Min = (float)-Math.PI * 0.75f;
        public readonly float Max = (float)Math.PI * 0.75f;

        private readonly GH_Knob _knob;
        private RectangleF _knobBounds;
        private PointF _canvasLocation;
        private Point _systemLocation;

        public float P { get; set; }
        public bool Locked { get; set; }

        public GH_KnobAttributes(GH_Component owner, string text, float width = 0, float height = 0) : base(owner)
        {
            _knob = new GH_Knob(text);

            _width = width;
            _height = height;
        }

        protected override void Layout()
        {
            base.Layout();
            if (_width == 0) _width = Bounds.Width;
            if (_height == 0) _height = Bounds.Height;

            var bounds = new RectangleF(Bounds.X, Bounds.Y, _width, _height);
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

            _knobBounds = Bounds;
            _knobBounds.X += (Bounds.Width - _knobDiameter) * 0.5f;
            _knobBounds.Y += (Bounds.Height - _knobDiameter) * 0.5f;
            _knobBounds.Width = _knobBounds.Height = _knobDiameter;

            _knob.Draw(graphics, _knobBounds, P);
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (!Owner.Locked && !Locked && e.Button == System.Windows.Forms.MouseButtons.Left && _knobBounds.Contains(e.CanvasLocation))
            {
                p0 = P;

                sender.MouseMove += Sender_MouseMove;
                sender.MouseUp += Sender_MouseUp;

                _canvasLocation = e.CanvasLocation;
                _systemLocation = System.Windows.Forms.Cursor.Position;

                System.Windows.Forms.Cursor.Hide();

                return GH_ObjectResponse.Handled;
            }

            return base.RespondToMouseDown(sender, e);
        }

        private void Sender_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var canvas = sender as GH_Canvas;

            canvas.MouseMove -= Sender_MouseMove;
            canvas.MouseUp -= Sender_MouseUp;

            System.Windows.Forms.Cursor.Show();
            System.Windows.Forms.Cursor.Position = _systemLocation;

            Owner.ExpireSolution(true);
        }

        private void Sender_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var canvas = sender as GH_Canvas;
            var dp = (_canvasLocation.Y - canvas.CursorCanvasPosition.Y) * 0.01f;

            P = SirenUtilities.Clamp(p0 + dp, Min, Max);

            ExpireLayout();
            canvas.Refresh();
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (!Owner.Locked && !Locked && e.Button == System.Windows.Forms.MouseButtons.Left && _knobBounds.Contains(e.CanvasLocation))
            {
                P = p0 = 0;

                ExpireLayout();
                sender.Refresh();
                Owner.ExpireSolution(true);

                return GH_ObjectResponse.Handled;
            }

            return base.RespondToMouseDoubleClick(sender, e);
        }
    }
}

