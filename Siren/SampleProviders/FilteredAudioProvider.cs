using NAudio.Dsp;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class FilteredAudioProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly ISampleProvider _cvProvider;
        private readonly BiQuadFilter _filter;
        private readonly float _cutoff;
        private readonly float _cvAmount;
        private readonly float _q;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public FilteredAudioProvider(ISampleProvider source, ISampleProvider cvProvider, BiQuadFilter filter, float cutoff, float cvAmount, float q)
        {
            _source = source;
            _cvProvider = cvProvider;
            _filter = filter;
            _cutoff = cutoff;
            _cvAmount = cvAmount;
            _q = q;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            float[] cvBuffer = new float[buffer.Length];
            int cvSampleRead = _cvProvider.Read(cvBuffer, offset, count);

            int sampleRead = _source.Read(buffer, offset, count);
            for (int n = 0; n < sampleRead; n++)
            {
                var cv = cvBuffer[offset + n];
                var filterCutoff = SirenUtilities.Clamp(_cutoff * 0.1f + cv * _cvAmount, -1, 1);

                filterCutoff = SirenUtilities.Remap(filterCutoff, -1, 1, 88200, 100000);
                //cvCutoff += cutoff;

                _filter.SetLowPassFilter(SirenSettings.SampleRate, filterCutoff, _q);
                buffer[offset + n] = _filter.Transform(buffer[offset + n]);
            }
            return sampleRead;
        }
    }
}
