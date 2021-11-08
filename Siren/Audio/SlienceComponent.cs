using System;
using Grasshopper.Kernel;

namespace Siren.Audio
{
    public class SlienceComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SlienceComponent class.
        /// </summary>
        public SlienceComponent()
          : base("Silence", "Silence",
              "Produces a silent signal.",
              "Siren", "Oscillators")
        {
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
            pManager.AddParameter(new CachedSoundParameter(), "Wave", "W", "Wave output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double duration = 0.5;
            if (!DA.GetData(0, ref duration)) return;

            var silence = SampleProviders.SignalGenerator.CreateSilence(TimeSpan.FromSeconds(duration));

            DA.SetData(0, silence);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.silence;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e1c7142f-1da2-4ac7-bfb6-d66818459680"); }
        }
    }
}
