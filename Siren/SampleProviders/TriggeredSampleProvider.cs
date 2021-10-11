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
		private List<float> cache;

		public WaveFormat WaveFormat => sample.WaveFormat;
		public long Length { get; private set; }
		public int Position { get; set; }

		public TriggeredSampleProvider(CachedSound sample, ISampleProvider pulses) 
		{
			this.sample = sample;
			this.pulses = pulses;
			cache = new List<float>();

			Position = 0;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			//var pulseBuffer = new float[count];

			int samplesRead = pulses.Read(buffer, offset, count);
			if (samplesRead == 0) return 0;

			var outBuffer = new float[samplesRead + sample.AudioData.Length];
			Array.Copy(cache.ToArray(), 0, outBuffer, 0, Math.Min(cache.Count, outBuffer.Length));
			cache.Clear();

			bool triggered = false;
			for (int n = 0; n < samplesRead; n++)
			{
				var value = buffer[offset + n];
				if (value < 0.1) triggered = false;
				else if (value > 0.1 & !triggered)
				{
					triggered = true;

					var location = n - Position;
					var remaining = buffer.Count() - location;
					var length = Math.Min(sample.AudioData.Length, remaining);

					if (length < sample.AudioData.Length) 
					{ 
						var samplesToCache = new float[sample.AudioData.Length - length];
						Array.Copy(sample.AudioData, length, samplesToCache, 0, samplesToCache.Length);
						cache.AddRange(samplesToCache);
					}

					Array.Copy(sample.AudioData, 0, outBuffer, offset + n, length);
				}
			}
			//buffer = outBuffer.Take(samplesRead).ToArray();

			Array.Copy(outBuffer, 0, buffer, 0, Math.Min(buffer.Length, outBuffer.Length));

			Position += count;

			return samplesRead;
		}
	}
}
