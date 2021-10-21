using Grasshopper.Kernel;
using System;

namespace Siren.Audio
{
	public class ConvolutionComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the ConvolutionComponent class.
		/// </summary>
		public ConvolutionComponent()
		  : base("Convolution Reverb", "ConvReb",
			  "Convolution reverb. Convole audio with an impulse resonse (or any other audio)",
			  "Siren", "Effects")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
			pManager.AddParameter(new WaveStreamParameter(), "Kernel", "K", "Wave input 2", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Kernel Size", "Sz", "Size of the Kernel. Higher values have a longer decay but are slower to bounce", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Kernel Skip", "Sk", "Samples to skip ( > 0). Higher values are less accurate but faster", GH_ParamAccess.item);

			pManager[3].Optional = true;
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
			var waveIn = CachedSound.Empty;
			if (!DA.GetData(0, ref waveIn)) return;

			var kernel = CachedSound.Empty;
			if (!DA.GetData(1, ref kernel)) return;

			int kSize = 1;
			if (!DA.GetData(2, ref kSize)) return;

			int kSkip = 4;
			DA.GetData(3, ref kSkip);

			var convolution = new SampleProviders.ConvolutionProvider(waveIn.ToSampleProvider(), kernel, kSize, kSkip);

			DA.SetData(0, convolution);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.convolution;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("eb763e42-a8be-4e4c-a338-977e48126a57"); }
		}
	}
}