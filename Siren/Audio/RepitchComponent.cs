using Grasshopper.Kernel;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;

namespace Siren
{
	public class RepitchComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the RepitchComponent class.
		/// </summary>
		public RepitchComponent()
		  : base("RepitchComponent", "Nickname",
			  "Description",
			  "Siren", "Effects")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
			pManager.AddNumberParameter("Pitch", "P", "PitchChange", GH_ParamAccess.item);
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

			double p = 1;
			if (!DA.GetData(1, ref p)) return;

			var semitone = Math.Pow(2, 1.0 / 12);
			var upOneTone = semitone * semitone;
			var downOneTone = 1.0 / upOneTone;
			
			var pitchShifted = new SmbPitchShiftingSampleProvider(waveIn.ToSampleProvider());
			pitchShifted.PitchFactor = (float)(upOneTone * p); // or downOneTone

			DA.SetData(0, pitchShifted);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.repitch;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("2afb9e8d-b7ca-4111-b3e1-e04b1249a134"); }
		}
	}
}