using NAudio.Wave;
using System;
using System.Linq;

namespace Siren.SampleProviders
{
	public class DelayProvider : ISampleProvider
	{
		private ISampleProvider source;
		private int delay;
		private float feedback;
		private float delayedSample;
		private float[] cache;

		public WaveFormat WaveFormat => source.WaveFormat;

		public DelayProvider(ISampleProvider source, TimeSpan time, float feedback)
		{
			this.source = source;
			this.delay = (int) (time.TotalSeconds * source.WaveFormat.SampleRate);
			this.feedback = feedback;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int sampleRead = source.Read(buffer, offset, count);

			for (int n = 0; n < sampleRead; n++)
			{
				if (n > delay)
				{
					delayedSample = buffer[offset + n - delay];
				}
				else if ( cache != null && cache.Length > delay)
				{
					delayedSample = cache[cache.Length - (delay - n) - 1];
				}

				var sample = buffer[offset + n] + delayedSample * feedback;
				sample = SirenUtilities.Clamp(sample, -1.0f, 1.0f);
				buffer[offset + n] = sample;
			}

			cache = buffer;
			return sampleRead;
		}


	}
}
