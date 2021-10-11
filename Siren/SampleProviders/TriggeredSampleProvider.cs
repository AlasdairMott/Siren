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
		private Queue<float> cache;

		public WaveFormat WaveFormat => sample.WaveFormat;
		public long Length { get; private set; }

		public TriggeredSampleProvider(CachedSound sample, ISampleProvider pulses) 
		{
			this.sample = sample;
			this.pulses = pulses;
			cache = new Queue<float>();
		}

		public int Read(float[] buffer, int offset, int count)
		{
			var pulseBuffer = new float[count];
			int samplesRead = pulses.Read(pulseBuffer, offset, count); //read the pulse signal
			if (samplesRead == 0)
			{
				if (cache.Count == 0) return 0;
				else if (samplesRead == 0) samplesRead = Math.Min(cache.Count, count);
			}

			bool triggered = false;
			for (int n = 0; n < samplesRead; n++)
			{
				float p = 0f;
				if (n < pulseBuffer.Length)
					p = pulseBuffer[offset + n];

				if (p < 0.1) triggered = false;
				else if (p > 0.1 & !triggered)
				{
					triggered = true;
					cache = new Queue<float>(sample.AudioData); //fill the cache with sample audio
				}
				if (cache.Count > 0)
				{
					buffer[n + offset] = cache.Dequeue(); //replace the buffer with audio from the cache
				}
			}

			return samplesRead;
		}
	}
}
