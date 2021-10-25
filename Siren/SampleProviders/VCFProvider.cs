using System;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    //filter design from https://www.musicdsp.org/en/latest/Filters/26-moog-vcf-variation-2.html

    public class VCFProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly ISampleProvider _frequency;
        private readonly float _resonance;

        private float _out1, _out2, _out3, _out4 = 0.0f;
        private float _in1, _in2, _in3, _in4 = 0.0f;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public VCFProvider(ISampleProvider source, ISampleProvider frequency, float resonance)
        {
            _source = source;
            _frequency = frequency;
            _resonance = resonance;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);

            float[] bufferCv = new float[buffer.Length];
            int samplesReadCv = _frequency.Read(bufferCv, offset, count);

            var nyquist = _source.WaveFormat.SampleRate * 0.5f;
            for (int n = 0; n < Math.Min(samplesRead, samplesReadCv); n++)
            {
                var cvf = SirenUtilities.Clamp(bufferCv[offset + n], -1, 1);
                cvf = cvf * 0.5f + 0.5f;
                var f = (cvf) * 1.16f;
                var fb = _resonance * (1.0f - 0.15f * f * f);

                var input = buffer[offset + n];
                input -= _out4 * fb;
                input *= 0.35013f * (f * f) * (f * f);

                _out1 = input + 0.3f * _in1 + (1f - f) * _out1; // Pole 1
                _in1 = input;
                _out2 = _out1 + 0.3f * _in2 + (1f - f) * _out2; // Pole 2
                _in2 = _out1;
                _out3 = _out2 + 0.3f * _in3 + (1f - f) * _out3; // Pole 3
                _in3 = _out2;
                _out4 = _out3 + 0.3f * _in4 + (1f - f) * _out4; // Pole 4
                _in4 = _out3;

                buffer[offset + n] = _out4;

            }
            return samplesRead;
        }
    }
}
