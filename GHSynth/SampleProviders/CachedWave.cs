using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHSynth.SampleProviders
{
	class CachedSound
	{
		public float[] AudioData { get; private set; }
		public WaveFormat WaveFormat { get; private set; }
		public int Length => AudioData.Length;

		public CachedSound(WaveStream wave)
		{
			WaveFormat = wave.WaveFormat;
			var wholeFile = new List<float>((int)(wave.Length / 4));
			var readBuffer = new float[wave.WaveFormat.SampleRate * wave.WaveFormat.Channels];
			int samplesRead;
			var sampleProvider = wave.ToSampleProvider();
			while ((samplesRead = sampleProvider.Read(readBuffer, 0, readBuffer.Length)) > 0)
			{
				wholeFile.AddRange(readBuffer.Take(samplesRead));
			}
			AudioData = wholeFile.ToArray();
		}
	}

	class CachedSoundSampleProvider : ISampleProvider
	{
		private readonly CachedSound cachedSound;
		private long position;

		public CachedSoundSampleProvider(CachedSound cachedSound)
		{
			this.cachedSound = cachedSound;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			var availableSamples = cachedSound.AudioData.Length - position;
			var samplesToCopy = Math.Min(availableSamples, count);
			Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);
			position += samplesToCopy;
			return (int)samplesToCopy;
		}

		public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
	}
}
