using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Siren.Audio
{
	public class SampleHoldComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the SampleAndHoldComponent class.
		/// </summary>
		public SampleHoldComponent()
		  : base("Sample & Hold", "S&H",
			  "Takes a 'snapshot' of a signal at the provided trigger and maintains it until re-triggered.",
			  "Siren", "CV Control")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
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
		protected override System.Drawing.Bitmap Icon => Properties.Resources.sampleHold;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("50e0dfb9-49f1-4107-bd95-5b3e29282520"); }
		}
	}
}