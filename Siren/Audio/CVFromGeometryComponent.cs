using Grasshopper.Kernel;
using NAudio.Wave;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Siren.Audio
{
	public class CVFromGeometryComponent : GH_Component /*, IGH_PreviewData*/
	{
		private Curve bounds;
		private List<Line> timeIntervals;
		private BoundingBox boundingBox;

		/// <summary>
		/// Initializes a new instance of the CVComponent class.
		/// </summary>
		public CVFromGeometryComponent()
		  : base("CV From Geometry", "CVGeo",
			  "Produces a wave by sampling a curve at a given resolution.",
			  "Siren", "CV Control")
		{
			bounds = new PolylineCurve();
			timeIntervals = new List<Line>();
			boundingBox = BoundingBox.Empty;
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddCurveParameter("Curve", "C", "CV Curve", GH_ParamAccess.list);
			pManager.AddPlaneParameter("Plane", "P", "Origin and orienation of the curve", GH_ParamAccess.item);
			pManager.AddNumberParameter("Time Factor", "T", "T", GH_ParamAccess.item);
			pManager.AddNumberParameter("Amplitude Factor", "A", "A", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Sample Rate", "S", "Samples per second", GH_ParamAccess.item);

			pManager[1].Optional = true;
			pManager[2].Optional = true;
			pManager[3].Optional = true;
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave output", GH_ParamAccess.item);
			pManager.AddParameter(new WaveStreamParameter(), "Trigger", "T", "Trigger output", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			timeIntervals.Clear();

			var curves = new List<Curve>();
			var plane = Plane.WorldXY;
			double X = SirenSettings.TimeScale;
			double Y = SirenSettings.AmplitudeScale;
			var sampleRate = SirenSettings.SampleRate;

			if (!DA.GetDataList(0, curves)) return;
			DA.GetData(1, ref plane);
			DA.GetData(2, ref X); if (X <= 0) throw new Exception("T must be positive");
			DA.GetData(3, ref Y); if (Y <= 0) throw new Exception("A must be positive");
			DA.GetData(4, ref sampleRate); if (sampleRate <= 0 || sampleRate > SirenSettings.SampleRate)
				throw new Exception("Sample rate must be positive and less than project sample rate");

			double tolerance = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
			var transform = Transform.ChangeBasis(Plane.WorldXY, plane);

			var start = Point3d.Origin;
			var bbox = new BoundingBox(start, start);
			curves.ForEach(c => { 
				c.Transform(transform); 
				bbox.Union(c.GetBoundingBox(true)); 
			});

			var width = bbox.GetEdges()[0].Length;
			var count = (int)(width / X * sampleRate);

			
			var cuttingPlane = new Plane(plane);
			cuttingPlane.XAxis = plane.ZAxis;
			cuttingPlane.ZAxis = -plane.XAxis;
			cuttingPlane.Origin = start;

			#region display
			var boundsRect = new Polyline(new Point3d[4] {
				bbox.Corner(true, true, true),
				bbox.Corner(false, true, true),
				bbox.Corner(false, false, true),
				bbox.Corner(true, false, true) });

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
				var line = new Line(new Point3d(start.X + i * X, -Y, 0), span);
				line.Transform(transform);
				timeIntervals.Add(line);
			}

			boundingBox = bounds.GetBoundingBox(true);
			#endregion

			int repeats = (int)SirenSettings.SampleRate / sampleRate;

			var bufferOscillator = new List<byte>();
			var bufferTrigger = new List<byte>();

			for (int i = 0; i < count; i++)
			{
				var silence = true;
				float value = 0.0f;
				int j = 0;
				do {
					var intersection = Rhino.Geometry.Intersect.Intersection.CurvePlane(curves[j], cuttingPlane, tolerance);
					if (intersection != null)
					{
						silence = false;
						var point = intersection.First().PointA;
						value = (float)Math.Max(Math.Min((point.Y / Y), 1), -1);
					}
					j++;
				}
				while (silence && j < curves.Count);

				var sampleOscillator = BitConverter.GetBytes(Convert.ToInt16(short.MaxValue * value));
				var sampleTrigger = BitConverter.GetBytes(Convert.ToInt16(short.MaxValue * Convert.ToSingle(!silence)));

				for (int k = 0; k < repeats; k++)
				{
					bufferOscillator.AddRange(sampleOscillator);
					bufferTrigger.AddRange(sampleTrigger);
				}
					
				cuttingPlane.Origin += new Point3d(width / count, 0, 0);
			}

			var bufferOscillatorStream = bufferOscillator.ToArray();
			var bufferTriggerStream = bufferTrigger.ToArray();

			var waveOscillator = new RawSourceWaveStream(bufferOscillatorStream, 0, bufferOscillatorStream.Length, new WaveFormat(SirenSettings.SampleRate, 1));
			var waveTrigger = new RawSourceWaveStream(bufferTriggerStream, 0, bufferTriggerStream.Length, new WaveFormat(SirenSettings.SampleRate, 1));

			DA.SetData(0, waveOscillator);
			DA.SetData(1, waveTrigger);
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

		public override BoundingBox ClippingBox => boundingBox;

		public override void DrawViewportWires(IGH_PreviewArgs args)
		{
			base.DrawViewportWires(args);
			if (this.Hidden) return;
			args.Display.DrawCurve(bounds, Color.Red);
			foreach (Line l in timeIntervals)
			{
				args.Display.DrawLine(l, Color.Red);
			}
		}
	}
}