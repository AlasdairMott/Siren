using System;
using System.Linq;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class FMOscillatorProvider : ISampleProvider
    {
        private readonly ISampleProvider _cvSource;
        private readonly ISampleProvider _opSource;
        private readonly ISampleProvider _opDepth;

        private float _phase;
        private float _phaseIncrement;
        private const float PI = (float)Math.PI;
        private const float TwoPI = 2f * PI;
        private readonly float _gain = 0.5f;

        public FMOscillatorProvider(ISampleProvider cv, ISampleProvider op, ISampleProvider depth)
        {
            this._cvSource = cv;
            this._opSource = op;
            this._opDepth = depth;
        }

        public WaveFormat WaveFormat => _cvSource.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int cvSamplesRead = _cvSource.Read(buffer, offset, count);

            var opBuffer = new float[buffer.Length];
            int opSamplesRead = _opSource.Read(opBuffer, offset, count);

            var depthBuffer = new float[buffer.Length];
            int depthSamplesRead = _opDepth.Read(depthBuffer, offset, count);

            var samplesRead = new int[3] { cvSamplesRead, opSamplesRead, depthSamplesRead }.Min();

            for (int n = 0; n < samplesRead; n++)
            {
                var pitch = ComputePitch(buffer[offset + n]); //pitch in Hz
                var opSample = opBuffer[offset + n];
                var depth = depthBuffer[offset + n] * (float)(pitch * 50);
                var fm = opSample * depth; //linear fm
                pitch += fm;

                _phaseIncrement = pitch * 2 * PI / WaveFormat.SampleRate;

                buffer[offset + n] = NextSample() * _gain;
            }

            return samplesRead;
        }

        private float ComputePitch(float input)
        {
            var semi = 0;
            var octave = 0;
            var controlVoltage = input * 10 - 1 + semi * (1.0 / 12.0);
            return (float)Math.Pow(2, controlVoltage + octave - 1) * 55;
        }

        private float NextSample()
        {
            float value = 0.0f;
            float t = _phase / TwoPI;
            value = (float)Math.Sin(_phase);

            _phase += _phaseIncrement;
            while (_phase >= TwoPI) _phase -= TwoPI;
            return value;
        }
    }
}
