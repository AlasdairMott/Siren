﻿using Grasshopper.Kernel;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;

namespace GHSynth
{
	public class RepitchComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the RepitchComponent class.
		/// </summary>
		public RepitchComponent()
		  : base("RepitchComponent", "Nickname",
			  "Description",
			  "GHSynth", "Subcategory")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
			pManager.AddNumberParameter("Pitch", "P", "PitchChange", GH_ParamAccess.item);
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave output", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			int sampleRate = 44100;

			var wave = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat());
			if (!DA.GetData(0, ref wave)) return;

			wave.Position = 0;

			double p = 1;
			if (!DA.GetData(1, ref p)) return;

			var semitone = Math.Pow(2, 1.0 / 12);
			var upOneTone = semitone * semitone;
			var downOneTone = 1.0 / upOneTone;
			
			var pitch = new SmbPitchShiftingSampleProvider(wave.ToSampleProvider());
			pitch.PitchFactor = (float)(upOneTone * p); // or downOneTone

			int count = (int)wave.Length;
			byte[] buffer = new byte[wave.Length];
			pitch.ToWaveProvider16().Read(buffer, 0, count);

			var stream = new RawSourceWaveStream(buffer, 0, count, wave.WaveFormat);

			//var waveProvider = pitch.ToWaveProvider();

			//RawSourceWaveStream stream = new RawSourceWaveStream(wave as Stream, wave.WaveFormat);
			//using (var tempStream = new NAudio.Utils.IgnoreDisposeStream(new MemoryStream()))
			//{
			//	WaveFileWriter.WriteWavFileToStream(tempStream, waveProvider);


			//	//clone the source stream here?
			//	//memoryStream.Seek(0, SeekOrigin.Begin);
			//	stream = new RawSourceWaveStream(tempStream.SourceStream, waveProvider.WaveFormat);
			//}


			//using (var waveFileWriter = new WaveFileWriter(stream, wave.WaveFormat))
			//{
			//	byte[] bytes = new byte[wave.Length];
			//	wave.Position = 0;
			//	pitch.ToWaveProvider().Read(bytes, 0, (int) wave.Length);
			//	waveFileWriter.Write(bytes, 0, bytes.Length);
			//	waveFileWriter.Flush();
			//}

			//WaveFileWriter.CreateWaveFile16("buffer.wav", pitch);

			//var buffer = new AudioFileReader("buffer.wav");

			//var offsetProvider = new OffsetSampleProvider(pitch);
			//offsetProvider.TakeSamples = wave.WaveFormat.SampleRate * wave.WaveFormat.Channels * wave.TotalTime.Seconds;

			//var signal = new SignalGenerator() { Type = SignalGeneratorType.SawTooth}

			//new WaveFileReader()

			//var s = new RawSourceWaveStream(offsetProvider, wave.WaveFormat);

			DA.SetData(0, stream);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.repitch;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("2afb9e8d-b7ca-4111-b3e1-e04b1249a134"); }
		}
	}
}