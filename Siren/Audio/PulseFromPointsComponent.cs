using Grasshopper.Kernel;
using Rhino.Geometry;
using Siren.SampleProviders;
using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace Siren.Audio
{
	public class PulseFromPointsComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the PulseFromPointsComponent class.
		/// </summary>
		public PulseFromPointsComponent()
		  : base("Pulse From Points", "CVPt",
			  "Description",
			  "Siren", "CV Control")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddPointParameter("Point", "Pt", "Points to read", GH_ParamAccess.list);
			pManager.AddPlaneParameter("Plane", "P", "Origin and orienation of the curve", GH_ParamAccess.item);
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
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			var points = new List<Point3d>();
			var plane = Plane.WorldXY;
			double X = SirenSettings.TimeScale;
			double Y = SirenSettings.AmplitudeScale;
			var sampleRate = SirenSettings.SampleRate;

			if (!DA.GetDataList(0, points)) return;
			DA.GetData(1, ref plane);
			DA.GetData(2, ref X); if (X <= 0) throw new Exception("T must be positive");
			DA.GetData(3, ref Y); if (Y <= 0) throw new Exception("A must be positive");
			
			var triggerTimes = new List<double>();
			foreach (var p in points) 
			{
				plane.RemapToPlaneSpace(p, out Point3d pt);
				if (pt.X >= 0) triggerTimes.Add(pt.X / X);
			}

			var waveFormat = new WaveFormat(SirenSettings.SampleRate, 1);
			var pulseSignal = new PulseProvider(triggerTimes, waveFormat);

			DA.SetData(0, pulseSignal);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.pointToPulse;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("b2a639fd-abbe-4206-87c7-ce73c8f73ed8"); }
		}
	}
}