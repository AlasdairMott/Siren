using System;
using NAudio.Wave.SampleProviders;

namespace Siren.SampleProviders
{
    internal class SignalGenerator
    {
        public static OffsetSampleProvider CreateNoise(TimeSpan duration, SignalGeneratorType type)
        {
            return CreateNoise(duration, 0.25, type);
        }

        public static OffsetSampleProvider CreateNoise(TimeSpan duration, double gain, SignalGeneratorType type)
        {
            var sampleRate = SirenSettings.SampleRate;
            var signalGenerator = new NAudio.Wave.SampleProviders.SignalGenerator(sampleRate, 1)
            {
                Type = type,
                Gain = gain
            };
            var offsetProvider = new OffsetSampleProvider(signalGenerator)
            {
                Take = duration
            };
            return offsetProvider;
        }

        public static OffsetSampleProvider CreateSilence(TimeSpan duration)
        {
            return CreateNoise(duration, 0, SignalGeneratorType.White);
        }
    }
}
