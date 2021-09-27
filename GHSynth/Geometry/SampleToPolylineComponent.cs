using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NAudio.Wave;
using Rhino.Geometry;

namespace GHSynth.Geometry
{
	public class SampleToPolylineComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the SampleToCurveComponent class.
		/// </summary>
		public SampleToPolylineComponent()
		  : base("SampleToCurveComponent", "Nickname",
			  "Description",
			  "GHSynth", "Geometry")
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
			pManager.AddCurveParameter("Polyline", "P", "Polyline", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			var wave = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat());
			if (!DA.GetData(0, ref wave)) return;

			var sample = wave.ToSampleProvider();
			var buffer = new float[wave.Length];
			sample.Read(buffer, 0, (int) wave.Length);

			var polyline = new Polyline();
			for (int i = 0; i < buffer.Length; i ++) 
			{
				polyline.Add(new Point3d(i * 0.01, buffer[i], 0));
			}

			DA.SetData(0, polyline);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				//You can add image files to your project resources and access them like this:
				// return Resources.IconForThisComponent;
				return null;
			}
		}

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("63c5f034-e7a0-4b01-b1be-d51bfcd2c786"); }
		}
	}
}