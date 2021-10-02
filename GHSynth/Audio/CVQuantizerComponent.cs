using Grasshopper.Kernel;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GHSynth.Audio
{
	public class CVQuantizerComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the CVQuantizer class.
		/// </summary>
		public CVQuantizerComponent()
		  : base("CVQuantizer", "Nickname",
			  "Description",
			  "GHSynth", "CV Control")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
			pManager.AddNumberParameter("Notes", "N", "Notes (as real numbers [0,12)) to quantize to", GH_ParamAccess.list);
			pManager[1].Optional = true;
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
			var cv = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat());
			if (!DA.GetData(0, ref cv)) return;

			var scale = new List<double>(); //Enumerable.Range(0, 11).Select(x => (double)x).ToList();
			DA.GetDataList(1, scale);

			if (scale.Min() < 0 || scale.Max() >= 12) throw new ArgumentOutOfRangeException("Values in scale must be from [0,12)");
			scale = scale.Select(s => s * (1.0 / 12.0)).ToList();

			var quantized = new SampleProviders.CVQuantizer(cv.ToSampleProvider(), scale);

			var stream = NAudioUtilities.WaveProviderToWaveStream
				(quantized,
				(int)cv.Length,
				cv.WaveFormat);
			cv.Position = 0;

			DA.SetData(0, stream);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.quantize;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("891c530d-4aca-4677-9aa5-e31ed25f8109"); }
		}
	}
}