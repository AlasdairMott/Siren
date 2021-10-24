using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class SampleHoldProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly ISampleProvider _pulses;
        private float _sample;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public SampleHoldProvider(ISampleProvider source, ISampleProvider pulses)
        {
            _source = source;
            _pulses = pulses;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            float[] pulseBuffer = new float[buffer.Length];
            int cvSampleRead = _pulses.Read(pulseBuffer, offset, count);

            int sampleRead = _source.Read(buffer, offset, count);

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
                    _sample = buffer[offset + n];
                }
                buffer[offset + n] = _sample;
            }
            return sampleRead;
        }
    }
}
