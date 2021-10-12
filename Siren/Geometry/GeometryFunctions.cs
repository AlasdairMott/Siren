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

		public enum ScalingMethod 
		{
			Unit,
			Logarithmic
		}

		public static Polyline ISampleToPolyline(ISampleProvider sample, double X, double Y, int resolution, 
			WindowMethod windowMethod = WindowMethod.First, ScalingMethod scalingMethod = ScalingMethod.Unit)
		{
			if (resolution < 1) throw new ArgumentOutOfRangeException("Must be greater than 0");

			int lenBuffer;
			if (resolution > (sample.WaveFormat.SampleRate * sample.WaveFormat.Channels))
				lenBuffer = 1;
			else lenBuffer = (sample.WaveFormat.SampleRate * sample.WaveFormat.Channels) / resolution;
			var polyline = new Polyline();

			var readBuffer = new float[lenBuffer];
			int samplesRead;

			var xPos = 0.01;
			var count = 0;

			do
			{
				samplesRead = sample.Read(readBuffer, 0, lenBuffer);
				if (samplesRead > 0)
				{
					var value = windowMethod == WindowMethod.First ? readBuffer.Take(samplesRead).First() : readBuffer.Take(samplesRead).Max();
					double x = (scalingMethod == ScalingMethod.Unit) ? xPos : Math.Log(xPos, 2);
					polyline.Add(new Point3d(x, value * Y, 0));
					xPos += (X / (double)sample.WaveFormat.SampleRate) * lenBuffer; 
					count += 1;
				}

			} while (samplesRead > 0);

			polyline.Transform(Transform.Translation(-new Vector3d(polyline.First().X, 0, 0)));

			return polyline;
		}
	}
}
