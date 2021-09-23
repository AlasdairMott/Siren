using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Attributes;
using System;
using System.Drawing;

namespace GHSynth
{
    public class SamplerPlayerComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the SamplerPlayerComponent class.
		/// </summary>
		public SamplerPlayerComponent()
		  : base("SamplerPlayerComponent", "Nickname",
			  "Description",
              "GHSynth", "Subcategory")
		{
		}

		public override void CreateAttributes()
		{
			m_attributes = new RoundAttributes(this);
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				//You can add image files to your project resources and access them like this:
				// return Resources.IconForThisComponent;
				return null;
			}
		}

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("55f99243-1902-4ae3-a1e4-b2041ac6abf1"); }
		}


        public class RoundAttributes : GH_ComponentAttributes
        {
            private Bitmap icon;
            private int buttonOffset;
            private Rectangle button;
            private RectangleF textBox;
            private SamplerPlayerComponent owner;

            public RoundAttributes(SamplerPlayerComponent owner) : base(owner)
            {
                icon = Properties.Resources.metronome;
                this.owner = owner;
                buttonOffset = (Convert.ToInt32(Bounds.Height) - 24) / 2;
                textBox = new RectangleF(Bounds.X + buttonOffset / 2, Bounds.Y, Bounds.Width - Bounds.Height, Bounds.Height);
                button = new Rectangle(Convert.ToInt32(Bounds.X) + Convert.ToInt32(Bounds.Width) - Convert.ToInt32(Bounds.Height) + buttonOffset,
                                       Convert.ToInt32(Bounds.Y) + buttonOffset,
                                       Convert.ToInt32(Bounds.Height) - (buttonOffset * 2),
                                       Convert.ToInt32(Bounds.Height) - (buttonOffset * 2));
            }

            protected override void Layout()
            {
                base.Layout();
                RectangleF updatedBounds = Bounds;
                updatedBounds.Width = 96;
                updatedBounds.Height = 36;
                Bounds = updatedBounds;
            }

            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                if (channel == GH_CanvasChannel.Wires || channel == GH_CanvasChannel.Overlay)
                {
                    base.Render(canvas, graphics, channel);
                }

                if (channel == GH_CanvasChannel.Objects)
                {
                    //Render output grip.
                    GH_CapsuleRenderEngine.RenderInputGrip(graphics, canvas.Viewport.Zoom, InputGrip, true);

                    // Updating the capsule rectangles
                    buttonOffset = (Convert.ToInt32(Bounds.Height) - 24) / 2;
                    textBox = new RectangleF(Bounds.X + buttonOffset / 2, Bounds.Y, Bounds.Width - Bounds.Height, Bounds.Height);
                    button = new Rectangle(Convert.ToInt32(Bounds.X) + Convert.ToInt32(Bounds.Width) - Convert.ToInt32(Bounds.Height) + buttonOffset,
                                           Convert.ToInt32(Bounds.Y) + buttonOffset,
                                           Convert.ToInt32(Bounds.Height) - (buttonOffset * 2),
                                           Convert.ToInt32(Bounds.Height) - (buttonOffset * 2));

                    // Creating the capsules
                    GH_Capsule outerCapsule = GH_Capsule.CreateTextCapsule(Bounds, textBox, GH_Palette.Normal, "Play sample");
                    GH_Capsule buttonCapsule = GH_Capsule.CreateCapsule(button, GH_Palette.Transparent);

                    // Rendering the capsules
                    outerCapsule.Render(graphics, Selected, Owner.Locked, true);
                    buttonCapsule.Render(graphics, icon, new GH_PaletteStyle(Color.White));

                    // Disposing of the capsules
                    outerCapsule.Dispose();
                    buttonCapsule.Dispose();
                }
            }

            public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                // Checking if it's a left click, and if it's in the button's area
                if (e.Button == System.Windows.Forms.MouseButtons.Left && ((RectangleF)button).Contains(e.CanvasLocation))
                {
                    // Changing the image
                    icon = Properties.Resources.quantize;
                }

                return base.RespondToMouseDown(sender, e);
            }

            public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                // Checking if the left mouse button is the one being used
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    // Updating the icon
                    icon = Properties.Resources.metronome;

                    sender.ScheduleRegen(1);

                    // Checking if it was clicked, and if it's in the right area
                    if (!Owner.Locked && e.Clicks >= 1 && ((RectangleF)button).Contains(e.CanvasLocation))
                    {
                        Rhino.RhinoApp.WriteLine("Pressed");
                    }
                }
                return base.RespondToMouseDown(sender, e);
            }
        }
    }
}