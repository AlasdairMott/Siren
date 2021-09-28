using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

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
			pManager.AddPointParameter("Points", "P", "Debugging points", GH_ParamAccess.item);
			//pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave output", GH_ParamAccess.item);
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
			var sampleRate = 44100;

			DA.GetData(0, ref curve);
			DA.GetData(1, ref plane);
			DA.GetData(2, ref X); if (X <= 0) throw new Exception("T must be positive");
			DA.GetData(3, ref Y); if (Y <= 0) throw new Exception("A must be positive");
			DA.GetData(4, ref sampleRate); if (sampleRate <= 0) throw new Exception("Sample rate must be positive");

			double tolerance = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

			var bbox = curve.GetBoundingBox(plane);
			var width = bbox.GetEdges()[0].Length;
			var count = (int) ( width / X) * sampleRate;

			var start = bbox.GetEdges()[1].PointAt(0.5);
			var cuttingPlane = new Plane(plane);
			cuttingPlane.Origin = start;

			var buffer = new float[count];
			var points = new Point3d[count];
			for (int i = 0; i < count; i++) 
			{
				var intersection = Rhino.Geometry.Intersect.Intersection.CurvePlane(curve, cuttingPlane, tolerance);

				var point = intersection.First().PointA;
				plane.ClosestParameter(point, out double s, out double t);
				buffer[i] = (float) (t / Y);
				points[i] = point;

				cuttingPlane.Origin += new Point3d(width / count, 0, 0);
			}

			DA.SetData(0, points);
			
			
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