using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace Siren.SampleProviders
{
	public class ADEnvelopeProvider : ISampleProvider
	{
		private ISampleProvider pulses;

		private float power;
		private float rise;
		private float fall;
		private float exponent;

		public WaveFormat WaveFormat => pulses.WaveFormat;

		public ADEnvelopeProvider(ISampleProvider pulses, TimeSpan attack, TimeSpan decay, float exponent)
		{
			this.pulses = pulses;
			power = 0;
			this.rise = (float) (1.0 / (attack.TotalSeconds * pulses.WaveFormat.SampleRate));
			this.fall = (float) (1.0 / (decay.TotalSeconds * pulses.WaveFormat.SampleRate));
			this.exponent = Math.Max(0f, exponent);
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
				power -= fall;
				power = Math.Max(0.0f, power);
				buffer[offset + n] = (float) Math.Pow(power, exponent);
			}

			return samplesRead;
		}


	}
}
