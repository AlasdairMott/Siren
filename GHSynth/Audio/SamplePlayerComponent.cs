using Grasshopper.Kernel;
using NAudio.Wave;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using NAudio.Wave.SampleProviders;
using System.Threading;

namespace GHSynth.Audio
{
	public class SamplePlayerComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the SamplePlayerComponent class.
		/// </summary>
		public SamplePlayerComponent()
		  : base("SamplePlayerComponent", "Nickname",
			  "Description",
			  "GHSynth", "Oscillators")
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
			var wave = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat());
			if (!DA.GetData(0, ref wave)) return;

			var points = new List<Point3d>();
			if (!DA.GetDataList(1, points)) return;

			double X = 1;
			DA.GetData(2, ref X); if (X <= 0) throw new Exception("T must be positive");

			var triggerTimes = points.Select(p => p.X / X).ToList();

			var mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(GHSynthSettings.SampleRate, 1));
			var offsets = triggerTimes.Select(t => {
				wave.Position = 0;
				var length = (int)(t * GHSynthSettings.SampleRate + wave.Length);
				var offsetSample = new OffsetSampleProvider(wave.ToSampleProvider()) { 
					DelayBy = TimeSpan.FromSeconds(t),
					TakeSamples = length
				};
				return NAudioUtilities.WaveProviderToWaveStream(
					offsetSample,
					length, 
					wave.WaveFormat);
				}
			);
			foreach (var offset in offsets) { mixer.AddMixerInput(offset); }

			var stream = NAudioUtilities.WaveProviderToWaveStream(
				mixer,
				(int) (triggerTimes.Max() * GHSynthSettings.SampleRate + wave.Length),
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