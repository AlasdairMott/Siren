using NAudio.Wave;
using Rhino.Geometry;
using System;
using System.Linq;

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
					xPos += (X / (double) sample.WaveFormat.SampleRate) * lenBuffer; //*500
					count += 1;
				}

			} while (samplesRead > 0);
			//var scale1d = Transform.Scale(Plane.WorldXY, ((double) resolution) / ((double) count * lenBuffer), 1, 1);
			//polyline.Transform(scale1d);
			return polyline;
		}

		public static Polyline ISampleToSurface(ISampleProvider sample, int resolution)
		{
			if (resolution < 1) throw new ArgumentOutOfRangeException("Must be greater than 0");

			var polyline = new Polyline();
			var samplesPerSecond = (sample.WaveFormat.SampleRate * sample.WaveFormat.Channels) / resolution;
			var readBuffer = new float[samplesPerSecond];
			int samplesRead;

			var xPos = 0.1;
			var yScale = 1;
			do
			{
				samplesRead = sample.Read(readBuffer, 0, samplesPerSecond);
				if (samplesRead > 0)
				{
					var max = readBuffer.Take(samplesRead).Max();
					polyline.Add(new Point3d(xPos, yScale + max * yScale, 0));
					xPos += 0.1;
				}

			} while (samplesRead > 0);
			return polyline;
		}
	}
}
