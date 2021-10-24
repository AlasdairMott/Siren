using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class CVQuantizer : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly List<double> _scale;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public CVQuantizer(ISampleProvider source, List<double> scale)
        {
            _source = source;
            _scale = scale;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int sampleRead = _source.Read(buffer, offset, count);
            for (int n = 0; n < sampleRead; n++)
            {
                var cv = (buffer[offset + n]) * 10 - 1; //1V/O

                var integer = Math.Truncate(cv);
                var real = cv - integer;

                real = _scale.Aggregate((x, y) => Math.Abs(x - real) < Math.Abs(y - real) ? x : y);
                cv = (float)(integer + real);

                //Transform to cv and back
                var sample = (cv + 1) / 10;
                buffer[offset + n] = sample;
            }
            return sampleRead;
        }
    }
}
