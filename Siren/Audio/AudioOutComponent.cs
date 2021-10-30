using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Siren
{
    public class AudioOutComponent : GH_Component
    {
        public Rhino.Geometry.Interval PlayState; // Form of (currentTime, totalTime)
        public readonly int TickRate = 100; // playStateTimer duration, e.g. playhead update rate (in ms)
        public double DefaultLatency { get; private set; }
        public MeteringSampleProvider Wave { get; private set; }
        public WaveOut WaveOut { get; }
        public double Peak { get; private set; } = 0.0;

        /// <summary>
        /// Initializes a new instance of the AudioOutComponent class.
        /// </summary>
        public AudioOutComponent()
          : base("Audio Out", "AOut",
              "Allows a signal to be played within Grasshopper.",
              "Siren", "Utilities")
        {
            WaveOut = new WaveOut();
            DefaultLatency = WaveOut.DesiredLatency / 1000f;
        }

        public override void CreateAttributes()
        {
            m_attributes = new GH_PlayButtonAttributes(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new WaveStreamParameter(), "Wave Left", "L", "Left channel input", GH_ParamAccess.item);
            pManager.AddParameter(new WaveStreamParameter(), "Wave Right", "R", "Right channel input", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntervalParameter("Play Progress", "P", "The play progress, in seconds, of the sample", GH_ParamAccess.item);
            pManager.AddNumberParameter("Level", "L", "Current audio level", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (WaveOut.PlaybackState == PlaybackState.Playing && PlayState != null)
            {
                PlayState.T0 = ((double)WaveOut.GetPosition()) / WaveOut.OutputWaveFormat.AverageBytesPerSecond;

                DA.SetData(0, PlayState);
                DA.SetData(1, Peak);

                OnPingDocument()?.ScheduleSolution(TickRate, TriggerPlayheadUpdate);

                return; // Skip rest of solve
            }
            else
            {
                PlayState.T0 = 0.0;
            }

            var left = CachedSound.Empty;
            var right = CachedSound.Empty;

            ISampleProvider stereo;
            if (!DA.GetData(0, ref left)) return;
            if (DA.GetData(1, ref right))
            {
                stereo = new StereoProvider(right.ToSampleProvider(), left.ToSampleProvider());
            }
            else
            {
                stereo = new MonoToStereoSampleProvider(left.ToSampleProvider());
            }

            if (Wave != null) { Wave.StreamVolume -= Wave_StreamVolume; } //[AM] not sure if this is necessary

            var notification = (int)TimeSpan.FromMilliseconds(TickRate).TotalSeconds * stereo.WaveFormat.SampleRate * 2;
            Wave = new MeteringSampleProvider(stereo, notification);
            Wave.StreamVolume += Wave_StreamVolume;

            var time = Math.Max(left.TotalTime.TotalSeconds, right.TotalTime.TotalSeconds);

            PlayState = new Rhino.Geometry.Interval(0.0, time);
            DA.SetData(0, PlayState);
            DA.SetData(1, 0);
        }

        private void Wave_StreamVolume(object sender, StreamVolumeEventArgs e) => Peak = Math.Max(e.MaxSampleValues[0], e.MaxSampleValues[1]);

        public void TriggerPlayheadUpdate(GH_Document gh) => ExpireSolution(false);

        public override void AddedToDocument(GH_Document document) => base.AddedToDocument(document);

        public override void RemovedFromDocument(GH_Document document)
        {
            if (Wave != null) { Wave.StreamVolume -= Wave_StreamVolume; }
            WaveOut.Stop();
            WaveOut.Dispose();

            base.RemovedFromDocument(document);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Properties.Resources.playback;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("55f99243-1902-4ae3-a1e4-b2041ac6abf1"); }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            var m_save = new ToolStripMenuItem("Save file")
            {
                Enabled = (Wave != null)
            };
            menu.Items.Add(m_save);
            m_save.Click += button_OnSave;
        }

        private void button_OnSave(object sender, EventArgs e)
        {
            var fd = new Rhino.UI.SaveFileDialog()
            {
                Title = "Save file",
                DefaultExt = "wav",
                Filter = "wav files (*.wav)|*.wav|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                FileName = "Audio"
            };

            var result = fd.ShowSaveDialog();
            if (result)
            {
                WaveFileWriter.CreateWaveFile(fd.FileName, Wave.ToWaveProvider());
            }
        }
    }

    public class GH_PlayButtonAttributes : GH_ComponentAttributes
    {
        private readonly int _dragSpace = 15;
        private readonly int _buttonWidth = 46;
        private RectangleF _outerButtonBounds; // Includes draghandle space
        private Rectangle _playButtonBounds; // Triggers click
        private readonly AudioOutComponent _owner;
        private bool _clicked = false;
        private bool _hasWave = false;

        public GH_PlayButtonAttributes(AudioOutComponent owner) : base(owner)
        {
            _owner = owner;
        }

        protected override void Layout()
        {
            base.Layout();

            _outerButtonBounds = new RectangleF(Bounds.X, Bounds.Y, _buttonWidth + _dragSpace * 2, Bounds.Height);
            LayoutInputParams(Owner, _outerButtonBounds);
            LayoutOutputParams(Owner, _outerButtonBounds);
            Bounds = LayoutBounds(Owner, _outerButtonBounds);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Objects)
            {
                base.Render(canvas, graphics, channel);
                return;
            }

            RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true); // Standard UI

            _playButtonBounds = GH_Convert.ToRectangle(_outerButtonBounds); // Icon inset space
            _playButtonBounds.X += _dragSpace;
            _playButtonBounds.Width = _buttonWidth;

            // Black button background
            GH_Capsule button;
            if (_clicked)
            {
                button = GH_Capsule.CreateTextCapsule(_playButtonBounds, _playButtonBounds, GH_Palette.Grey, "", 1, 0);
                _clicked = false;
            }
            else
            {
                button = GH_Capsule.CreateTextCapsule(_playButtonBounds, _playButtonBounds, GH_Palette.Black, "", 1, 0);
            }

            button.Render(graphics, Selected, Owner.Locked, false);
            button.Dispose();

            if (_owner.WaveOut.PlaybackState != PlaybackState.Playing)
                DrawPlayTriangle(graphics, _playButtonBounds);
            else
                DrawStopSquare(graphics, _playButtonBounds);

            var levelBounds = Bounds;
            levelBounds.X = Bounds.X + Bounds.Width - 32;
            levelBounds.Width = 4;
            levelBounds.Height = 26;
            levelBounds.Y += (Bounds.Height - levelBounds.Height) * 0.5f;
            DrawLevel(canvas, graphics, levelBounds);
        }

        private void DrawStopSquare(Graphics graphics, Rectangle playButtonBounds)
        {
            using (var fill = new SolidBrush(Color.White))
            using (var outerstroke = new Pen(Color.Black, 4f) { LineJoin = LineJoin.Round })
            using (var innerstroke = new Pen(Color.LightGray, 2f) { LineJoin = LineJoin.Round })
            {
                var size = new Size(12, 12);
                var topLeft = new Point(playButtonBounds.X + 17, playButtonBounds.Y + 16);
                var square = new Rectangle(topLeft, size);
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
                int YTop = playButtonBounds.Y + 17;
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

        private void DrawLevel(GH_Canvas canvas, Graphics graphics, RectangleF bounds)
        {
            var playing = _owner.WaveOut.PlaybackState == PlaybackState.Playing;

            using (var black = new Pen(Color.FromArgb(160, 0, 0, 0), 0.8f))
            using (var blackSolid = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
            using (var greenSolid = new SolidBrush(Color.FromArgb(255, 52, 209, 76)))
            {
                var count = 4;
                var rect = bounds;
                rect.Height = rect.Width;
                var spacing = (bounds.Height - rect.Height) / (count - 1);
                for (var i = count - 1; i >= 0; i--)
                {
                    rect.Y = bounds.Y + i * spacing;

                    if (playing && _hasWave && _owner.Peak * count >= count - i - 1)
                    {
                        graphics.FillEllipse(greenSolid, rect);
                    }
                    else
                    {
                        graphics.FillEllipse(blackSolid, rect);
                    }

                    graphics.DrawEllipse(black, rect);
                }
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            // Checking if it's a left click, and if it's in the button's area
            if (e.Button == System.Windows.Forms.MouseButtons.Left && ((RectangleF)_playButtonBounds).Contains(e.CanvasLocation))
            {
                _clicked = true;

                if (_owner.WaveOut.PlaybackState != PlaybackState.Playing) // Start playing
                {
                    if (_owner.Wave == null) { return GH_ObjectResponse.Handled; }

                    _hasWave = true;

                    _owner.WaveOut.Init(_owner.Wave);
                    _owner.WaveOut.Play();

                    _owner.PlayState.T0 = _owner.DefaultLatency;
                    _owner.OnPingDocument()?.ScheduleSolution(_owner.TickRate, _owner.TriggerPlayheadUpdate);

                }
                else // Stop playing
                {
                    _owner.WaveOut.Stop();
                }

                ExpireLayout();
                sender.Refresh();
                return GH_ObjectResponse.Handled;
            }

            return base.RespondToMouseDown(sender, e);
        }
    }
}
