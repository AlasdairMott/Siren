using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Siren.SampleProviders
{
	public class TriggeredSampleProvider : ISampleProvider
	{
		private CachedSound sample;
		private ISampleProvider pulses;

		public WaveFormat WaveFormat => sample.WaveFormat;

		public TriggeredSampleProvider(CachedSound sample, ISampleProvider pulses) 
		{
			this.sample = sample;
			this.pulses = pulses;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int sampleRead = pulses.Read(buffer, offset, count);
			var outBuffer = new float[sampleRead + sample.AudioData.Length];

			bool triggered = false;

			for (int n = 0; n < sampleRead; n++)
			{
				var value = buffer[offset + n];
				if (value < 0.1) triggered = false;
				else if (value > 0.1 & !triggered)
				{
					triggered = true;
					Array.Copy(sample.AudioData, 0, outBuffer, offset + n, sample.AudioData.Length);
				}
			}
			buffer = outBuffer;
			return outBuffer.Length;
		}
	}
}
