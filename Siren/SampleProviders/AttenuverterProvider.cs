using NAudio.Wave;

namespace Siren.SampleProviders
{
	public class AttenuverterProvider : ISampleProvider
	{
		private ISampleProvider source;
		private float amount;

		public WaveFormat WaveFormat => source.WaveFormat;

		public AttenuverterProvider(ISampleProvider source, float amount)
		{
			this.source = source;
			this.amount = amount;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int sampleRead = source.Read(buffer, offset, count);
			for (int n = 0; n < sampleRead; n++)
			{
				buffer[offset + n] *= amount;
			}
			return sampleRead;
		}
	}
}
