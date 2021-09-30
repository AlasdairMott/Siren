using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace GHSynth.SampleProviders
{
	public class OscillatorProvider : ISampleProvider
	{
		private ISampleProvider source;
		private SignalGenerator signalGenerator;
		//private OffsetSampleProvider offsetProvider;

		public WaveFormat WaveFormat => source.WaveFormat;

		public OscillatorProvider(ISampleProvider source, SignalGenerator signalGenerator)
		{
			this.source = source;
			this.signalGenerator = signalGenerator;
			//offsetProvider = new OffsetSampleProvider(signalGenerator);
			//offsetProvider.TakeSamples = 1;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int sampleRead = source.Read(buffer, offset, count);
			for (int n = 0; n < sampleRead; n++)
			{
				//signalGenerator.Frequency = 440;
				signalGenerator.Frequency = (float) Math.Pow(2, (buffer[offset + n]*8) - 1) * 55;
				var sample = new float[1];
				signalGenerator.Read(sample, 0, 1);
				buffer[offset + n] = sample[0];



				
			}
			return sampleRead;
		}

	}
}
