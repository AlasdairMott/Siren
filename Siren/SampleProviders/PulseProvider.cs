using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Siren.SampleProviders
{
	public class PulseProvider : ISampleProvider
	{
		private int pulseLength;
		private int[] times;
		private List<float> cache;

		public WaveFormat WaveFormat { get; private set; }
		public long Length { get; private set; }
		public int Position { get; set; }

		public PulseProvider(List<double> times, WaveFormat waveFormat)
		{
			pulseLength = (int) (TimeSpan.FromMilliseconds(1).TotalSeconds * waveFormat.SampleRate);
			this.times = times.OrderBy(t => t).ToArray().Select(t => (int)(t * waveFormat.SampleRate)).ToArray();
			cache = new List<float>();

			WaveFormat = waveFormat;
			Length = this.times.Last() + pulseLength;
			Position = 0;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			if (Position > Length) return 0; //end of pulses

			int samplesRead = Math.Min((int) Length - Position, count);

			var pulse = Enumerable.Repeat(0.5f, pulseLength).ToArray();
			foreach (var t in times)
			{
				if (t < Position || t > Position + samplesRead) continue;

				var location = t - Position;
				var remaining = buffer.Count() - location;
				var length = Math.Min(pulseLength, remaining);
				Array.Copy(pulse, 0, buffer, location, length);
			}

			Position += count;

			return samplesRead;
		}
	}
}
