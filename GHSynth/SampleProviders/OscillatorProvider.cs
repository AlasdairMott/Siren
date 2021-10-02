using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace GHSynth.SampleProviders
{
	public class OscillatorProvider : ISampleProvider
	{
		private ISampleProvider source;
		private SignalGenerator signalGenerator;
		private double octave;
		private double semi;

		public WaveFormat WaveFormat => source.WaveFormat;

		public OscillatorProvider(ISampleProvider source, SignalGenerator signalGenerator, double octave, double semi)
		{
			this.source = source;
			this.signalGenerator = signalGenerator;
			this.octave = octave;
			this.semi = semi;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int sampleRead = source.Read(buffer, offset, count);
			for (int n = 0; n < sampleRead; n++)
			{
				var cv = (buffer[offset + n]) * 10 - 1 + semi*(1.0/12.0);
				signalGenerator.Frequency = (float) Math.Pow(2, cv + octave - 1) * 55;
				var sample = new float[1];
				signalGenerator.Read(sample, 0, 1);
				buffer[offset + n] = sample[0];
			}
			return sampleRead;
		}

	}
}
