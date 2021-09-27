using NAudio.Wave;

namespace GHSynth
{
	public class NAudioUtilities
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
	}
}
