using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Siren.Geometry
{
	public class SpectralMeshComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the SpectralMeshComponent class.
		/// </summary>
		public SpectralMeshComponent()
		  : base("SpectralMeshComponent", "Nickname",
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
			pManager.AddIntegerParameter("Resolution", "R", "Resolution of the display", GH_ParamAccess.item);
			pManager.AddNumberParameter("Window", "W", "Window to take max from (in seconds)", GH_ParamAccess.item);
			pManager.AddNumberParameter("Time Factor", "T", "T", GH_ParamAccess.item);
			pManager.AddNumberParameter("Amplitude Factor", "A", "A", GH_ParamAccess.item);

			pManager[1].Optional = true;
			pManager[2].Optional = true;
			pManager[3].Optional = true;
			pManager[4].Optional = true;
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddCurveParameter("FFT", "FFT", "FFT", GH_ParamAccess.item);
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
		protected override System.Drawing.Bitmap Icon => Properties.Resources.spectrum;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("ea104011-bd14-46e6-95ea-43380d4ccc98"); }
		}
	}
}