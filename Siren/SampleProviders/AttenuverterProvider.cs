using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class AttenuverterProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly float _amount;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public AttenuverterProvider(ISampleProvider source, float amount)
        {
            _source = source;
            _amount = amount;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int sampleRead = _source.Read(buffer, offset, count);
            for (int n = 0; n < sampleRead; n++)
            {
                buffer[offset + n] *= _amount;
            }
            return sampleRead;
        }
    }
}
