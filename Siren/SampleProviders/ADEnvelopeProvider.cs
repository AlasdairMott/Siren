using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace Siren.SampleProviders
{
	public class ADEnvelopeProvider : ISampleProvider
	{
		private ISampleProvider pulses;

		private float power;
		private float envelope;
		private float rise;
		private float fall;
		private float exponent;
		private bool rising;

		public WaveFormat WaveFormat => pulses.WaveFormat;

		public ADEnvelopeProvider(ISampleProvider pulses, float attack, float decay, float exponent)
		{
			this.pulses = pulses;
			power = 0;
			envelope = 0;
			this.rise = (float) Math.Pow(SirenUtilities.Clamp(attack, 0.0f, 1.0f), 0.005);
			this.fall = (float) Math.Pow(SirenUtilities.Clamp(decay, 0.0f, 1.0f), 0.005);
			this.rise = SirenUtilities.Remap(this.rise, 0.0f, 1.0f, 0.03f, 0.0001f);
			this.fall = SirenUtilities.Remap(this.fall, 0.0f, 1.0f, 0.03f, 0.0001f);

			//this.rise = (float) (1.0f / Math.Log(Math.Max(attack, 1.001), 2)) * 0.01f;
			//this.fall = (float) (1.0f / Math.Log(Math.Max(decay, 1.001), 2)) * 0.01f;
			//this.rise = 1.0f / SirenUtilities.Clamp(attack, 0.1f, 0.9f) * 0.0001f; Math.Log(attack, 2);
			//this.fall = 1.0f / SirenUtilities.Clamp(decay, 0.1f, 0.9f) * 0.0001f;
			this.exponent = Math.Max(0f, exponent);
			rising = false;
		}

		private float Process(float a, float b, float by)
		{
			return b * by + a * (1 - by);
		}

		public int Read(float[] buffer, int offset, int count)
		{
			var pulseBuffer = new float[count];
			int samplesRead = pulses.Read(pulseBuffer, offset, count); //read the pulse signal
			if (samplesRead == 0)
			{
				if (envelope == 0f) return 0;
				else samplesRead = WaveFormat.SampleRate;
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
					rising = true;
				}
				if (rising)
				{
					envelope = Process(envelope, 1.0f, rise);

					if (envelope >= 1.0 - 0.001f) rising = false;
				}
				else
				{
					envelope = Process(envelope, 0.0f, fall);
					if (envelope <= 0.001f) envelope = 0.0f;
				}

				buffer[offset + n] = envelope;
			}

			return samplesRead;
		}


	}
}
