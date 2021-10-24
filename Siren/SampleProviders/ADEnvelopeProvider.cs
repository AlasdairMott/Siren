using System;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class ADEnvelopeProvider : ISampleProvider
    {
        private readonly ISampleProvider pulses;

        private float envelope;
        private float power;
        private readonly float rise;
        private readonly float fall;
        private readonly float exponent;
        private bool rising;

        public WaveFormat WaveFormat => pulses.WaveFormat;

        public ADEnvelopeProvider(ISampleProvider pulses, TimeSpan attack, TimeSpan decay, float exponent)
        {
            this.pulses = pulses;
            envelope = 0;
            power = 0;
            rise = Math.Min((float)(1.0 / (attack.TotalSeconds * pulses.WaveFormat.SampleRate)), 1.0f);
            fall = Math.Min((float)(1.0 / (decay.TotalSeconds * pulses.WaveFormat.SampleRate)), 1.0f);
            this.exponent = Math.Max(0f, exponent);
            rising = false;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var pulseBuffer = new float[count];
            int samplesRead = pulses.Read(pulseBuffer, offset, count); //read the pulse signal
            if (samplesRead == 0)
            {
                if (envelope == 0f) return 0;
                else if (samplesRead == 0) samplesRead = WaveFormat.SampleRate;
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
                    rising = true;
                    envelope = (float)Math.Pow(power, exponent);

                }
                if (rising && power < 1.0f)
                {
                    envelope += rise;
                    envelope = SirenUtilities.Clamp(envelope, 0.0f, 1.0f);
                    power = (float)Math.Pow(envelope, 1.0f / exponent);
                }
                else
                {
                    rising = false;
                    envelope -= fall;
                    envelope = SirenUtilities.Clamp(envelope, 0.0f, 1.0f);
                    power = (float)Math.Pow(envelope, exponent);
                }

                if (float.IsNaN(envelope)) throw new System.Exception("NaN exception");

                buffer[offset + n] = power;
            }

            return samplesRead;
        }


    }
}