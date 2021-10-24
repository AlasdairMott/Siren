using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Siren.SampleProviders
{
    public class OscillatorProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly SignalGenerator _signalGenerator;
        private readonly double _octave;
        private readonly double _semi;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public OscillatorProvider(ISampleProvider source, SignalGenerator signalGenerator, double octave, double semi)
        {
            _source = source;
            _signalGenerator = signalGenerator;
            _octave = octave;
            _semi = semi;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int sampleRead = _source.Read(buffer, offset, count);
            for (int n = 0; n < sampleRead; n++)
            {
                var cv = (buffer[offset + n]) * 10 - 1 + _semi * (1.0 / 12.0);
                _signalGenerator.Frequency = (float)Math.Pow(2, cv + _octave - 1) * 55;
                var sample = new float[1];
                _signalGenerator.Read(sample, 0, 1);
                buffer[offset + n] = sample[0];
            }
            return sampleRead;
        }

    }
}
