using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using NAudio.Wave;
using NAudio.Dsp;
using NAudio.Wave.SampleProviders;

namespace GHSynth.Components
{
	public class OscillatorComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the OscillatorComponent class.
		/// </summary>
		public OscillatorComponent()
		  : base("OscillatorComponent", "Nickname",
			  "Description",
			  "GHSynth", "Subcategory")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddNumberParameter("Frequency", "F", "Frequency of the note", GH_ParamAccess.item);
			pManager.AddNumberParameter("Duration", "D", "Duration of the note", GH_ParamAccess.item);
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
			int sampleRate = 44100;
			double frequency = 440;
			if (!DA.GetData(0, ref frequency)) return;

			double duration = 1.0;
			if (!DA.GetData(1, ref duration)) return;

			var signal = Oscillator(frequency, duration, SignalGeneratorType.Square);

			var wave = NAudioUtilities.WaveProviderToWaveStream(
				signal.ToWaveProvider16().ToSampleProvider(), 
				signal.TakeSamples,
				new WaveFormat(sampleRate, 1));

			DA.SetData(0, wave);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.sin;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("3fc1b43e-fc6d-413e-a0fb-e9e4d696a449"); }
		}

		public OffsetSampleProvider Oscillator(double frequency, double duration, SignalGeneratorType type) 
		{
			var sampleRate = 44100;
			var signalGenerator = new SignalGenerator(sampleRate, 1);
			signalGenerator.Type = type;
			signalGenerator.Frequency = frequency;
			signalGenerator.Gain = 0.25;
			var offsetProvider = new OffsetSampleProvider(signalGenerator);
			offsetProvider.TakeSamples = (int) (sampleRate * duration);
			return offsetProvider;
		}
	}
}