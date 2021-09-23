using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GHSynth
{
	public class Quantizer : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the Quantizer class.
		/// </summary>
		public Quantizer()
		  : base("Quantizer", "Q",
			  "Quantize pitch",
			  "GHSynth", "Subcategory")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddNumberParameter("Pitch", "P", "Pitch in V/O", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Notes in scale", "N", "Number of notes in the scale", GH_ParamAccess.item);
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddNumberParameter("Pitch", "Q", "Quantized Pitch in V/O", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			double pitch = 1;
			if (!DA.GetData(0, ref pitch)) return;

			int notes = 12;
			DA.GetData(1, ref notes);

			//round pitch to nearest (1/notes)
			DA.SetData(0, Math.Round(pitch * notes, 0) / notes);
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
			get { return new Guid("1f321dfe-b48e-4c6b-bec9-00ddfff05868"); }
		}
	}
}