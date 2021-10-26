using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using Siren.SampleProviders;

namespace Siren
{
    // adapted from Mark Heath's cached sound class https://markheath.net/post/fire-and-forget-audio-playback-with
    public class CachedSound : ICloneable
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public long Length { get; private set; }
        public static CachedSound Empty => new CachedSound(new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat()));
        public TimeSpan TotalTime => TimeSpan.FromSeconds(((double)Length) / WaveFormat.SampleRate);

        public CachedSound(WaveStream wave)
        {
            WaveFormat = wave.WaveFormat;
            Length = wave.Length;

            var wholeFile = new List<float>((int)(wave.Length / 4));
            var readBuffer = new float[wave.WaveFormat.SampleRate * wave.WaveFormat.Channels];

            int samplesRead;
            var provider = wave.ToSampleProvider();
            while ((samplesRead = provider.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                wholeFile.AddRange(readBuffer.Take(samplesRead));
            }
            AudioData = wholeFile.ToArray();
        }

        public CachedSound(ISampleProvider sampleProvider)
        {
            var buffer = new List<float>();

            int count = sampleProvider.WaveFormat.SampleRate;
            int samplesRead = 0;
            do
            {
                var readBuffer = new float[count];
                samplesRead = sampleProvider.Read(readBuffer, 0, count);
                buffer.AddRange(readBuffer.Take(samplesRead));

            } while (samplesRead > 0);

            WaveFormat = sampleProvider.WaveFormat;
            Length = buffer.Count;

            AudioData = buffer.ToArray();
        }

        public CachedSound(float[] data, WaveFormat waveFormat)
        {
            throw new NotImplementedException();
        }

        public CachedSound(CachedSound other)
        {
            AudioData = new float[other.AudioData.Length];
            other.AudioData.CopyTo(AudioData, 0);
            WaveFormat = other.WaveFormat;
            Length = other.Length;
        }

        public CachedSoundSampleProvider ToSampleProvider() => new CachedSoundSampleProvider(this);

        public CachedSound Clone() { return new CachedSound(this); }

        object ICloneable.Clone() => Clone();

        public void SaveToFile(string fileName)
        {
            using (var writer = new WaveFileWriter(fileName, WaveFormat))
            {
                writer.WriteSamples(AudioData, 0, (int)Length);
            }
        }

        public byte[] ToByteArray()
        {
            System.IO.MemoryStream stream;
            using (stream = new System.IO.MemoryStream())
            using (var writer = new WaveFileWriter(stream, WaveFormat))
            {
                writer.WriteSamples(AudioData, 0, (int)Length);
            }
            return stream.ToArray();
        }

        public static CachedSound FromByteArray(byte[] buffer)
        {
            CachedSound sound;
            using (var stream = new System.IO.MemoryStream(buffer))
            using (var reader = new WaveFileReader(stream))
            {
                sound = new CachedSound(reader.ToSampleProvider());
            }
            return sound;
        }
    }
}
