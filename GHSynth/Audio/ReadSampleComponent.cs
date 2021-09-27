using Grasshopper.Kernel;
using NAudio.Wave;
using System;
using Rhino.Geometry;
using System.Linq;

namespace GHSynth
{
	public class ReadSampleComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the ReadSampleComponent class.
		/// </summary>
		public ReadSampleComponent()
		  : base("ReadSampleComponent", "Nickname",
			  "Description",
			  "GHSynth", "Subcategory")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddTextParameter("Path", "P", "Path of audio file", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Resolution", "R", "Resolution of the display", GH_ParamAccess.item);
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave output", GH_ParamAccess.item);
			pManager.AddCurveParameter("Polyline", "P", "Polyline", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			string path = "";
			if (!DA.GetData(0, ref path)) return;

			int resolution = 10;
			DA.GetData(1, ref resolution);

			var audioFile = new AudioFileReader(path);

			//var wave = audioFile.ToWaveProvider16().ToSampleProvider();

			var raw = NAudioUtilities.WaveProviderToWaveStream(
				audioFile,
				(int)audioFile.Length,
				audioFile.WaveFormat);

			DA.SetData(0, raw);
			//DA.SetData(1, ISampleToPolyline(audioFile, resolution));

			var polyline = new Polyline();
			var samplesPerSecond = (audioFile.WaveFormat.SampleRate * audioFile.WaveFormat.Channels) 4;
			var readBuffer = new float[samplesPerSecond];
			int samplesRead;

			var xPos = 0.1;
			var yScale = 1;
			do
			{
				samplesRead = audioFile.Read(readBuffer, 0, samplesPerSecond);
				if (samplesRead > 0)
				{
					var max = readBuffer.Take(samplesRead).Max();
					polyline.Add(new Point3d(xPos, yScale + max * yScale, 0));
					xPos += 0.1;
				}

			} while (samplesRead > 0);
			DA.SetData(1, polyline);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.readSample;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("805913e8-8e7b-4264-9368-87228bc3c850"); }
		}

		public Polyline ISampleToPolyline(ISampleProvider sample, int resolution) 
		{
			if (resolution < 1) throw new ArgumentOutOfRangeException("Must be greater than 0");

			var polyline = new Polyline();
			var samplesPerSecond = (sample.WaveFormat.SampleRate * sample.WaveFormat.Channels) / resolution;
			var readBuffer = new float[samplesPerSecond];
			int samplesRead;

			var xPos = 0.1;
			var yScale = 1;
			do
			{
				samplesRead = sample.Read(readBuffer, 0, samplesPerSecond);
				if (samplesRead > 0)
				{
					var max = readBuffer.Take(samplesRead).Max();
					polyline.Add(new Point3d(xPos, yScale + max * yScale, 0));
					xPos += 0.1;
				}

			} while (samplesRead > 0);
			return polyline;
		}
	}
}