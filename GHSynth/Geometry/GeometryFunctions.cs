﻿using NAudio.Wave;
using Rhino.Geometry;
using System;
using System.Linq;

namespace GHSynth.Geometry
{
	public class GeometryFunctions
	{
		public static Polyline ISampleToPolyline(ISampleProvider sample, int resolution)
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