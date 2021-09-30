using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NAudio.Wave;
using Rhino.Geometry;

namespace GHSynth
{
	public class VCAComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the VCAComponent class.
		/// </summary>
		public VCAComponent()
		  : base("VCAComponent", "Nickname",
			  "Description",
			  "GHSynth", "VCA")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
			pManager.AddParameter(new WaveStreamParameter(), "Amplitude", "A", "Amplitude input", GH_ParamAccess.item);
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			var wave = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat());
			if (!DA.GetData(0, ref wave)) return;

			var amplitude = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat());
			if (!DA.GetData(1, ref amplitude)) return;

			wave.Position = 0;
			amplitude.Position = 0;

			var vca = new SampleProviders.VCAProvider(wave.ToSampleProvider(), amplitude.ToSampleProvider());

			var stream = NAudioUtilities.WaveProviderToWaveStream
				(vca, 
				(int) wave.Length,
				wave.WaveFormat);
			wave.Position = 0;
			amplitude.Position = 0;

			DA.SetData(0, stream);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.VCA;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("b252a87b-0a79-4fe8-abc5-bd7bc55b4bc0"); }
		}
	}
}