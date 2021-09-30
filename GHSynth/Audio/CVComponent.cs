using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using NAudio.Wave;
using System.IO;
using System.Drawing;

namespace GHSynth.Audio
{
	public class CVComponent : GH_Component /*, IGH_PreviewData*/
	{
		private Curve bounds;
		private List<Line> timeIntervals;

		/// <summary>
		/// Initializes a new instance of the CVComponent class.
		/// </summary>
		public CVComponent()
		  : base("CVComponent", "Nickname",
			  "Description",
			  "GHSynth", "CV Generators")
		{
			bounds = new PolylineCurve();
			timeIntervals = new List<Line>();
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
			var count = (int)(width / X) * sampleRate;

			var start = bbox.GetEdges()[3].PointAt(0.5);
			var cuttingPlane = new Plane(plane);
			cuttingPlane.XAxis = plane.ZAxis;
			cuttingPlane.ZAxis = -plane.XAxis;
			cuttingPlane.Origin = start;

			#region display
			var transform = Transform.ChangeBasis(Plane.WorldXY, plane);
			var boundsRect = new Polyline(new Point3d[4] {
				bbox.Corner(true, true, true),
				bbox.Corner(false, true, true),
				bbox.Corner(false, false, true),
				bbox.Corner(true, false, true) });
			boundsRect.Transform(transform);
			boundsRect[0] = new Point3d(boundsRect[0].X, -Y, boundsRect[0].Z);
			boundsRect[1] = new Point3d(boundsRect[1].X, -Y, boundsRect[1].Z);
			boundsRect[2] = new Point3d(boundsRect[2].X,  Y, boundsRect[2].Z);
			boundsRect[3] = new Point3d(boundsRect[3].X,  Y, boundsRect[3].Z);
			transform.TryGetInverse(out transform);
			boundsRect.Transform(transform);
			bounds = boundsRect.ToPolylineCurve();

			var numberOfSeconds = Math.Ceiling(width / X);
			var span = new Vector3d(0, 2 * Y, 0);
			for (int i = 0; i < numberOfSeconds; i++)
			{
				var line = new Line(new Point3d(i * X, -Y, 0), span);
				line.Transform(transform);
				timeIntervals.Add(line);
			}
			#endregion

			int repeats = (int) GHSynthSettings.SampleRate / sampleRate;

			var buffer = new List<byte>();
			for (int i = 0; i < count; i++) 
			{
				var intersection = Rhino.Geometry.Intersect.Intersection.CurvePlane(curve, cuttingPlane, tolerance);

				var point = intersection.First().PointA;
				plane.ClosestParameter(point, out double s, out double t);
				var value = (float) Math.Min(Math.Max((t / Y), -1), 1);
				var sample = Convert.ToInt16(short.MaxValue * value);
				var sampleBytes = BitConverter.GetBytes(sample);

				for (int j = 0; j < repeats; j++)
					buffer.AddRange(sampleBytes);

				cuttingPlane.Origin += new Point3d(width / count, 0, 0);
			}

			var bufferStream = buffer.ToArray();
			var wave = new RawSourceWaveStream(bufferStream, 0, bufferStream.Length, new WaveFormat(GHSynthSettings.SampleRate, 1));

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

		public override void DrawViewportWires(IGH_PreviewArgs args)
		{
			base.DrawViewportWires(args);
			args.Display.DrawCurve(bounds, Color.Red);
			foreach (Line l in timeIntervals)
			{
				args.Display.DrawLine(l, Color.Red);
			}
		}
	}
}