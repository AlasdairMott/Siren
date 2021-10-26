using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class PulseProvider : ISampleProvider
    {
        private readonly int _pulseLength;
        private readonly int[] _times;
        private readonly List<float> _cache;

        public WaveFormat WaveFormat { get; private set; }
        public long Length { get; private set; }
        public int Position { get; set; }

        public PulseProvider(List<TimeSpan> times, WaveFormat waveFormat)
        {
            _pulseLength = (int)(TimeSpan.FromMilliseconds(1).TotalSeconds * waveFormat.SampleRate);
            _times = times.OrderBy(t => t).ToArray().Select(t => (int)(t.TotalSeconds * waveFormat.SampleRate)).ToArray();
            _cache = new List<float>();

            WaveFormat = waveFormat;
            Length = _times.Last() + _pulseLength;
            Position = 0;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (Position > Length) return 0; //end of pulses

            int samplesRead = Math.Min((int)Length - Position, count);

            var pulse = Enumerable.Repeat(0.5f, _pulseLength).ToArray();
            foreach (var t in _times)
            {
                if (t < Position || t > Position + samplesRead) continue;

                var location = t - Position;
                var remaining = buffer.Count() - location;
                var length = Math.Min(_pulseLength, remaining);
                Array.Copy(pulse, 0, buffer, location, length);
            }

            Position += count;

            return samplesRead;
        }
    }
}
