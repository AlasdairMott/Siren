using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class SawtoothProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly double _octave;
        private readonly double _semi;
        private float _moduloCounter;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public SawtoothProvider(ISampleProvider source, double octave, double semi)
        {
            _source = source;
            _octave = octave;
            _semi = semi;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int sampleRead = _source.Read(buffer, offset, count);
            for (int n = 0; n < sampleRead; n++)
            {
                var cv = (buffer[offset + n]) * 10 - 1 + _semi * (1.0 / 12.0);
                var pitch = (float) Math.Pow(2, cv + _octave - 1) * 55;
                var increment = pitch / WaveFormat.SampleRate;
                _moduloCounter += increment;
                if (_moduloCounter > 1.0f) _moduloCounter -= 1.0f;
                buffer[offset + n] = _moduloCounter - 0.5f;
            }
            return sampleRead;
        }
    }
}
