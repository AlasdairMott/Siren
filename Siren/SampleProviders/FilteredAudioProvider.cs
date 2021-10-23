using NAudio.Dsp;
using NAudio.Wave;

namespace Siren.SampleProviders
{
    public class FilteredAudioProvider : ISampleProvider
    {
        private ISampleProvider source;
        private ISampleProvider cvProvider;
        private BiQuadFilter filter;
        private float cutoff;
        private float cvAmount;
        private float q;

        public WaveFormat WaveFormat => source.WaveFormat;

        public FilteredAudioProvider(ISampleProvider source, ISampleProvider cvProvider, BiQuadFilter filter, float cutoff, float cvAmount, float q)
        {
            this.source = source;
            this.cvProvider = cvProvider;
            this.filter = filter;
            this.cutoff = cutoff;
            this.cvAmount = cvAmount;
            this.q = q;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            float[] cvBuffer = new float[buffer.Length];
            int cvSampleRead = cvProvider.Read(cvBuffer, offset, count);

            int sampleRead = source.Read(buffer, offset, count);
            for (int n = 0; n < sampleRead; n++)
            {
                var cv = cvBuffer[offset + n];
                var filterCutoff = SirenUtilities.Clamp(cutoff * 0.1f + cv * cvAmount, -1, 1);

                filterCutoff = SirenUtilities.Remap(filterCutoff, -1, 1, 88200, 100000);
                //cvCutoff += cutoff;

                filter.SetLowPassFilter(SirenSettings.SampleRate, filterCutoff, q);
                buffer[offset + n] = filter.Transform(buffer[offset + n]);
            }
            return sampleRead;
        }
    }
}
