using NAudio.Wave;
using System;

namespace Siren.SampleProviders
{
    class FFT : ISampleProvider
	{
        private ISampleProvider source;

        public double[] DataFft;
        public WaveFormat WaveFormat => source.WaveFormat;

        public FFT(ISampleProvider source) 
        {
            this.source = source;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var dataPcm = new Int16[count];
            int sampleRead = source.Read(buffer, 0, count);
            for (int n = 0; n < sampleRead; n++)
            {
                dataPcm[n] = Convert.ToInt16(short.MaxValue * buffer[n]);
            }
            updateFFT(dataPcm);
            return sampleRead;
        }

        /* Code copied from fft function from https://github.com/swharden/Csharp-Data-Visualization/blob/a7facd303904e5c8feb4186c48edac124cc92429/examples/2019-06-08-audio-fft/Form1.cs */
        private void updateFFT(Int16[] dataPcm)
        {
            // the PCM size to be analyzed with FFT must be a power of 2
            int fftPoints = 2;
            while (fftPoints * 2 <= dataPcm.Length)
                fftPoints *= 2;

            // apply a Hamming window function as we load the FFT array then calculate the FFT
            NAudio.Dsp.Complex[] fftFull = new NAudio.Dsp.Complex[fftPoints];
            for (int i = 0; i < fftPoints; i++)
                fftFull[i].X = (float)(dataPcm[i] * NAudio.Dsp.FastFourierTransform.HammingWindow(i, fftPoints));
            NAudio.Dsp.FastFourierTransform.FFT(true, (int)Math.Log(fftPoints, 2.0), fftFull);

            // copy the complex values into the double array that will be plotted
            if (DataFft == null)
                DataFft = new double[fftPoints / 2];
            for (int i = 0; i < fftPoints / 2; i++)
            {
                double fftLeft = Math.Abs(fftFull[i].X + fftFull[i].Y);
                double fftRight = Math.Abs(fftFull[fftPoints - i - 1].X + fftFull[fftPoints - i - 1].Y);
                DataFft[i] = fftLeft + fftRight;

                DataFft[i] *= 1.0 / fftPoints;
            }
        }
    }
}
