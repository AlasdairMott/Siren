using Grasshopper.Kernel;
using NAudio.Wave;
using NAudio.Dsp;
using System;

namespace GHSynth
{
	public class MultimodeFilterComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the MultimodeFilterComponent class.
		/// </summary>
		public MultimodeFilterComponent()
		  : base("MultimodeFilterComponent", "Nickname",
			  "Description",
			  "GHSynth", "Subcategory")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
			pManager.AddNumberParameter("Cutoff Freqeuncy", "F", "Cutoff Freqeuncy", GH_ParamAccess.item);
			pManager.AddNumberParameter("Resonance", "Q", "Resonance", GH_ParamAccess.item);
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
			int sampleRate = GHSynthSettings.SampleRate;

			var wave = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat());
			if (!DA.GetData(0, ref wave)) return;

			var cutoff = 1.0;
			if (!DA.GetData(1, ref cutoff)) return;

			var q = 1.0;
			if (!DA.GetData(2, ref q)) return;

			var filter = BiQuadFilter.LowPassFilter(sampleRate, (float) cutoff, (float) q);

			var filtered = new SampleProviders.FilteredAudioProvider(wave.ToSampleProvider(), filter);

			wave.Position = 0;
			var stream = NAudioUtilities.WaveProviderToWaveStream(
				filtered, 
				(int)wave.Length,
				wave.WaveFormat);
			wave.Position = 0;

			DA.SetData(0, stream);
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