using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class CVQuantizer : ISampleProvider
    {
        private readonly List<TimeSpan> _triggers = new List<TimeSpan>();
        private readonly ISampleProvider _source;
        private readonly List<double> _scale;
        private float _prev;
        private int _position;

        public WaveFormat WaveFormat => _source.WaveFormat;
        public ISampleProvider Triggers => new PulseProvider(_triggers, WaveFormat);

        public CVQuantizer(ISampleProvider source, List<double> scale)
        {
            _source = source;
            _scale = scale;
            _position = 0;
            _prev = 0;
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

                _position++;
                if (sample != _prev)
                {
                    _triggers.Add(TimeSpan.FromSeconds(((float)_position) / WaveFormat.SampleRate));
                    _prev = sample;
                }
            }
            return sampleRead;
        }
    }
}
