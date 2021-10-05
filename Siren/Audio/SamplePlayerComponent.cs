using Siren.SampleProviders;
using Grasshopper.Kernel;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Siren.Audio
{
	public class SamplePlayerComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the SamplePlayerComponent class.
		/// </summary>
		public SamplePlayerComponent()
		  : base("SamplePlayerComponent", "Nickname",
			  "Description",
			  "Siren", "Oscillators")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Sample", "W", "Sample input", GH_ParamAccess.item);
			pManager.AddPointParameter("Points", "P", "Points", GH_ParamAccess.list);
			pManager.AddNumberParameter("Time Factor", "T", "T", GH_ParamAccess.item);
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
			var wave = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat()) as WaveStream;
			if (!DA.GetData(0, ref wave)) return;

			var points = new List<Point3d>();
			if (!DA.GetDataList(1, points)) return;

			double X = 1;
			DA.GetData(2, ref X); if (X <= 0) throw new Exception("T must be positive");

			var triggerTimes = points.Select(p => p.X / X).ToList();

			var mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SirenSettings.SampleRate, 1));

			var offsets = new List<ISampleProvider>();
			foreach (double t in triggerTimes) 
			{
				wave.Position = 0;
				var length = (int)(t * SirenSettings.SampleRate + wave.Length);
				var cachedSound = new CachedSound(wave);
				var cachedSoundProvider = new CachedSoundSampleProvider(cachedSound);

				var offsetSample = new OffsetSampleProvider(cachedSoundProvider)
				{
					DelayBy = TimeSpan.FromSeconds(t),
					TakeSamples = length
				};
				
				mixer.AddMixerInput(offsetSample);
			}

			int totalLength = (int)(triggerTimes.Max() * SirenSettings.SampleRate + wave.Length) * 2;
			var stream = NAudioUtilities.WaveProviderToWaveStream(
				mixer,
				totalLength,
				wave.WaveFormat);

			DA.SetData(0, stream);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.sampleTrigger;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("75a5ac64-a05c-4639-8532-a7935ae30e39"); }
		}
	}
}