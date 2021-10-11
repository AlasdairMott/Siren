using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Drawing;
using System.Timers;

namespace Siren
{
    public class AudioOutComponent : GH_Component
	{
        private readonly WaveOut waveOut;
        public bool waveIsPlaying = false; 
        public Rhino.Geometry.Interval playState; // Form of (currentTime, totalTime)
        public readonly int tickRate = 100; // playStateTimer duration, e.g. playhead update rate (in ms)
        public double defaultLatency;

        public CachedSound Wave { get; private set; }
        public MixingSampleProvider Mixer { get; private set; }
        public float Volume { get; set; }

        /// <summary>
        /// Initializes a new instance of the AudioOutComponent class.
        /// </summary>
        public AudioOutComponent()
		  : base("Audio Out", "AOut",
			  "Allows a signal to be played within Grasshopper.",
              "Siren", "Utilities")
		{
            waveOut = new WaveOut();
            Mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SirenSettings.SampleRate, 1));
            Mixer.ReadFully = true;
            waveOut.Init(Mixer);
            defaultLatency = waveOut.DesiredLatency / 1000f;

            Wave = CachedSound.Empty;
            Volume = 1.0f;
        }

		public override void CreateAttributes()
		{
			m_attributes = new CustomGHButton(this);
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
            pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
        }

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntervalParameter("Play Progress", "P", "The play progress, in seconds, of the sample", GH_ParamAccess.item);
        }

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
            if (waveIsPlaying && playState != null)
            {
                var playedProgress = waveOut.GetPosition() / Wave.Length; // Bytes played vs bytes total
                playState.T0 = playedProgress * playState.T1; // %played back to seconds

                if (playState.T0 > playState.T1)
                {
                    waveIsPlaying = false;
                    playState.T0 = 0.0;
                }
                else
                    OnPingDocument()?.ScheduleSolution(tickRate, TriggerPlayheadUpdate);

                DA.SetData(0, playState);
                return; // Skip rest of solve
            }

            var waveIn = CachedSound.Empty;
            if (!DA.GetData(0, ref waveIn)) return;

            Wave = waveIn;

            playState = new Rhino.Geometry.Interval(0.0, (double)waveIn.Length / waveIn.WaveFormat.SampleRate);
            DA.SetData(0, playState);
        }

        public void TriggerPlayheadUpdate(GH_Document gh)
        {
            this.ExpireSolution(false);
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            waveOut.Play();
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            base.RemovedFromDocument(document);
            waveOut.Stop();
            waveOut.Dispose();
        }
            
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.playback;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("55f99243-1902-4ae3-a1e4-b2041ac6abf1"); }
		}

        //protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        //{
        //    base.AppendAdditionalComponentMenuItems(menu);
            
        //}
    }

    public class CustomGHButton : GH_ComponentAttributes
    {
        private Bitmap icon;
        private int buttonOffset;
        private Rectangle button;
        private AudioOutComponent owner;
        
        public CustomGHButton(AudioOutComponent owner) : base(owner)
        {
            icon = Properties.Resources.playback_Off;
            this.owner = owner;
            buttonOffset = (Convert.ToInt32(Bounds.Height) - 24) / 2;
            //textBox = new RectangleF(Bounds.X + buttonOffset / 2, Bounds.Y, Bounds.Width - Bounds.Height, Bounds.Height);
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

            var componentInputs = (this.DocObject as IGH_Component).Params.Input;
            var inputAttributes = componentInputs[0].Attributes;
            var inputBounds = inputAttributes.Bounds;
            inputBounds.Y += 4;
            inputAttributes.Bounds = inputBounds;

            var componentOutputs = (this.DocObject as IGH_Component).Params.Output;
            var outputAttributes = componentOutputs[0].Attributes;
            var outputBounds = outputAttributes.Bounds;
            outputBounds.Y += 4;
            outputBounds.X += 28;
            outputAttributes.Bounds = outputBounds;
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel == GH_CanvasChannel.Wires || channel == GH_CanvasChannel.Overlay)
            {
                base.Render(canvas, graphics, channel);
            }

            if (channel == GH_CanvasChannel.Objects)
            {
                //Render input and output grip.
                GH_CapsuleRenderEngine.RenderInputGrip(graphics, canvas.Viewport.Zoom, InputGrip, true);
                GH_CapsuleRenderEngine.RenderOutputGrip(graphics, canvas.Viewport.Zoom, OutputGrip, true);

                // Updating the capsule rectangles
                buttonOffset = 0;//(Convert.ToInt32(Bounds.Height) - 24) / 2;
                var leftTextBox = new RectangleF(Bounds.X - 12/*+ buttonOffset / 2*/, Bounds.Y, Bounds.Width - Bounds.Height, Bounds.Height);
                var rightTextBox = new RectangleF(Bounds.X + Bounds.Width - 16, Bounds.Y, 10, Bounds.Height); 
                button = new Rectangle(Convert.ToInt32(Bounds.X) + Convert.ToInt32(Bounds.Width) - Convert.ToInt32(Bounds.Height) + buttonOffset,
                                       Convert.ToInt32(Bounds.Y) + buttonOffset,
                                       Convert.ToInt32(Bounds.Height) - (buttonOffset * 2),
                                       Convert.ToInt32(Bounds.Height) - (buttonOffset * 2));

                // Creating the capsules
                GH_Capsule outerLeftCapsule = GH_Capsule.CreateTextCapsule(Bounds, leftTextBox, GH_Palette.Black, "W");
                GH_Capsule buttonCapsule = GH_Capsule.CreateCapsule(button, GH_Palette.Transparent, 2, 2);
                //GH_Capsule outerRightCapsule = GH_Capsule.CreateTextCapsule(Bounds, rightTextBox, GH_Palette.Black, "P");

                // Rendering the capsules
                outerLeftCapsule.Render(graphics, Selected, Owner.Locked, true);
                buttonCapsule.Render(graphics, icon, Color.Transparent);
                //outerRightCapsule.Render(graphics, Selected, Owner.Locked, true);

                // Disposing of the capsules
                outerLeftCapsule.Dispose();
                buttonCapsule.Dispose();
                //outerRightCapsule.Dispose();
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            // Checking if it's a left click, and if it's in the button's area
            if (e.Button == System.Windows.Forms.MouseButtons.Left && ((RectangleF)button).Contains(e.CanvasLocation))
            {
                // Changing the image
                icon = Properties.Resources.playback_On;
            }

            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            // Checking if the left mouse button is the one being used
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Updating the icon
                icon = Properties.Resources.playback_Off;

                sender.ScheduleRegen(1);

                // Checking if it was clicked, and if it's in the right area
                if (!Owner.Locked && e.Clicks >= 1 && ((RectangleF)button).Contains(e.CanvasLocation))
                {
                    owner.Mixer.AddMixerInput(owner.Wave.ToSampleProvider());

                    owner.playState.T0 = owner.defaultLatency;
                    owner.waveIsPlaying = true;
                    owner.OnPingDocument()?.ScheduleSolution(owner.tickRate, owner.TriggerPlayheadUpdate);
                    //owner.Mixer.AddMixerInput(new SampleProviders.LoopStream(owner.Wave));
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }
}
