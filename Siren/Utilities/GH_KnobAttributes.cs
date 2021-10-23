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
		private float width = 70f;
		//private float height = 70f;
		private float knobDiameter = 36f;
		private float p0;
		private float p1;
		private bool lowerKnob;
		private RectangleF knobBounds;
		private PointF mouseLocation;
		private PointF debugLocation;

		public GH_KnobAttributes(GH_Component owner) : base(owner) 
		{ 
		}

		protected override void Layout()
		{
			base.Layout();
			var bounds = new RectangleF(Bounds.X, Bounds.Y, width, Bounds.Height);
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

			using (var pen = new Pen(Color.Cyan, 3f))
			{
				graphics.DrawLine(pen, mouseLocation, debugLocation);
			}
		}

		private void DrawKnob(Graphics graphics, RectangleF bounds, float angle)
		{
			var pt1 = new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
			var pt2 = new PointF(
				(float) Math.Cos(angle) * bounds.Width * 0.5f + pt1.X, 
				(float) Math.Sin(angle) * bounds.Width * 0.5f + pt1.Y);

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
				//if (e.CanvasLocation.Y > (knobBounds.Y + knobBounds.Height) * 0.5f) lowerKnob = true;
				//else lowerKnob = false;

				sender.MouseMove += Sender_MouseMove;
				sender.MouseUp += Sender_MouseUp;

				mouseLocation = e.CanvasLocation;
				return GH_ObjectResponse.Handled;
			}

			return base.RespondToMouseDown(sender, e);
		}

		private void Sender_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			p0 = p1;
			(sender as GH_Canvas).MouseMove -= Sender_MouseMove;
			(sender as GH_Canvas).MouseUp -= Sender_MouseUp;
		}

		private void Sender_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			var dp = (mouseLocation.Y - e.Location.Y) * 0.01f;
			//if (lowerKnob) dp *= -1;
			p1 = p0 + dp;

			debugLocation = e.Location;
			debugLocation = (sender as GH_Canvas).CursorCanvasPosition;

			ExpireLayout();
			(sender as GH_Canvas).Refresh();
		}

		public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
		{
			p0 = 0;
			return base.RespondToMouseDoubleClick(sender, e);
		}
	}
}

