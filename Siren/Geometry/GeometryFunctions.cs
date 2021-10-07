using NAudio.Wave;
using Rhino.Geometry;
using System;
using System.Linq;

namespace Siren.Geometry
{
	public class GeometryFunctions
	{
		public enum WindowMethod
		{
			First,
			Max
		}

		public static Polyline ISampleToPolyline(ISampleProvider sample, double X, double Y, int resolution, WindowMethod method = WindowMethod.First)
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
					var value = method == WindowMethod.First ? readBuffer.Take(samplesRead).First() : readBuffer.Take(samplesRead).Max();
					polyline.Add(new Point3d(xPos, value * Y, 0));
					xPos += (X / (double)sample.WaveFormat.SampleRate) * lenBuffer; //*500
					count += 1;
				}

			} while (samplesRead > 0); 

			return polyline;
		}
	}
}
