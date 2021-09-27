using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHSynth.Audio
{
	public class CVComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the CVComponent class.
		/// </summary>
		public CVComponent()
		  : base("CVComponent", "Nickname",
			  "Description",
			  "GHSynth", "Subcategory")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddCurveParameter("Curve", "C", "CV Curve", GH_ParamAccess.item);
			//pManager.AddIntegerParameter("Sample Count", "S", "Number of times to sample to curve", GH_ParamAccess.item);
			pManager.AddInterval2DParameter("Horizontal Range", "H", "H", GH_ParamAccess.item);
			pManager.AddInterval2DParameter("Vertical Range", "V", "V", GH_ParamAccess.item);

			for(int p = 0; p < pManager.ParamCount; p++) pManager[p].Optional = true;
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
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.cv;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("3fe2ff09-1683-4131-87d7-d2760c50d4ff"); }
		}
	}
}