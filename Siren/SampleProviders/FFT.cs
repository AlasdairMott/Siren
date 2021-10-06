using NAudio.Wave;
using System;

namespace Siren.SampleProviders
{
    class FFT
	{
		public double[] dataFft;
        Int16[] dataPcm;
        WaveStream wave;
        ISampleProvider source;

        public FFT(WaveStream wave) 
        {
            source = wave.ToSampleProvider();
        }

        public int Read() 
        {
            dataPcm = new Int16[wave.Length];

            var buffer = new float[wave.Length];
            int sampleRead = source.Read(buffer, 0, (int)wave.Length);

            for (int n = 0; n < sampleRead; n++)
            {
                dataPcm[n] = Convert.ToInt16(short.MaxValue * buffer[n]);
            }
            updateFFT();

            return sampleRead;
        }

        /* Code copied from fft function from https://github.com/swharden/Csharp-Data-Visualization/blob/a7facd303904e5c8feb4186c48edac124cc92429/examples/2019-06-08-audio-fft/Form1.cs */
        private void updateFFT()
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
            if (dataFft == null)
                dataFft = new double[fftPoints / 2];
            for (int i = 0; i < fftPoints / 2; i++)
            {
                double fftLeft = Math.Abs(fftFull[i].X + fftFull[i].Y);
                double fftRight = Math.Abs(fftFull[fftPoints - i - 1].X + fftFull[fftPoints - i - 1].Y);
                dataFft[i] = fftLeft + fftRight;
            }
        }
    }
}
