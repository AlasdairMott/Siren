using System;
using System.Linq;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class ConvolutionProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly float[] kernel;
        private readonly float[] window;
        private readonly int kernelSize;
        private readonly float divisor;
        private readonly int k;

        public WaveFormat WaveFormat => source.WaveFormat;

        public ConvolutionProvider(ISampleProvider source, CachedSound kernel, int count, int skip)
        {
            if (count > kernel.Length) throw new Exception("count larger than kernel");

            this.source = source;
            this.kernel = kernel.AudioData.Take(count).ToArray();
            kernelSize = count;
            divisor = this.kernel.Sum();
            window = new float[kernelSize];
            k = skip;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int sampleRead = source.Read(buffer, offset, count);

            for (int n = 0; n < sampleRead; n++)
            {
                //Slide the window
                Array.Copy(window, 0, window, 1, kernelSize - 1);
                window[0] = buffer[offset + n];

                var sample = 0.0f;
                for (int i = 0; i < kernelSize / k; i++)
                {
                    sample += kernel[i * k] * window[i * k];
                }
                buffer[offset + n] = sample / divisor * k;
            }
            return sampleRead;
        }
    }
}
