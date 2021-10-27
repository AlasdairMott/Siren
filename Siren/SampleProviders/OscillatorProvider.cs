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

        public WaveFormat WaveFormat => _source.WaveFormat;

        /// <summary>
        /// Initializes a new instance for the oscillator. 
        /// </summary>
        /// <param name="source">control voltage (v/o) for frequency (0 to 1f, normalized to 0 to 10f) </param>
        /// <param name="octave">octave offset</param>
        /// <param name="semi">semitone offset</param>
        /// <remarks>Trivial oscillators. From Martin Finke's blog http://www.martin-finke.de/blog/articles/audio-plugins-008-synthesizing-waveforms/</remarks>
        public OscillatorProvider(ISampleProvider source, WaveType waveType, double octave, double semi)
        {
            _source = source;
            _octave = octave;
            _semi = semi;
            _waveType = waveType;
        }

        /// <summary>
        /// Reads from this provider.
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);

            switch (_waveType)
            {
                case WaveType.Sin:
                    for (int n = 0; n < samplesRead; n++)
                    {
                        var pitch = ComputePitch(buffer[offset + n]);
                        _phaseIncrement = pitch * 2 * PI / WaveFormat.SampleRate;

                        buffer[offset + n] = (float)Math.Sin(_phase);

                        _phase += _phaseIncrement;
                        while (_phase >= TwoPI) _phase -= TwoPI;
                    }
                    break;
                case WaveType.Sawtooth:
                    for (int n = 0; n < samplesRead; n++)
                    {
                        var pitch = ComputePitch(buffer[offset + n]);
                        _phaseIncrement = pitch * 2 * PI / WaveFormat.SampleRate;

                        buffer[offset + n] = 1.0f - 2.0f * _phase / TwoPI;

                        _phase += _phaseIncrement;
                        while (_phase >= TwoPI) _phase -= TwoPI;
                    }
                    break;
                case WaveType.Triangle:
                    for (int n = 0; n < samplesRead; n++)
                    {
                        var pitch = ComputePitch(buffer[offset + n]);
                        _phaseIncrement = pitch * 2 * PI / WaveFormat.SampleRate;

                        var value = -1.0f + (2.0f * _phase / TwoPI);
                        buffer[offset + n] = 2.0f * (float)(Math.Abs(value) - 0.5);
                        _phase += _phaseIncrement;
                        while (_phase >= TwoPI) _phase -= TwoPI;
                    }
                    break;
                case WaveType.Square:
                    for (int n = 0; n < samplesRead; n++)
                    {
                        var pitch = ComputePitch(buffer[offset + n]);
                        _phaseIncrement = pitch * 2 * PI / WaveFormat.SampleRate;

                        if (_phase <= PI) buffer[offset + n] = 1.0f;
                        else buffer[offset + n] = -1.0f;

                        _phase += _phaseIncrement;
                        while (_phase >= TwoPI) _phase -= TwoPI;
                    }
                    break;
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

    }
}
