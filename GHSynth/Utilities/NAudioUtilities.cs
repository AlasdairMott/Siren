using NAudio.Wave;

namespace GHSynth
{
	public class NAudioUtilities
	{
		public static RawSourceWaveStream WaveProviderToWaveStream(ISampleProvider provider, int length, WaveFormat waveFormat) 
		{
			byte[] buffer = new byte[length];
			int samplesRead = provider.ToWaveProvider16().Read(buffer, 0, length);
			if (samplesRead == 0) throw new System.Exception("No Samples were read");

			var stream = new RawSourceWaveStream(buffer, 0, length, waveFormat);
			return stream;
		}
	}
}
