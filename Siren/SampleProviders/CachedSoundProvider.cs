using NAudio.Wave;
using System;

namespace Siren.SampleProviders
{
    public class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        public long Position { get; private set; }
        public long Length => cachedSound.Length;
        public TimeSpan CurrentTime => TimeSpan.FromSeconds(((double) Position) / cachedSound.WaveFormat.SampleRate * Length);

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = cachedSound.AudioData.Length - Position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(cachedSound.AudioData, Position, buffer, offset, samplesToCopy);
            Position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
    }
}
