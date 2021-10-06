using System;
using System.Collections.Generic;
using NAudio.Wave;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Siren.Audio
{
	public class SpectrumAnalyserComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the SpectrumAnalyserComponent class.
		/// </summary>
		public SpectrumAnalyserComponent()
		  : base("SpectrumAnalyserComponent", "Nickname",
			  "Description",
			  "Siren", "Utilities")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
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

			wave.Position = 0;
			var fft = new SampleProviders.FFT(wave.ToSampleProvider());

			var buffer = new float[wave.Length];
			int samplesRead = fft.Read(buffer, 0, (int)wave.Length);

			wave.Position = 0;

			var polyline = new Polyline();
			for (int i = 0; i < fft.DataFft.Length / 2; i++)
			{
				var pt = new Point3d(2 * i * ((double)SirenSettings.TimeScale / (double)SirenSettings.SampleRate), fft.DataFft[i] * (double)SirenSettings.AmplitudeScale, 0);
				polyline.Add(pt);
			}

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