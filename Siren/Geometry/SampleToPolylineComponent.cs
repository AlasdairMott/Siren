using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NAudio.Wave;
using Rhino.Geometry;

namespace Siren.Geometry
{
	public class SampleToPolylineComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the SampleToCurveComponent class.
		/// </summary>
		public SampleToPolylineComponent()
		  : base("SampleToCurveComponent", "Nickname",
			  "Description",
			  "GHSynth", "Utilities")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
			pManager.AddNumberParameter("Time Factor", "T", "T", GH_ParamAccess.item);
			pManager.AddNumberParameter("Amplitude Factor", "A", "A", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Resolution", "R", "Resolution of the display", GH_ParamAccess.item);

			pManager[1].Optional = true;
			pManager[2].Optional = true;
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddCurveParameter("Polyline", "P", "Polyline", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			var wave = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat()) as WaveStream;
			if (!DA.GetData(0, ref wave)) return;

			double X = SirenSettings.TimeScale;
			double Y = SirenSettings.AmplitudeScale;
			int resolution = 10;

			DA.GetData(1, ref X); if (X <= 0) throw new Exception("T must be positive");
			DA.GetData(2, ref Y); if (Y <= 0) throw new Exception("A must be positive");
			DA.GetData("Resolution", ref resolution); if (resolution <= 0) throw new Exception("Resolution must be positive");

			wave.Position = 0;
			var polyline = GeometryFunctions.ISampleToPolyline(wave.ToSampleProvider(), X, Y, resolution);
			wave.Position = 0;

			DA.SetData(0, polyline);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.curve;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("63c5f034-e7a0-4b01-b1be-d51bfcd2c786"); }
		}
	}
}