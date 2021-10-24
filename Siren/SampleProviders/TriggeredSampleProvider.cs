using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class TriggeredSampleProvider : ISampleProvider
    {
        private readonly CachedSound _sample;
        private readonly ISampleProvider _pulses;
        private Queue<float> _cache;

        public WaveFormat WaveFormat => _sample.WaveFormat;
        public long Length { get; private set; }

        public TriggeredSampleProvider(CachedSound sample, ISampleProvider pulses)
        {
            _sample = sample;
            _pulses = pulses;
            _cache = new Queue<float>();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var pulseBuffer = new float[count];
            int samplesRead = _pulses.Read(pulseBuffer, offset, count); //read the pulse signal
            if (samplesRead == 0)
            {
                if (_cache.Count == 0) return 0;
                else if (samplesRead == 0) samplesRead = Math.Min(_cache.Count, count);
            }

            bool triggered = false;
            for (int n = 0; n < samplesRead; n++)
            {
                float p = 0f;
                if (n < pulseBuffer.Length)
                    p = pulseBuffer[offset + n];

                if (p < 0.1) triggered = false;
                else if (p > 0.1 & !triggered)
                {
                    triggered = true;
                    _cache = new Queue<float>(_sample.AudioData); //fill the cache with sample audio
                }
                if (_cache.Count > 0)
                {
                    buffer[n + offset] = _cache.Dequeue(); //replace the buffer with audio from the cache
                }
            }

            return samplesRead;
        }
    }
}
