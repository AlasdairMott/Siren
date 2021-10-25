using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class AttenuverterProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly float _amount;
        private readonly float _offset;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public AttenuverterProvider(ISampleProvider source, float amount, float offset)
        {
            _source = source;
            _amount = amount;
            _offset = offset;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int sampleRead = _source.Read(buffer, offset, count);
            for (int n = 0; n < sampleRead; n++)
            {
                buffer[offset + n] *= _amount;
                buffer[offset + n] += _offset;
            }
            return sampleRead;
        }
    }
}
