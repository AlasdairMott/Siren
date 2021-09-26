using NAudio.Dsp;
using NAudio.Wave;

namespace GHSynth.SampleProviders
{
	public class FilteredAudioProvider : ISampleProvider
	{
		private ISampleProvider source;
		private BiQuadFilter filter;

		public WaveFormat WaveFormat => source.WaveFormat;

		public FilteredAudioProvider(ISampleProvider source, BiQuadFilter filter)
		{
			this.source = source;
			this.filter = filter;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int sampleRead = source.Read(buffer, offset, count);
			for (int n = 0; n < sampleRead; n++)
			{
				buffer[offset + n] = filter.Transform(buffer[offset + n]);
			}
			return sampleRead;
		}
	}
}
