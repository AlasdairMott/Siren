using NAudio.Wave;
using System;

namespace Siren.SampleProviders
{
	public class DelayProvider : ISampleProvider
	{
		private ISampleProvider source;
		private int delay;
		private float feedback;

		public WaveFormat WaveFormat => source.WaveFormat;

		public DelayProvider(ISampleProvider source, TimeSpan time, float feedback)
		{
			this.source = source;
			this.delay = (int) time.TotalSeconds * source.WaveFormat.SampleRate;
			this.feedback = feedback;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int sampleRead = source.Read(buffer, offset, count);
			for (int n = 0; n < sampleRead; n++)
			{
				//buffer[offset + n] = 
			}
			return sampleRead;
		}
	}
}
