using NAudio.Wave.SampleProviders;

namespace GHSynth.SampleProviders
{
	class SimpleSignalGenerator
	{
		public static OffsetSampleProvider Oscillator(double frequency, double duration, SignalGeneratorType type)
		{
			var sampleRate = 44100;
			var signalGenerator = new SignalGenerator(sampleRate, 1);
			signalGenerator.Type = type;
			signalGenerator.Frequency = frequency;
			signalGenerator.Gain = 0.25;
			var offsetProvider = new OffsetSampleProvider(signalGenerator);
			offsetProvider.TakeSamples = (int)(sampleRate * duration);
			return offsetProvider;
		}
	}
}
