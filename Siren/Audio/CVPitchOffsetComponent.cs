using System;
using Grasshopper.Kernel;

namespace Siren.Audio
{
    public class CVPitchOffsetComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CVPitchOffsetComponent class.
        /// </summary>
        public CVPitchOffsetComponent()
          : base("CV Pitch Offset", "cvOff",
              "Offset CV signal pitch in octaves and semitones",
              "Siren", "CV Control")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new CachedSoundParameter(), "Wave", "W", "CV to offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Octaves", "O", "Octave offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Semitones", "S", "Semi tone offset", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new CachedSoundParameter(), "Wave", "W", "Wave output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var cvIn = CachedSound.Empty;
            if (!DA.GetData(0, ref cvIn)) return;

            double octave = 0;
            double semi = 0;
            DA.GetData(1, ref octave);
            DA.GetData(2, ref semi);

            var offsetAmount = (float)(0.1 * ((octave * 12 + semi) / 12.0));

            var offset = new SampleProviders.AttenuverterProvider(cvIn.ToSampleProvider(), 1.0f, offsetAmount);

            DA.SetData(0, offset);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.offset;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d7fdbc54-b762-449d-8283-d39f6c803d35"); }
        }
    }
}
