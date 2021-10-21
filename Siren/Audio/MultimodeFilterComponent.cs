using Grasshopper.Kernel;
using NAudio.Dsp;
using System;

namespace Siren
{
	public class MultimodeFilterComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the MultimodeFilterComponent class.
		/// </summary>
		public MultimodeFilterComponent()
		  : base("Multimode Filter", "MMF",
			  "Subtracts frequencies with a specified range from a signal.",
			  "Siren", "Effects")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
			pManager.AddParameter(new WaveStreamParameter(), "Frequency CV", "f", "Frequency CV", GH_ParamAccess.item);
			pManager.AddNumberParameter("Cutoff Frequency", "F", "Cutoff Frequency", GH_ParamAccess.item);
			pManager.AddNumberParameter("Frequency CV amount", "cv", "Frequency CV amount", GH_ParamAccess.item);
			pManager.AddNumberParameter("Resonance", "Q", "Resonance", GH_ParamAccess.item);

			pManager[1].Optional = true;
			pManager[2].Optional = true;
			pManager[3].Optional = true;
			pManager[4].Optional = true;
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
			int sampleRate = SirenSettings.SampleRate;

			var waveIn = CachedSound.Empty;
			if (!DA.GetData(0, ref waveIn)) return;

			var cvIn = CachedSound.Empty;
			if (!DA.GetData("Frequency CV", ref cvIn)) return;

			var cutoff = 10.0;
			DA.GetData("Cutoff Frequency", ref cutoff);
			cutoff = SirenUtilities.Clamp((float) cutoff, -10f, 10f);

			var cvAmount = 0.0;
			DA.GetData("Frequency CV amount", ref cvAmount);

			var q = 1.0;
			DA.GetData("Resonance", ref q);

			var filter = BiQuadFilter.LowPassFilter(sampleRate, (float) cutoff, (float) q);

			var filtered = new SampleProviders.FilteredAudioProvider(
				waveIn.ToSampleProvider(),
				cvIn.ToSampleProvider(), 
				filter,
				(float) cutoff,
				(float) cvAmount,
				(float) q);

			DA.SetData(0, filtered);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.filter;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("7e1099b4-4b91-41bb-8e5d-e719ea45333e"); }
		}
	}

	
}