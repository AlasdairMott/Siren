using System;
using Grasshopper.Kernel;

namespace Siren.Geometry
{
    public class SampleToPointComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SampleToPointComponent class.
        /// </summary>
        public SampleToPointComponent()
          : base("Sample To Point", "Nickname",
              "Description",
              "Siren", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
            pManager.AddNumberParameter("Time Factor", "T", "T", GH_ParamAccess.item);
            pManager.AddNumberParameter("Threshold", "A", "Amplitude Threshold", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var waveIn = CachedSound.Empty;
            if (!DA.GetData(0, ref waveIn)) return;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.sampleToPoints;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("39b82b62-f090-404b-8e8f-fe7b95ed4cbf"); }
        }
    }
}