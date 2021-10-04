using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Siren.SampleProviders
{
	public class CVQuantizer : ISampleProvider
	{
		private ISampleProvider source;
		private List<double> scale;

		public WaveFormat WaveFormat => source.WaveFormat;

		public CVQuantizer(ISampleProvider source, List<double> scale)
		{
			this.source = source;
			this.scale = scale;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			int sampleRead = source.Read(buffer, offset, count);
			for (int n = 0; n < sampleRead; n++)
			{
				var cv = (buffer[offset + n]) * 10 - 1; //1V/O

				var integer = Math.Truncate(cv);
				var real = cv - integer;
				
				real = scale.Aggregate((x, y) => Math.Abs(x - real) < Math.Abs(y - real) ? x : y);
				cv = (float) (integer + real);

				//Transform to cv and back
				var sample = (cv + 1) / 10;
				buffer[offset + n] = sample;
			}
			return sampleRead;
		}
	}
}
