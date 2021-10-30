using System;

namespace NAudio.Wave.SampleProviders
{
    /// <summary>
    /// No nonsense mono to stereo provider, no volume adjustment,
    /// just copies input to left and right. 
    /// </summary>
    /// <remarks>adapted from https://github.com/naudio/NAudio/blob/fb35ce8367f30b8bc5ea84e7d2529e172cf4c381/NAudio.Core/Wave/SampleProviders/MonoToStereoSampleProvider.cs</remarks>
    public class StereoProvider : ISampleProvider
    {
        private readonly ISampleProvider _left;
        private readonly ISampleProvider _right;
        private float[] _sourceBufferLeft;
        private float[] _sourceBufferRight;

        /// <summary>
        /// Initializes a new instance of MonoToStereoSampleProvider
        /// </summary>
        /// <param name="left">Left channel sample provider</param>
        /// <param name="right">Right channel provider</param>
        public StereoProvider(ISampleProvider left, ISampleProvider right)
        {
            if (left.WaveFormat.Channels != 1 || right.WaveFormat.Channels != 1)
            {
                throw new ArgumentException("Sources must be mono");
            }
            if (left.WaveFormat.SampleRate != right.WaveFormat.SampleRate)
            {
                throw new ArgumentException("Sources must have same sample rate");
            }

            _left = left;
            _right = right;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(left.WaveFormat.SampleRate, 2);
        }

        /// <summary>
        /// WaveFormat of this provider
        /// </summary>
        public WaveFormat WaveFormat { get; }

        /// <summary>
        /// Reads samples from this provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="count">Number of samples required</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            var sourceSamplesRequired = count / 2;
            var outIndex = offset;
            EnsureSourceBuffer(sourceSamplesRequired);

            var leftSamplesRead = _left.Read(_sourceBufferLeft, 0, sourceSamplesRequired);
            var rightSamplesRead = _right.Read(_sourceBufferRight, 0, sourceSamplesRequired);
            var samplesRead = Math.Max(leftSamplesRead, rightSamplesRead);

            for (var n = 0; n < samplesRead; n++)
            {
                buffer[outIndex++] = _sourceBufferRight[Math.Min(n, rightSamplesRead)];
                buffer[outIndex++] = _sourceBufferLeft[Math.Min(n, leftSamplesRead)];
            }
            return samplesRead * 2;
        }

        private void EnsureSourceBuffer(int count)
        {
            if (_sourceBufferLeft == null || _sourceBufferLeft.Length < count)
            {
                _sourceBufferLeft = new float[count];
            }
            if (_sourceBufferRight == null || _sourceBufferRight.Length < count)
            {
                _sourceBufferRight = new float[count];
            }
        }
    }
}
