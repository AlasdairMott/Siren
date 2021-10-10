using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using NAudio.Wave;
using Rhino.Geometry;

namespace Siren
{
	public class VCAComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the VCAComponent class.
		/// </summary>
		public VCAComponent()
		  : base("VCAComponent", "Nickname",
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
			var waveIn = CachedSound.Empty;
			if (!DA.GetData(0, ref waveIn)) return;

			var cvIn = CachedSound.Empty;
			if (!DA.GetData(1, ref cvIn)) return;

			var vca = new SampleProviders.VCAProvider(waveIn.ToSampleProvider(), cvIn.ToSampleProvider());

			DA.SetData(0, vca);
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