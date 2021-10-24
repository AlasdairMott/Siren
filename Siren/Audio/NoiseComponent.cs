using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using NAudio.Wave.SampleProviders;

namespace Siren.Audio
{
    public class NoiseComponent : GH_Component
    {
        private string _noisetype;

        /// <summary>
        /// Initializes a new instance of the NoiseComponent class.
        /// </summary>
        public NoiseComponent()
          : base("Noise", "Noise",
              "Produces a signal comprised of random pitches.",
              "Siren", "Oscillators")
        {
            _noisetype = "White";
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Duration", "D", "Duration of the noise", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int sampleRate = SirenSettings.SampleRate;
            double duration = 0.5;
            if (!DA.GetData(0, ref duration)) return;

            SignalGeneratorType type;
            switch (_noisetype)
            {
                case "White": type = SignalGeneratorType.White; break;
                case "Pink": type = SignalGeneratorType.Pink; break;
                default: throw new ArgumentOutOfRangeException("wavetype not valid");
            }
            var noise = SampleProviders.NoiseGenerator.Oscillator(440, duration * 2, type);

            DA.SetData(0, noise);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.noise;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("35236b27-bee8-43ac-bef7-1f76946566da"); }
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);

            var m_oscillator = Menu_AppendItem(menu, "Waves", null, true);
            var waves = new List<string>() { "White", "Pink" };
            foreach (var w in waves)
            {
                var option = new ToolStripMenuItem()
                {
                    Text = w,
                    Checked = (w == _noisetype)
                };
                m_oscillator.DropDownItems.Add(option);
            }
            m_oscillator.DropDownItemClicked += MenuWaveClicked;
        }

        private void MenuWaveClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            _noisetype = ((ToolStripMenuItem)e.ClickedItem).Text;
            ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("wavetype", _noisetype);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            reader.TryGetString("wavetype", ref _noisetype);
            return base.Read(reader);
        }
    }
}
