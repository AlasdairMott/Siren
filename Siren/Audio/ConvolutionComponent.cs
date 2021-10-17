using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Siren.Audio
{
	public class ConvolutionComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the ConvolutionComponent class.
		/// </summary>
		public ConvolutionComponent()
		  : base("Convolution", "Nickname",
			  "Description",
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
			pManager.AddIntegerParameter("Kernel Size", "Ks", "Size of the Kernel", GH_ParamAccess.item);
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

			var convolution = new SampleProviders.ConvolutionProvider(waveIn.ToSampleProvider(), kernel, kSize);

			DA.SetData(0, convolution);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon
		{
			get
			{
				//You can add image files to your project resources and access them like this:
				// return Resources.IconForThisComponent;
				return null;
			}
		}

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("eb763e42-a8be-4e4c-a338-977e48126a57"); }
		}
	}
}