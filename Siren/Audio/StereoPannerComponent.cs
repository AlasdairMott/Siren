using System;
using Grasshopper.Kernel;
using NAudio.Wave.SampleProviders;
using Siren.Utilities;

namespace Siren.Audio
{
    public class StereoPannerComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the StereoPannerComponent class.
        /// </summary>
        public StereoPannerComponent()
          : base("Stereo Panner", "Pan",
              "Description",
              "Siren", "Mixing")
        {
        }

        public override void CreateAttributes()
        {
            m_attributes = new GH_KnobAttributes(this, "PAN", 40f, 50f);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pan", "P", "Pan left/right", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new WaveStreamParameter(), "Left", "L", "Left wave output", GH_ParamAccess.item);
            pManager.AddParameter(new WaveStreamParameter(), "Right", "R", "Right wave output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var waveIn = CachedSound.Empty;
            if (!DA.GetData(0, ref waveIn)) return;

            var attributes = m_attributes as GH_KnobAttributes;

            var pan = 0.0;
            if (!DA.GetData(1, ref pan))
            {
                pan = attributes.P;
                pan = SirenUtilities.Remap((float)pan, attributes.Min, attributes.Max, -1.0f, 1.0f);
                attributes.Locked = false;
            }
            else
            {
                attributes.P = SirenUtilities.Clamp((float)pan, attributes.Min, attributes.Max);
                attributes.Locked = true;
            }

            var waveInProvider = waveIn.ToSampleProvider();
            var stereo = new PanningSampleProvider(waveInProvider)
            {
                Pan = (float)pan
            };

            DA.SetData(0, new StereoToMonoSampleProvider(stereo) { RightVolume = 0.0f });

            waveInProvider.Position = 0;
            DA.SetData(1, new StereoToMonoSampleProvider(stereo) { LeftVolume = 0.0f });
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
            get { return new Guid("62a5f1cb-5dc9-488b-a45b-6c2710b14832"); }
        }
    }
}
