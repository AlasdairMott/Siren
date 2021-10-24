using NAudio.Wave.SampleProviders;

namespace Siren.SampleProviders
{
    internal class NoiseGenerator
    {
        public static OffsetSampleProvider Oscillator(double frequency, double duration, SignalGeneratorType type)
        {
            var sampleRate = SirenSettings.SampleRate;
            var signalGenerator = new SignalGenerator(sampleRate, 1)
            {
                Type = type,
                Frequency = frequency,
                Gain = 0.25
            };
            var offsetProvider = new OffsetSampleProvider(signalGenerator)
            {
                TakeSamples = (int)(sampleRate * duration)
            };
            return offsetProvider;
        }
    }
}
