//using NAudio.Wave;
//using NAudio.Wave.SampleProviders;
//using System;
//using System.Linq;
//using System.Collections.Generic;

//namespace GHSynth.SampleProviders
//{
//	public class TriggeredSampleProvider : ISampleProvider
//	{
//		private RawSourceWaveStream sample;
//		private MixingSampleProvider mixer;
//		private List<double> triggers;
//		public WaveFormat WaveFormat => sample.WaveFormat;

//		public TriggeredSampleProvider(RawSourceWaveStream sample, List<double> triggers)
//		{
//			this.sample = sample;
//			this.triggers = triggers;

//			mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(GHSynthSettings.SampleRate, 1));
//			var offsets = triggers.Select(t =>
//				new OffsetSampleProvider(sample.ToSampleProvider())
//				{ DelayBy = TimeSpan.FromSeconds(t) });
//			foreach (var offset in offsets) { mixer.AddMixerInput(offset); }
//		}

//		public int Read(float[] buffer, int offset, int count)
//		{
//			int sampleRead = mixer.Read(buffer, offset, count);

//			var triggersQueue = new Queue<double>(triggers);
//			var sampled = sample.Read()

//			for (int n = 0; n < triggers.Last() + sample.Length; n++)
//			{
//				if ((int) (triggersQueue.Peek() * WaveFormat.SampleRate) == n) 
//				{
//					sample.Position = 0;
//					triggersQueue.Dequeue();
//				}
//				buffer[offset + n] = sample[offset + n]
//				//read mixed down output
//				//if trigger at a time, reset sample position?

//				//var cv = (buffer[offset + n]) * 10 - 1 + semi * (1.0 / 12.0);
//				//signalGenerator.Frequency = (float)Math.Pow(2, cv + octave - 1) * 55;
//				//var sample = new float[1];
//				//signalGenerator.Read(sample, 0, 1);
//				//buffer[offset + n] = sample[0];
//			}
//			return sampleRead;
//		}
//	}
//}
