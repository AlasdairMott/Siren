using System;
using System.Collections.Generic;
using NAudio.Wave;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace Siren.Geometry
{
	public class SpectralCurveComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the SpectrumAnalyserComponent class.
		/// </summary>
		public SpectralCurveComponent()
		  : base("SpectralCurveComponent", "Nickname",
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
			pManager.AddNumberParameter("Time Factor", "T", "T", GH_ParamAccess.item);
			pManager.AddNumberParameter("Amplitude Factor", "A", "A", GH_ParamAccess.item);

			pManager[1].Optional = true;
			pManager[2].Optional = true;
			pManager[3].Optional = true;
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
			var wave = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat());
			if (!DA.GetData("Wave", ref wave)) return;

			double X = SirenSettings.TimeScale;
			double Y = SirenSettings.AmplitudeScale;
			int resolution = 10;

			DA.GetData("Resolution", ref resolution); if (resolution <= 0) throw new Exception("Resolution must be positive");
			DA.GetData(2, ref X); if (X <= 0) throw new Exception("T must be positive");
			DA.GetData(3, ref Y); if (Y <= 0) throw new Exception("A must be positive");
			
			wave.Position = 0;
			var fft = new SampleProviders.FFT(wave.ToSampleProvider());
			wave.Position = 0;

			var buffer = new float[wave.Length];
			int samplesRead = fft.Read(buffer, 0, (int)wave.Length);
			var stream = fft.GetFFTWave();

			var polyline = GeometryFunctions.ISampleToPolyline(stream.ToSampleProvider(), X, Y, resolution, 
				GeometryFunctions.WindowMethod.Max, GeometryFunctions.ScalingMethod.Logarithmic);

			var factor = (X / wave.TotalTime.TotalSeconds) / polyline.Last.X;
			polyline.Transform(Transform.Scale(Plane.WorldXY, factor, 1.0, 1.0));

			DA.SetData(0, polyline);
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
			get { return new Guid("5bc1d42d-a12c-4464-943b-fc1d09e01d71"); }
		}
	}
}