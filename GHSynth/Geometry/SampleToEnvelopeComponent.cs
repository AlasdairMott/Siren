using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NAudio.Wave;
using Rhino.Geometry;

namespace GHSynth.Geometry
{
	public class SampleToEnvelopeComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the SampleToEnvelopeComponent class.
		/// </summary>
		public SampleToEnvelopeComponent()
		  : base("SampleToEnvelopeComponent", "Nickname",
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
			pManager.AddIntegerParameter("Resolution", "R", "Resolution of the display", GH_ParamAccess.item);
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

			int resolution = 10;
			DA.GetData(1, ref resolution);

			wave.Position = 0;
			var polyline = GeometryFunctions.ISampleToPolyline(wave.ToSampleProvider(), resolution);
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
			get { return new Guid("8489807f-419a-4294-a170-8792c43c79f0"); }
		}
	}
}