using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace Siren.Audio
{
    public class CVQuantizerComponent : GH_Component
    {
        private string _scaleName;

        /// <summary>
        /// Initializes a new instance of the CVQuantizer class.
        /// </summary>
        public CVQuantizerComponent()
          : base("CV Quantizer", "Quant",
              "Takes a signal and transforms it into discrete steps that match the provided notes.",
              "Siren", "CV Control")
        {
            _scaleName = "Chromatic";
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new CachedSoundParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
            pManager.AddNumberParameter("Notes", "N", "Notes (as real numbers [0,12)) to quantize to", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new CachedSoundParameter(), "Wave", "W", "Wave output", GH_ParamAccess.item);
            pManager.AddParameter(new CachedSoundParameter(), "Trigger", "T", "Trigger output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var cvIn = CachedSound.Empty;
            if (!DA.GetData(0, ref cvIn)) return;

            var scale = new List<double>();
            if (!DA.GetDataList(1, scale))
            {
                scale = Scales(_scaleName);
            }

            if (scale.Min() < 0 || scale.Max() >= 12) throw new ArgumentOutOfRangeException("Values in scale must be from [0,12)");
            scale = scale.Select(s => s * (1.0 / 12.0)).ToList();

            var quantizer = new SampleProviders.CVQuantizer(cvIn.ToSampleProvider(), scale);

            DA.SetData(0, quantizer);
            DA.SetData(1, quantizer.Triggers);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.quantize;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("891c530d-4aca-4677-9aa5-e31ed25f8109"); }
        }

        private List<double> Scales(string name)
        {
            var scales = new Dictionary<string, List<double>>()
            {
                {"Chromatic", new List<double>(){0,1,2,3,4,5,6,7,8,9,10,11}},
                {"Major", new List<double>(){0,2,4,5,7,9,11}},
                {"Minor", new List<double>(){0,2,3,5,7,8,10}},
                {"Whole Tone", new List<double>(){0,2,4,6,8,10}},
                {"Major Pentatonic", new List<double>(){0,2,4,7,9}},
                {"Minor Pentatonic", new List<double>(){0,3,5,7,10}},
                {"Octave", new List<double>(){0}},
            };
            return scales[name];
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);

            var m_scale = Menu_AppendItem(menu, "Scales", null, true);
            var waves = new List<string>() { "Chromatic", "Major", "Minor", "Whole Tone", "Major Pentatonic", "Minor Pentatonic", "Octave" };
            foreach (var w in waves)
            {
                var option = new ToolStripMenuItem()
                {
                    Text = w,
                    Checked = (w == _scaleName)
                };
                m_scale.DropDownItems.Add(option);
            }
            m_scale.DropDownItemClicked += MenuScaleClicked;
        }

        private void MenuScaleClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            _scaleName = ((ToolStripMenuItem)e.ClickedItem).Text;
            ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("scaleName", _scaleName);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            reader.TryGetString("scaleName", ref _scaleName);
            return base.Read(reader);
        }
    }
}
