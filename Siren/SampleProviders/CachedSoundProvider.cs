using System;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound _cachedSound;
        public long Position { get; private set; }
        public long Length => _cachedSound.Length;
        public TimeSpan CurrentTime => TimeSpan.FromSeconds((double)Position / _cachedSound.WaveFormat.SampleRate);
        public float Level { get; private set; }

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            _cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = _cachedSound.AudioData.Length - Position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(_cachedSound.AudioData, Position, buffer, offset, samplesToCopy);
            Position += samplesToCopy;
            if (samplesToCopy > 0) Level = buffer[0];
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return _cachedSound.WaveFormat; } }
    }
}
