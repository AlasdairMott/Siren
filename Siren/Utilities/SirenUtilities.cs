using System;
using NAudio.Wave;

namespace Siren
{
    public class SirenUtilities
    {
        public static RawSourceWaveStream WaveProviderToWaveStream(ISampleProvider provider, int length, WaveFormat waveFormat)
        {
            byte[] buffer = new byte[length];
            var provider16 = provider.ToWaveProvider16();
            int samplesRead = provider16.Read(buffer, 0, length);
            if (samplesRead == 0) throw new System.Exception("No Samples were read");

            var stream = new RawSourceWaveStream(buffer, 0, length, waveFormat);
            return stream;
        }

        public static float Remap(float value, float sourceA, float sourceB, float targetA, float targetB)
        {
            return targetA + (value - sourceA) * (targetB - targetA) / (sourceB - sourceA);
        }

        public static float Lerp(float start, float finish, float p)
        {
            return finish * p + start * (1 - p);
        }

        public static float Clamp(float value, float floor, float ceiling)
        {
            return Math.Max(Math.Min(value, ceiling), floor);
        }

        public static float Limit(float value)
        {
            return Math.Max(Math.Min(value, 1.0f), -1.0f);
        }
    }
}
