using System;
using Grasshopper.Kernel;

namespace Siren.Audio
{
    public class ADEnvelopeComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ADEnvelopeComponent class.
        /// </summary>
        public ADEnvelopeComponent()
          : base("AD Envelope Generator", "AD",
              "Attack/Decay Envelope Generator",
              "Siren", "Envelope")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new WaveStreamParameter(), "Trigger", "T", "Trigger input", GH_ParamAccess.item);
            pManager.AddNumberParameter("Attack", "A", "Attack", GH_ParamAccess.item);
            pManager.AddNumberParameter("Decay", "D", "Decay", GH_ParamAccess.item);
            pManager.AddNumberParameter("Exponent", "E", "Exponent (> 0)", GH_ParamAccess.item);
            pManager[3].Optional = true;
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
            var waveIn = CachedSound.Empty;
            if (!DA.GetData(0, ref waveIn)) return;

            var attack = 1.0;
            var decay = 1.0;
            var exponent = 1.0;
            if (!DA.GetData(1, ref attack)) return;
            else if (attack < 0) return;
            if (!DA.GetData(2, ref decay)) return;
            else if (decay < 0) return;
            DA.GetData(3, ref exponent);
            if (exponent <= 0) return;

            var AD = new SampleProviders.ADEnvelopeProvider(waveIn.ToSampleProvider(), TimeSpan.FromSeconds(attack), TimeSpan.FromSeconds(decay), (float)exponent);

            DA.SetData(0, AD);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.AD;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("62e6c701-bd59-4e03-9898-7bf953043b4b"); }
        }
    }
}
