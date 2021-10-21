using Grasshopper.Kernel;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Siren.Components
{
	public class UnityMixerComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the MixerComponent class.
		/// </summary>
		public UnityMixerComponent()
		  : base("Mixer", "Mixer",
			  "Additively combines multiple signals into a single signal.",
			  "Siren", "VCA")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.list);
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
			var wavesIn = new List<CachedSound>();
			if (!DA.GetDataList(0, wavesIn)) return;
			if (wavesIn.Contains(null))
            {
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "An invalid wave input was provided; check for issues with earlier components.");
				return;
			}

			var multiplier = 1 / wavesIn.Count;
			var mixer = new MixingSampleProvider(wavesIn.Select(s => s.ToSampleProvider()));

			DA.SetData(0, mixer);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.mixer;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("77093d78-0394-40ae-aea3-d3892d6ec1e1"); }
		}
	}
}