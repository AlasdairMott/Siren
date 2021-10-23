using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Siren.Utilities
{
	public class KnobComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the KnobComponent class.
		/// </summary>
		public KnobComponent()
		  : base("Knob", "Knob",
			  "Knob Demo",
			  "Siren", "Utilities")
		{
		}

		public override void CreateAttributes()
		{
			m_attributes = new GH_KnobAttributes(this, "Frequency");
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
			pManager.AddNumberParameter("Delay", "D", "Delay Amount", GH_ParamAccess.item);
			pManager.AddNumberParameter("Feedback", "F", "Feedback", GH_ParamAccess.item);

			for (int i = 0; i < 3; i++) pManager[i].Optional = true;
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
		protected override System.Drawing.Bitmap Icon => Properties.Resources.Hz;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("63088559-9ad5-4878-890f-f9e5529dfc68"); }
		}
	}
}