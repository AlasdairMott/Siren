using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Siren.SampleProviders;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Siren
{
    public class AudioOutComponent : GH_Component
	{
        private readonly WaveOut waveOut;

        public bool WaveIsPlaying = false; 
        public Rhino.Geometry.Interval PlayState; // Form of (currentTime, totalTime)
        public readonly int TickRate = 100; // playStateTimer duration, e.g. playhead update rate (in ms)
        public double DefaultLatency;
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
            DefaultLatency = waveOut.DesiredLatency / 1000f;

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
            if (WaveIsPlaying && PlayState != null)
            {
                var mixerWave = (m_attributes as CustomGHButton).PlayingWave;
                PlayState.T0 = mixerWave.CurrentTime.TotalSeconds;

                if (PlayState.T0 + 0.099 > PlayState.T1) // 99 because currentTime ~0.001 less than total
                {
                    WaveIsPlaying = false;
                    PlayState.T0 = 0.0;
                }
                else
                    OnPingDocument()?.ScheduleSolution(TickRate, TriggerPlayheadUpdate);

                DA.SetData(0, PlayState);
                return; // Skip rest of solve
            }

            var waveIn = CachedSound.Empty;
            if (!DA.GetData(0, ref waveIn)) return;

            Wave = waveIn;

            PlayState = new Rhino.Geometry.Interval(0.0, (double)waveIn.Length / waveIn.WaveFormat.SampleRate);
            DA.SetData(0, PlayState);
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
        private int dragSpace = 15;
        private int buttonWidth = 46;
        private int componentHeight = 24;
        private RectangleF outerButtonBounds; // Includes draghandle space
        private Rectangle playButtonBounds; // Triggers click
        private AudioOutComponent owner;
        private bool aboutToPlay = false;

        public CachedSoundSampleProvider PlayingWave { get; private set; }
        
        public CustomGHButton(AudioOutComponent owner) : base(owner)
        {
            this.owner = owner;
        }

        protected override void Layout()
        {
            Pivot = GH_Convert.ToPoint(Pivot);

            outerButtonBounds = new RectangleF(Pivot.X, Pivot.Y, buttonWidth + dragSpace * 2, componentHeight); 
            LayoutInputParams(Owner, outerButtonBounds);
            LayoutOutputParams(Owner, outerButtonBounds);
            Bounds = LayoutBounds(Owner, outerButtonBounds);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != Grasshopper.GUI.Canvas.GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
            }
            RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true); // Standard UI

            playButtonBounds = GH_Convert.ToRectangle(outerButtonBounds); // Icon inset space
            playButtonBounds.X += dragSpace;
            playButtonBounds.Width = buttonWidth;

            // Black button background
            GH_Capsule button;
            if (aboutToPlay)
                button = GH_Capsule.CreateTextCapsule(playButtonBounds, playButtonBounds, GH_Palette.Grey, "", 1, 0);
            else
                button = GH_Capsule.CreateTextCapsule(playButtonBounds, playButtonBounds, GH_Palette.Black, "", 1, componentHeight / 2);

            button.Render(graphics, Selected, Owner.Locked, false);

            if (!owner.WaveIsPlaying)
                DrawPlayTriangle(graphics, playButtonBounds);
            else
                DrawStopSquare(graphics, playButtonBounds);
        }

        private void DrawStopSquare(Graphics graphics, Rectangle playButtonBounds)
        {
            using (var fill = new SolidBrush(Color.White))
            using (var outerstroke = new Pen(Color.Black, 4f) { LineJoin = LineJoin.Round })
            using (var innerstroke = new Pen(Color.LightGray, 2f) { LineJoin = LineJoin.Round }) 
            {
                var topLeft = new Point(playButtonBounds.X + 17, playButtonBounds.Y + 6);
                var square = new Rectangle(topLeft, new Size(12, 12));
                graphics.DrawRectangle(outerstroke, square);
                graphics.DrawRectangle(innerstroke, square);
                graphics.FillRectangle(fill, square);

                var gradientEnd = new Point(topLeft.X, topLeft.Y + 4);
                using (var highlight = new LinearGradientBrush(topLeft, gradientEnd, Color.LightGray, Color.Transparent))
                {
                    var highlightSquare = new Rectangle(topLeft, new Size(square.Width, 4));
                    graphics.FillRectangle(highlight, highlightSquare);
                }
            }
        }

        private void DrawPlayTriangle(Graphics graphics, Rectangle playButtonBounds)
        {
            using (var fill = new SolidBrush(Color.White))
            using (var outerstroke = new Pen(Color.Black, 4f) { LineJoin = LineJoin.Round })
            using (var innerstroke = new Pen(Color.LightGray, 2f) { LineJoin = LineJoin.Round })
            {
                int Xleft = playButtonBounds.X + 20;
                int YTop = playButtonBounds.Y + 7;
                int iconHeight = 10;
                Point[] trianglePts = new Point[] {
                    new Point(Xleft, YTop), // Top
                    new Point(Xleft + 7, YTop + iconHeight / 2), // Middle-Right
                    new Point(Xleft, YTop + iconHeight) // Bottom
                };
                graphics.DrawPolygon(outerstroke, trianglePts); // Black rim
                graphics.DrawPolygon(innerstroke, trianglePts); // Gray border
                graphics.FillPolygon(fill, trianglePts);

                var gradientEnd = new Point(Xleft, YTop + 5); // Stop-point of highlight
                using (var highlight = new LinearGradientBrush(trianglePts[0], gradientEnd, Color.LightGray, Color.Transparent))
                {
                    Point[] triangleHighlightPts = new Point[] {
                        trianglePts[0], // Top
                        new Point(Xleft + 7, gradientEnd.Y), // Middle-Right
                        gradientEnd // Bottom
                    };
                    graphics.FillPolygon(highlight, triangleHighlightPts); 
                }
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            // Checking if it's a left click, and if it's in the button's area
            if (e.Button == System.Windows.Forms.MouseButtons.Left && ((RectangleF)playButtonBounds).Contains(e.CanvasLocation))
                aboutToPlay = true;

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
                    aboutToPlay = false;

                    if (!owner.WaveIsPlaying) // Start playing
                    {
                        PlayingWave = owner.Wave.ToSampleProvider();
                        owner.Mixer.AddMixerInput(PlayingWave);

                        owner.PlayState.T0 = owner.DefaultLatency;
                        owner.WaveIsPlaying = true;
                        owner.OnPingDocument()?.ScheduleSolution(owner.TickRate, owner.TriggerPlayheadUpdate);
                    }
                    else // Stop playing
                    {
                        owner.WaveIsPlaying = false; 
                        owner.Mixer.RemoveAllMixerInputs();
                    }
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }
}
