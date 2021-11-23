using System;
using Grasshopper.Kernel;

namespace Siren.Audio
{
    public class DelayComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DelayComponent class.
        /// </summary>
        public DelayComponent()
          : base("Delay", "Dly",
              "Delay",
              "Siren", "Effects")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new CachedSoundParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
            pManager.AddNumberParameter("Delay", "D", "Delay Amount", GH_ParamAccess.item);
            pManager.AddNumberParameter("Feedback", "F", "Feedback", GH_ParamAccess.item);
            pManager.AddNumberParameter("Colour", "C", "Colour", GH_ParamAccess.item);
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
            var waveIn = CachedSound.Empty;
            if (!DA.GetData(0, ref waveIn)) return;

            double time = 1.0;
            double feedback = 0.0;
            double colour = 0.5;

            if (!DA.GetData(1, ref time)) return;
            if (!DA.GetData(2, ref feedback)) return;
            if (!DA.GetData(3, ref colour)) return;

            if (!(0 <= colour || colour <= 1.0)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Colour must be between 0 and 1"); return; }

            colour = Math.Pow(colour, 2);

            var delay = new SampleProviders.DelayProvider(waveIn.ToSampleProvider(), TimeSpan.FromSeconds(time), (float)feedback, (float)colour);

            DA.SetData(0, delay);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.echo;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("38a64e7e-f367-4ffe-9065-9c8b1e9735d5"); }
        }
    }
}