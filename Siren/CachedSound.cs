using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
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
            this.AudioData = new float[other.AudioData.Length];
            other.AudioData.CopyTo(this.AudioData, 0);
            this.WaveFormat = other.WaveFormat;
            this.Length = other.Length;
        }

        public CachedSoundSampleProvider ToSampleProvider() => new CachedSoundSampleProvider(this);

        public RawSourceWaveStream ToRawSourceWaveStream() 
        {
            var stream = new List<byte>();
            for (int n =0; n < AudioData.Length; n++)
            {
                short valShort = Convert.ToInt16(short.MaxValue * SirenUtilities.Limit(AudioData[n]));
                var valByte = BitConverter.GetBytes(valShort);
                stream.AddRange(valByte);
            }

            return new RawSourceWaveStream(stream.ToArray(), 0, stream.Count, WaveFormat);
        }

        public CachedSound Clone() { return new CachedSound(this); }

        object ICloneable.Clone() => Clone();

    }

}
