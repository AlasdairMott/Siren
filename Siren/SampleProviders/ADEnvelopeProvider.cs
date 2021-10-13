using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace Siren.SampleProviders
{
	public class ADEnvelopeProvider : ISampleProvider
	{
		private ISampleProvider pulses;

		private int attackLength;
		private int decayLength;
		private float power;
		private float decay;

		public WaveFormat WaveFormat => pulses.WaveFormat;

		public ADEnvelopeProvider(ISampleProvider pulses, float decay)
		{
			this.pulses = pulses;
			//attackLength = (int) attack.TotalSeconds * pulses.WaveFormat.SampleRate;
			//decayLength = (int) decay.TotalSeconds * pulses.WaveFormat.SampleRate;
			power = 0;
			this.decay = NAudioUtilities.Clamp(decay, 0.0f, 0.9999f);
		}

		public int Read(float[] buffer, int offset, int count)
		{
			var pulseBuffer = new float[count];
			int samplesRead = pulses.Read(pulseBuffer, offset, count); //read the pulse signal
			if (samplesRead == 0)
			{
				if (power == 0f) return 0;
				else if (samplesRead == 0) samplesRead = WaveFormat.SampleRate;
			}

			bool triggered = false;
			for (int n = 0; n < samplesRead; n++)
			{
				float p = 0f;
				if (n < pulseBuffer.Length)
					p = pulseBuffer[offset + n];

				if (p < 0.1) triggered = false;
				else if (p > 0.1 & !triggered)
				{
					triggered = true;
					power = 1.0f;
				}
				power *= decay;
				if (power < 0.01f) power = 0.0f;
				buffer[offset + n] = power;
			}

			return samplesRead;
		}


	}
}
