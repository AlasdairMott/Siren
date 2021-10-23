using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace Siren.Utilities
{
	public class GH_KnobAttributes : GH_ComponentAttributes
	{
		//private float width = 70f;
		//private float height = 70f;
		private float knobDiameter = 36f;

		private float p0;
		private float p1;
		private float min = (float)-Math.PI * 0.75f;
		private float max = (float) Math.PI * 0.75f;

		private RectangleF knobBounds;
		private PointF canvasLocation;
		private Point  systemLocation;

		public GH_KnobAttributes(GH_Component owner) : base(owner) 
		{ 
		}

		protected override void Layout()
		{
			base.Layout();
			var bounds = new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
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
			DrawKnob(graphics, knobBounds, p1);
		}

		private void DrawKnob(Graphics graphics, RectangleF bounds, float angle)
		{
			var pt1 = new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
			var pt2 = new PointF(
				(float) Math.Cos(angle - Math.PI/2) * bounds.Width * 0.5f + pt1.X, 
				(float) Math.Sin(angle - Math.PI/2) * bounds.Width * 0.5f + pt1.Y);

			using (var whiteBrush = new SolidBrush(Color.White))
			using (var shadowBrush = new SolidBrush(Color.FromArgb(20,0,0,0)))
			using (var blackBrush = new SolidBrush(Color.Black))
			using (var grey = new Pen(Color.FromArgb(80, 255, 255, 255), 1f))
			using (var white = new Pen(Color.White, 3f))
			{
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

			p0 = p1;
			canvas.MouseMove -= Sender_MouseMove;
			canvas.MouseUp -= Sender_MouseUp;

			System.Windows.Forms.Cursor.Show();
			System.Windows.Forms.Cursor.Position = systemLocation;
		}

		private void Sender_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			var canvas = sender as GH_Canvas;
			var dp = (canvasLocation.Y - canvas.CursorCanvasPosition.Y) * 0.01f;

			p1 = SirenUtilities.Clamp(p0 + dp, min, max);

			ExpireLayout();
			canvas.Refresh();
		}

		public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
		{
			p0 = 0;
			return base.RespondToMouseDoubleClick(sender, e);
		}
	}
}

