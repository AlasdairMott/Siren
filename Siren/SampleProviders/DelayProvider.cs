using System;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class DelayProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly int _delay;
        private readonly float _feedback;
        private float _delayedSample;
        private readonly float _colour;
        private float[] _cache;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public DelayProvider(ISampleProvider source, TimeSpan time, float feedback, float colour)
        {
            _source = source;
            _delay = (int)(time.TotalSeconds * source.WaveFormat.SampleRate);
            _feedback = feedback;
            _colour = SirenUtilities.Clamp(colour, 0.0f, 1.0f);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int sampleRead = _source.Read(buffer, offset, count);

            for (int n = 0; n < sampleRead; n++)
            {
                float s = 0f;
                if (n > _delay)
                {
                    s = buffer[offset + n - _delay];
                }
                else if (_cache != null && _cache.Length > _delay)
                {
                    s = _cache[_cache.Length - (_delay - n) - 1];
                }

                _delayedSample = s * _colour + _delayedSample * (1 - _colour);

                var sample = buffer[offset + n] + _delayedSample * _feedback;
                sample = SirenUtilities.Clamp(sample, -1.0f, 1.0f);
                buffer[offset + n] = sample;
            }

            _cache = buffer;
            return sampleRead;
        }


    }
}
