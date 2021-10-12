using Siren.SampleProviders;
using Grasshopper.Kernel;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Siren.Audio
{
	public class SamplePlayerComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the SamplePlayerComponent class.
		/// </summary>
		public SamplePlayerComponent()
		  : base("Sample Player", "SampleP",
			  "Description",
			  "Siren", "Oscillators")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Sample", "W", "Sample input", GH_ParamAccess.item);
			pManager.AddParameter(new WaveStreamParameter(), "Pulse", "P", "Pulse input", GH_ParamAccess.item);
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
			var waveIn = CachedSound.Empty;
			if (!DA.GetData(0, ref waveIn)) return;

			var pulseIn = CachedSound.Empty;
			if (!DA.GetData(1, ref pulseIn)) return;

			var waveOut = new TriggeredSampleProvider(waveIn, pulseIn.ToSampleProvider());

			DA.SetData(0, waveOut);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.sampleTrigger;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("75a5ac64-a05c-4639-8532-a7935ae30e39"); }
		}
	}
}