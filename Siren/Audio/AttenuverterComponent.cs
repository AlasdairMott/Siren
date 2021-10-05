using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NAudio.Wave;
using Rhino.Geometry;

namespace Siren.Audio
{
	public class AttenuverterComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the AttenuverterComponent class.
		/// </summary>
		public AttenuverterComponent()
		  : base("AttenuverterComponent", "Nickname",
			  "Description",
			  "Siren", "VCA")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
			pManager.AddNumberParameter("Attenuation", "A", "Attenuation", GH_ParamAccess.item);
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

			var attenuation = 1.0;
			if (!DA.GetData(1, ref attenuation)) return;

			wave.Position = 0;

			var attenuverter = new SampleProviders.AttenuverterProvider(wave.ToSampleProvider(), (float) attenuation);

			var stream = NAudioUtilities.WaveProviderToWaveStream
				(attenuverter,
				(int)wave.Length,
				wave.WaveFormat);
			wave.Position = 0;

			DA.SetData(0, stream);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.attenuverter;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("aa66bb45-be86-4aa7-83ad-f3cc2666ecab"); }
		}
	}
}