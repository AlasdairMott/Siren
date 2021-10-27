using System;
using NAudio.Wave;

namespace Siren.SampleProviders
{    /// <summary>
     /// Wave type
     /// </summary>
    public enum WaveType
    {
        Sin,
        Sawtooth,
        Triangle,
        Square
    }

    public class OscillatorProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly double _octave;
        private readonly double _semi;
        private readonly WaveType _waveType;

        private const float PI = (float)Math.PI;
        private const float TwoPI = 2f * PI;
        private float _phase;
        private float _phaseIncrement;
        private readonly float _gain;
        private float _prev;

        public WaveFormat WaveFormat => _source.WaveFormat;

        /// <summary>
        /// Initializes a new instance for the oscillator. 
        /// </summary>
        /// <param name="source">control voltage (v/o) for frequency (0 to 1f, normalized to 0 to 10f) </param>
        /// <param name="octave">octave offset</param>
        /// <param name="semi">semitone offset</param>
        /// <remarks>Oscillawtors from Martin Finke's blog http://www.martin-finke.de/blog/articles/audio-plugins-008-synthesizing-waveforms/ with simple anti-aliasing using PolyBLEP</remarks>
        public OscillatorProvider(ISampleProvider source, WaveType waveType, double octave, double semi)
        {
            _source = source;
            _octave = octave;
            _semi = semi;
            _waveType = waveType;
            _gain = 0.5f;
        }

        /// <summary>
        /// Reads from this provider.
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);

            for (int n = 0; n < samplesRead; n++)
            {
                var pitch = ComputePitch(buffer[offset + n]);
                _phaseIncrement = pitch * 2 * PI / WaveFormat.SampleRate;

                buffer[offset + n] = NextSample() * _gain;
            }

            return samplesRead;
        }

        /// <summary>
        /// Remaps (0-1) to (0,10) v/0, then converts to frequency.
        /// </summary>
        private float ComputePitch(float input)
        {
            var controlVoltage = input * 10 - 1 + _semi * (1.0 / 12.0);
            return (float)Math.Pow(2, controlVoltage + _octave - 1) * 55;
        }

        /// <summary>
        /// Naive waveform functions. Lots of aliasing at high frequencies.
        /// </summary>
        /// <param name="mode">Waveform type to render</param>
        /// <remarks>Triangle is not being used here, but should be for LFOs</remarks>
        private float NaiveWaveform(WaveType mode)
        {
            float value;
            switch (mode)
            {
                case WaveType.Sin:
                    value = (float)Math.Sin(_phase);
                    break;
                case WaveType.Sawtooth:
                    value = (2.0f * _phase / TwoPI) - 1.0f;
                    break;
                case WaveType.Triangle:
                    value = -1.0f + (2.0f * _phase / TwoPI);
                    value = 2.0f * (float)(Math.Abs(value) - 0.5);
                    break;
                case WaveType.Square:
                    value = _phase <= PI ? 1.0f : -1.0f;
                    break;
                default:
                    value = 0.0f;
                    break;
            }
            return value;
        }

        /// <summary>
        /// Render next sample for waveform
        /// </summary>
        private float NextSample()
        {
            float value = 0.0f;
            float t = _phase / TwoPI;

            if (_waveType == WaveType.Sin)
            {
                value = NaiveWaveform(WaveType.Sin);
            }
            else if (_waveType == WaveType.Sawtooth)
            {
                value = NaiveWaveform(WaveType.Sawtooth);
                value -= PolyBLEP(t);
            }
            else
            {
                value = NaiveWaveform(WaveType.Square);
                value += PolyBLEP(t);
                value -= PolyBLEP((t + 0.5f) % 1.0f);
            }
            if (_waveType == WaveType.Triangle)
            {
                // Leaky integrator: y[n] = A * x[n] + (1 - A) * y[n-1]
                value = _phaseIncrement * value + (1 - _phaseIncrement) * _prev;
                _prev = value;
            }

            _phase += _phaseIncrement;
            while (_phase >= TwoPI) _phase -= TwoPI;
            return value;
        }

        /// <summary>
        /// PolyBLEP function to reduce aliasing.
        /// </summary>
        /// <remarks>
        // PolyBLEP by Tale, http://www.kvraudio.com/forum/viewtopic.php?t=375517, "rounds" the waveform edges.
        // Converted to C# from Martin Finnke's blog, http://www.martin-finke.de/blog/articles/audio-plugins-018-polyblep-oscillator/
        /// </remarks>
        private float PolyBLEP(float t)
        {
            float dt = _phaseIncrement / TwoPI;
            // 0 <= t < 1
            if (t < dt)
            {
                t /= dt;
                return t + t - t * t - 1.0f;
            }
            // -1 < t < 0
            else if (t > 1.0f - dt)
            {
                t = (t - 1.0f) / dt;
                return t * t + t + t + 1.0f;
            }
            // 0 otherwise
            else return 0.0f;
        }
    }
}
