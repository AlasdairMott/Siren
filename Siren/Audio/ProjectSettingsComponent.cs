using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Siren.Audio
{
	public class ProjectSettingsComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the ProjectSettingsComponent class.
		/// </summary>
		public ProjectSettingsComponent()
		  : base("ProjectSettingsComponent", "Nickname",
			  "Description",
			  "GHSynth", "Utilities")
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
			pManager.AddIntegerParameter("Sample Rate", "S", "Sample rate for the project", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Time Scale", "T", "Time scale for the project", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Amplitude Scale", "A", "Amplitude scale for the project", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Tempo", "bpm", "Tempo of the project", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			DA.SetData(0, SirenSettings.SampleRate);
			DA.SetData(1, SirenSettings.TimeScale);
			DA.SetData(2, SirenSettings.AmplitudeScale);
			DA.SetData(3, SirenSettings.Tempo);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.settings;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("0c8dd00a-ab61-4da4-9549-4283c44ff18f"); }
		}
	}
}