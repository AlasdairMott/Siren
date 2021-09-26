using Grasshopper.Kernel;
using Skelp.ProtonSynth.Envelopes;
using Skelp.ProtonSynth.SignalGenerators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Threading;

namespace GHSynth
{
	public class ProtonSynthComponent : GH_Component
	{
		private IOscillator osc;
		private IEnvelope ampEnvelope;
		
		/// <summary>
		/// Initializes a new instance of the ProtonSynthComponent class.
		/// </summary>
		public ProtonSynthComponent()
		  : base("ProtonSynthComponent", "Nickname",
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

			

			double maxAmplitude = 1;

			osc = new SawtoothOscillator(sampleRate, frequency, maxAmplitude);
			ampEnvelope = new LinearEnvelope(sampleRate);

			// Setting up envelope parameters
			ampEnvelope.AttackTime = TimeSpan.FromSeconds(1);
			ampEnvelope.DecayTime = TimeSpan.FromSeconds(0.1);
			ampEnvelope.SustainAmplitude = 0.5;
			ampEnvelope.ReleaseTime = TimeSpan.FromSeconds(0.1);

			// Starts the envelope
			ampEnvelope.TriggerOn();

			//Synthesize a single sample (1 out of 44100 per second) and modulate its amplitude



			var rawSource = GetBytes((int) (sampleRate * duration)); //1 second of raw audio
			var stream = new RawSourceWaveStream(rawSource, 0, rawSource.Length, new WaveFormat(44100, 16, 1));

			//var provider = new 

			//using (var outputDevice = new WaveOutEvent())
			//{
			//	outputDevice.Init(stream);
			//	//outputDevice.Init(audioFile);
			//	outputDevice.Play();
			//	while (outputDevice.PlaybackState == PlaybackState.Playing)
			//	{
			//		Thread.Sleep(1000);
			//	}
			//}

			DA.SetData(0, stream);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.wave;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("d33f98c7-6cab-4305-96b7-8d15452b18fb"); }
		}

		//static async Task FillBuffer()
		//{
		//	while (true)
		//	{
		//		if (bwp.BufferedDuration < TimeSpan.FromMilliseconds(128))
		//		{
		//			var bytes = GetBytes(1323);

		//			bwp.AddSamples(bytes, 0, bytes.Length);
		//		}

		//		await Task.Delay(10);
		//	}
		//}

		private byte[] GetBytes(int sampleCount)
		{
			//int SAMPLE_RATE = 44100;
			//short[] wave = new short[sampleCount];
			//byte[]  binaryWave = new byte[SAMPLE_RATE * sizeof(short)];
			//for (int i = 0; i < sampleCount; i++)
			//{
			//	wave[i] = Convert.ToInt16(short.MaxValue * (osc.GetSample() * ampEnvelope.GetAmplitude())); 
			//}
			//Buffer.BlockCopy(wave, 0, binaryWave, 0, wave.Length * sizeof(short));
			//return binaryWave;
			var output = new List<byte>();

			for (int i = 0; i < sampleCount; i++)
			{
				var ampAmplitude = ampEnvelope.GetAmplitude();
				var oscSample = osc.GetSample();
				var result = oscSample * ampAmplitude;

				var nAudioSampleInRange = GetShort(result);
				var nAudioSampleInBytes = BitConverter.GetBytes(nAudioSampleInRange);

				output.AddRange(nAudioSampleInBytes);
			}

			return output.ToArray();
		}

		private short GetShort(double x)
		{
			double oMin = -1;
			double oMax = 1;
			double nMin = short.MinValue;
			double nMax = short.MaxValue;

			var reverseInput = false;
			var oldMin = Math.Min(oMin, oMax);
			var oldMax = Math.Max(oMin, oMax);
			if (oldMin != oMin)
				reverseInput = true;

			var reverseOutput = false;
			var newMin = Math.Min(nMin, nMax);
			var newMax = Math.Max(nMin, nMax);
			if (newMin != nMin)
				reverseOutput = true;

			var portion = (x - oldMin) * (newMax - newMin) / (oldMax - oldMin);
			if (reverseInput)
				portion = (oldMax - x) * (newMax - newMin) / (oldMax - oldMin);

			var result = portion + newMin;
			if (reverseOutput)
				result = newMax - portion;

			return (short)Math.Round(result);
		}
	}
}