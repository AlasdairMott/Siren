using System;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class VCAProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly ISampleProvider cv;

        public WaveFormat WaveFormat => source.WaveFormat;

        public VCAProvider(ISampleProvider source, ISampleProvider cv)
        {
            this.source = source;
            this.cv = cv;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int sampleRead = source.Read(buffer, offset, count);

            float[] cvBuffer = new float[buffer.Length];
            int cvSampleRead = cv.Read(cvBuffer, offset, count);

            for (int n = 0; n < sampleRead; n++)
            {
                buffer[offset + n] *= Math.Max(Clamp(cvBuffer[offset + n], 1f), 0);
            }
            return sampleRead;
        }

        private float Clamp(float input, float ceiling)
        {
            if (input > ceiling) return ceiling;
            return input;
        }
    }
}
