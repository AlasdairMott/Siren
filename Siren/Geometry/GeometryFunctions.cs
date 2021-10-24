using System;
using System.Linq;
using NAudio.Wave;
using Rhino.Geometry;

namespace Siren.Geometry
{
    public class GeometryFunctions
    {
        public static Polyline ISampleToPolyline(ISampleProvider sample, double X, double Y, int resolution)
        {
            if (resolution < 1) throw new ArgumentOutOfRangeException("Must be greater than 0");

            int lenBuffer;
            if (resolution > (sample.WaveFormat.SampleRate * sample.WaveFormat.Channels))
                lenBuffer = 1;
            else lenBuffer = (sample.WaveFormat.SampleRate * sample.WaveFormat.Channels) / resolution;
            var polyline = new Polyline();

            var readBuffer = new float[lenBuffer];
            int samplesRead;

            var xPos = 0.0;
            var count = 0;
            do
            {
                samplesRead = sample.Read(readBuffer, 0, lenBuffer);
                if (samplesRead > 0)
                {
                    var value = readBuffer.Take(samplesRead).First();
                    polyline.Add(new Point3d(xPos, value * Y, 0));
                    xPos += (X / sample.WaveFormat.SampleRate) * lenBuffer;
                    count += 1;
                }

            } while (samplesRead > 0);

            return polyline;
        }

    }
}
