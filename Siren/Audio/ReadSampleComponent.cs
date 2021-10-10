using Grasshopper.Kernel;
using NAudio.Wave;
using System;
using Rhino.Geometry;
using System.Linq;

namespace Siren
{
	public class ReadSampleComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the ReadSampleComponent class.
		/// </summary>
		public ReadSampleComponent()
		  : base("Read Sample", "SampleR",
			  "Description",
			  "Siren", "Oscillators")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddTextParameter("Path", "P", "Path of audio file", GH_ParamAccess.item);
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
			string path = "";
			if (!DA.GetData(0, ref path)) return;

			var audioFile = new AudioFileReader(path);

			//var cachedWave = new SampleProviders.CachedSound(audioFile);

			//audioFile.Position = 0;
			//var raw = NAudioUtilities.WaveProviderToWaveStream(
			//	new SampleProviders.CachedSoundSampleProvider(cachedWave),
			//	(int)audioFile.Length,
			//	cachedWave.WaveFormat);
			//audioFile.Position = 0;

			//audioFile.Position = 0;
			//var raw = NAudioUtilities.WaveProviderToWaveStream(
			//	audioFile.ToSampleProvider(),
			//	(int)audioFile.Length,
			//	audioFile.WaveFormat);
			//audioFile.Position = 0;

			DA.SetData(0, audioFile);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.readSample;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("805913e8-8e7b-4264-9368-87228bc3c850"); }
		}

		
	}
}