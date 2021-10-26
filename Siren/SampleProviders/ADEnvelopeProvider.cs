using System;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class ADEnvelopeProvider : ISampleProvider
    {
        private readonly ISampleProvider _pulses;

        private float _envelope;
        private float _power;
        private readonly float _rise;
        private readonly float _fall;
        private readonly float _exponent;
        private bool _rising;
        private bool _triggered;

        public WaveFormat WaveFormat => _pulses.WaveFormat;

        public ADEnvelopeProvider(ISampleProvider pulses, TimeSpan attack, TimeSpan decay, float exponent)
        {
            _pulses = pulses;
            _envelope = 0;
            _power = 0;
            _rise = Math.Min((float)(1.0 / (attack.TotalSeconds * pulses.WaveFormat.SampleRate)), 1.0f);
            _fall = Math.Min((float)(1.0 / (decay.TotalSeconds * pulses.WaveFormat.SampleRate)), 1.0f);
            _exponent = Math.Max(0f, exponent);
            _rising = false;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var pulseBuffer = new float[count];
            int samplesRead = _pulses.Read(pulseBuffer, offset, count); //read the pulse signal
            if (samplesRead == 0)
            {
                if (_envelope == 0f) return 0;
                else if (samplesRead == 0) samplesRead = WaveFormat.SampleRate;
            }

            for (int n = 0; n < samplesRead; n++)
            {
                float p = 0f;
                if (n < pulseBuffer.Length)
                    p = pulseBuffer[offset + n];

                if (p < 0.1) _triggered = false;
                else if (p > 0.1 & !_triggered)
                {
                    _triggered = true;
                    _rising = true;
                    _envelope = (float)Math.Pow(_power, _exponent);

                }
                if (_rising && _power < 1.0f)
                {
                    _envelope += _rise;
                    _envelope = SirenUtilities.Clamp(_envelope, 0.0f, 1.0f);
                    _power = (float)Math.Pow(_envelope, 1.0f / _exponent);
                }
                else
                {
                    _rising = false;
                    _envelope -= _fall;
                    _envelope = SirenUtilities.Clamp(_envelope, 0.0f, 1.0f);
                    _power = (float)Math.Pow(_envelope, _exponent);
                }

                if (float.IsNaN(_envelope)) throw new System.Exception("NaN exception");

                buffer[offset + n] = _power;
            }

            return samplesRead;
        }


    }
}
