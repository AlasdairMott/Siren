using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Siren
{
    public class AudioOutComponent : GH_Component
	{
        public readonly WaveOut waveOut;
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
                var mixerWave = (m_attributes as CustomGHButton).playingWave;
                playState.T0 = mixerWave.CurrentTime.TotalSeconds;

                if (playState.T0 + 0.099 > playState.T1) // 99 because currentTime ~0.001 less than total
                {
                    waveIsPlaying = false;
                    playState.T0 = 0.0;
                    (m_attributes as CustomGHButton).playIcon = Properties.Resources.playback_On;
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
    }

    public class CustomGHButton : GH_ComponentAttributes
    {
        public Bitmap playIcon;
        private int iconSize = 24;
        private int iconPadding = 4;
        private RectangleF playButtonBounds;
        private AudioOutComponent owner; 
        public WaveStream playingWave;

        public CustomGHButton(AudioOutComponent owner) : base(owner)
        {
            playIcon = Properties.Resources.playback_On;
            this.owner = owner;
        }

        protected override void Layout()
        {
            Pivot = GH_Convert.ToPoint(Pivot);

            playButtonBounds = new RectangleF(Pivot.X, Pivot.Y, iconSize + iconPadding, iconSize + iconPadding);
            LayoutInputParams(Owner, playButtonBounds);
            LayoutOutputParams(Owner, playButtonBounds);
            Bounds = LayoutBounds(Owner, playButtonBounds);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != Grasshopper.GUI.Canvas.GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
            }
            RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true); // Standard UI

            Rectangle capsuleMiddle = GH_Convert.ToRectangle(playButtonBounds); // Icon inset space
            capsuleMiddle.Inflate(-3, -3);

            using (var brush = new SolidBrush(Color.FromArgb(30, 30, 30)))
            using (var penDark = new Pen(Color.Black, 6.6f) { LineJoin = LineJoin.Round })
            {
                var rectangleSmall = capsuleMiddle;
                rectangleSmall.Inflate(-2, -2);
                graphics.DrawRectangle(penDark, rectangleSmall); //use this inset thick line to get rounder edges
                graphics.FillRectangle(brush, capsuleMiddle);
            }

            // Make space for icon and draw it
            capsuleMiddle.Width = iconSize;
            capsuleMiddle.Height = iconSize;
            graphics.DrawImage(playIcon, capsuleMiddle);
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            // Checking if it's a left click, and if it's in the button's area
            if (e.Button == System.Windows.Forms.MouseButtons.Left && ((RectangleF)playButtonBounds).Contains(e.CanvasLocation))
            {
                // Changing the image
                playIcon = Properties.Resources.playback_On;
            }

            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            // Checking if the left mouse button is the one being used
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                sender.ScheduleRegen(1);

                // Checking if it was clicked, and if it's in the right area
                if (!Owner.Locked && e.Clicks >= 1 && ((RectangleF)playButtonBounds).Contains(e.CanvasLocation))
                {
                    if (!owner.waveIsPlaying) // Start playing
                    {
                        this.playingWave = owner.Wave.ToRawSourceWaveStream();
                        owner.Mixer.AddMixerInput(this.playingWave);

                        owner.playState.T0 = owner.defaultLatency;
                        owner.waveIsPlaying = true;
                        playIcon = Properties.Resources.playback_Off;
                        owner.OnPingDocument()?.ScheduleSolution(owner.tickRate, owner.TriggerPlayheadUpdate);
                    }
                    else // Stop playing
                    {
                        playIcon = Properties.Resources.playback_On;
                        owner.waveIsPlaying = false;
                        // TODO: properly reset playing position of mixer or wave
                    }
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }
}
