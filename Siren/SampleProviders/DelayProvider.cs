using NAudio.Wave;
using System;

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
			cache = new float[this.delay];
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int sampleRead = source.Read(buffer, offset, count);

			for (int n = 0; n < sampleRead; n++)
			{
				if (n > delay) {
					var n_0 = buffer[offset + n - delay + 2];
					var n_1 = buffer[offset + n - delay + 1];
					var n_3 = buffer[offset + n - delay];

					delayedSample = (n_0 + n_1 + n_3) * 0.333f;
				}

				var sample = buffer[offset + n] + delayedSample * feedback;
				sample = SirenUtilities.Clamp(sample, -1.0f, 1.0f);
				buffer[offset + n] = sample;
			}
			return sampleRead;
		}
	}
}
