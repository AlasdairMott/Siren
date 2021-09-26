using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GHSynth
{
	public class NAudioUtilities
	{
		public static RawSourceWaveStream WaveProviderToWaveStream(ISampleProvider provider, WaveStream wave) 
		{
			int count = (int)wave.Length;
			byte[] buffer = new byte[wave.Length];
			provider.ToWaveProvider16().Read(buffer, 0, count);

			var stream = new RawSourceWaveStream(buffer, 0, count, wave.WaveFormat);
			return stream;
		}
	}
}
