using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dsp;
using NAudio.Wave;

namespace GHSynth.SampleProviders
{
	public class VCAProvider : ISampleProvider
	{
		private ISampleProvider source1;
		private ISampleProvider source2;

		public WaveFormat WaveFormat => source1.WaveFormat;

		public VCAProvider(ISampleProvider source1, ISampleProvider source2)
		{
			this.source1 = source1;
			this.source2 = source2;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int sampleRead = source1.Read(buffer, offset, count);

			float[] buffer2 = new float[buffer.Length];
			int sample2Read = source2.Read(buffer2, offset, count);

			for (int n = 0; n < sampleRead; n++)
			{
				buffer[offset + n] *= Math.Max(Clamp(buffer2[offset + n], 1f), 0);
			}
			return sampleRead;
		}

		private float Clamp(float input, float ceiling) 
		{
			if (input > ceiling) return ceiling;
			return input;
		}
	}
}
