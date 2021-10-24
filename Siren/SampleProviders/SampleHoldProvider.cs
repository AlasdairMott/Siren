using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class SampleHoldProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly ISampleProvider pulses;
        private float sample;

        public WaveFormat WaveFormat => source.WaveFormat;

        public SampleHoldProvider(ISampleProvider source, ISampleProvider pulses)
        {
            this.source = source;
            this.pulses = pulses;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            float[] pulseBuffer = new float[buffer.Length];
            int cvSampleRead = pulses.Read(pulseBuffer, offset, count);

            int sampleRead = source.Read(buffer, offset, count);

            bool triggered = false;
            for (int n = 0; n < sampleRead; n++)
            {
                float p = 0f;
                if (n < pulseBuffer.Length)
                    p = pulseBuffer[offset + n];

                if (p < 0.1) triggered = false;
                else if (p > 0.1 & !triggered)
                {
                    triggered = true;
                    sample = buffer[offset + n];
                }
                buffer[offset + n] = sample;
            }
            return sampleRead;
        }
    }
}
