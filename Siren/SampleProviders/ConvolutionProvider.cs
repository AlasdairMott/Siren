using System;
using System.Linq;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class ConvolutionProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly float[] _kernel;
        private readonly float[] _window;
        private readonly int _kernelSize;
        private readonly float _divisor;
        private readonly int _k;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public ConvolutionProvider(ISampleProvider source, CachedSound kernel, int count, int skip)
        {
            if (count > kernel.Length) throw new Exception("count larger than kernel");

            _source = source;
            _kernel = kernel.AudioData.Take(count).ToArray();
            _kernelSize = count;
            _divisor = _kernel.Sum();
            _window = new float[_kernelSize];
            _k = skip;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int sampleRead = _source.Read(buffer, offset, count);

            for (int n = 0; n < sampleRead; n++)
            {
                //Slide the window
                Array.Copy(_window, 0, _window, 1, _kernelSize - 1);
                _window[0] = buffer[offset + n];

                var sample = 0.0f;
                for (int i = 0; i < _kernelSize / _k; i++)
                {
                    sample += _kernel[i * _k] * _window[i * _k];
                }
                buffer[offset + n] = sample / _divisor * _k;
            }
            return sampleRead;
        }
    }
}
