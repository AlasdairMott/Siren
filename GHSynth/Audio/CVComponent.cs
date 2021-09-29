using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using NAudio.Wave;
using System.IO;

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
			pManager.AddPlaneParameter("Plane", "P", "Origin and orienation of the curve", GH_ParamAccess.item);
			pManager.AddNumberParameter("Time Factor", "T", "T", GH_ParamAccess.item);
			pManager.AddNumberParameter("Amplitude Factor", "A", "A", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Sample Rate", "S", "Samples per second", GH_ParamAccess.item);

			//for (int p = 1; p < pManager.ParamCount; p++) pManager[p].Optional = true;
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			//pManager.AddPointParameter("Points", "P", "Debugging points", GH_ParamAccess.list);
			//pManager.AddNumberParameter("Floats", "F", "Debugging floats", GH_ParamAccess.list);
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave output", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			var curve = new PolylineCurve() as Curve;
			var plane = Plane.WorldXY;
			double X = 0;
			double Y = 0;
			var sampleRate = GHSynthSettings.SampleRate;

			if (!DA.GetData(0, ref curve)) return;
			DA.GetData(1, ref plane);
			DA.GetData(2, ref X); if (X <= 0) throw new Exception("T must be positive");
			DA.GetData(3, ref Y); if (Y <= 0) throw new Exception("A must be positive");
			DA.GetData(4, ref sampleRate); if (sampleRate <= 0 || sampleRate > GHSynthSettings.SampleRate) 
				throw new Exception("Sample rate must be positive and less than project sample rate");

			double tolerance = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

			var bbox = curve.GetBoundingBox(plane);
			var width = bbox.GetEdges()[0].Length;
			var count = (int) ( width / X) * sampleRate;

			var start = bbox.GetEdges()[3].PointAt(0.5);
			var cuttingPlane = new Plane(plane);
			cuttingPlane.XAxis = plane.ZAxis;
			cuttingPlane.ZAxis = - plane.XAxis;
			cuttingPlane.Origin = start;

			int repeats = (int) GHSynthSettings.SampleRate / sampleRate;

			//var shortBuffer = new short[count];
			var buffer = new List<byte>();
			//var points = new Point3d[count];
			for (int i = 0; i < count; i++) 
			{
				var intersection = Rhino.Geometry.Intersect.Intersection.CurvePlane(curve, cuttingPlane, tolerance);

				var point = intersection.First().PointA;
				plane.ClosestParameter(point, out double s, out double t);
				var sample = Convert.ToInt16(short.MaxValue * (float)(t / Y));
				var sampleBytes = BitConverter.GetBytes(sample);

				
				for (int j = 0; j < repeats; j++)
					buffer.AddRange(sampleBytes);

				//points[i] = point;
				//var gimbal = new List<Line>()
				//{
				//	new Line(cuttingPlane.Origin, cuttingPlane.Origin + cuttingPlane.XAxis * 2),
				//	new Line(cuttingPlane.Origin, cuttingPlane.Origin + cuttingPlane.YAxis * 2),
				//	new Line(cuttingPlane.Origin, cuttingPlane.Origin + cuttingPlane.ZAxis * 2)
				//};
				//foreach (var g in gimbal) Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(g);

				cuttingPlane.Origin += new Point3d(width / count, 0, 0);
			}

			//DA.SetDataList(0, points);
			//DA.SetDataList(1, buffer);
			var bufferStream = buffer.ToArray();
			var wave = new RawSourceWaveStream(bufferStream, 0, bufferStream.Length, new WaveFormat(GHSynthSettings.SampleRate, 1));

			//var resampled = new MediaFoundationResampler(wave, 44100);

			//wave.Position = 0;
			//var stream = NAudioUtilities.WaveProviderToWaveStream(
			//	resampled.ToSampleProvider(),
			//	(int)wave.Length,
			//	resampled.WaveFormat);
			//wave.Position = 0;

			//using (var stream = new MemoryStream()) 
			//{
			//	var writer = new StreamWriter(stream);
			//	writer.Write(buffer);
			//	writer.Flush();
			//	stream.Position = 0;
			//	wave = new RawSourceWaveStream(stream, new WaveFormat(sampleRate, 1));
			//}

			//wave = new short[duration];
			//var binaryWave = new byte[sampleRate * sizeof(short)];
			//for (int i = 0; i < duration; i++)
			//{
			//	wave[i] = Convert.ToInt16(short.MaxValue * Math.Sin(((Math.PI * 2 * frequency) / SAMPLE_RATE) * i));
			//}
			//Buffer.BlockCopy(shortBuffer, 0, binaryWave, 0, shortBuffer.Length * sizeof(short));
			//wave = new RawSourceWaveStream(binaryWave, 0, binaryWave.Length, new WaveFormat(sampleRate, 1));



			DA.SetData(0, wave);

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