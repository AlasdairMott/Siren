using System;
using System.Collections.Generic;
using NAudio.Wave;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace GHSynth.Audio
{
	public class WavePropertiesComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the WavePropertiesComponent class.
		/// </summary>
		public WavePropertiesComponent()
		  : base("WavePropertiesComponent", "Nickname",
			  "Description",
			  "GHSynth", "Utilities")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddNumberParameter("Length", "L", "Length of the sample", GH_ParamAccess.item);
			pManager.AddGenericParameter("Time", "T", "Time of the sample", GH_ParamAccess.item);
			pManager.AddGenericParameter("SampleRate", "R", "Sample rate of the sample", GH_ParamAccess.item);
			pManager.AddNumberParameter("Max", "Max", "Max amplitude", GH_ParamAccess.item);
			pManager.AddNumberParameter("Min", "Min", "Mix amplitude", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			var wave = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat()) as WaveStream;
			if (!DA.GetData(0, ref wave)) return;

			DA.SetData(0, wave.Length);
			DA.SetData(1, wave.TotalTime.TotalSeconds);
			DA.SetData(2, wave.WaveFormat.SampleRate);

			wave.Position = 0;
			var readBuffer = new float[wave.Length];
			int samplesRead = wave.ToSampleProvider().Read(readBuffer, 0, (int) wave.Length);
			wave.Position = 0;

			if (samplesRead > 0)
			{
				var max = readBuffer.Max();
				var min = readBuffer.Min();

				DA.SetData(3, max);
				DA.SetData(4, min);
			}


		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.waveProperties;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("cddafd35-7247-46cb-85a6-97e2faf91d7d"); }
		}
	}
}