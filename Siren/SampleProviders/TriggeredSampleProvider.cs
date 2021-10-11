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

					Array.Copy(sample.AudioData, 0, outBuffer, offset + n, length);
				}
			}
			//buffer = outBuffer.Take(samplesRead).ToArray();
			for (int n =0; n < samplesRead; n++)
			{
				buffer[n] = outBuffer[n];
			}

			Position += count;

			return samplesRead;
		}
	}
}
