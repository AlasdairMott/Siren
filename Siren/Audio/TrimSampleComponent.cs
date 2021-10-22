using Grasshopper.Kernel;
using NAudio.Wave.SampleProviders;
using System;

namespace Siren.Audio
{
	public class TrimSampleComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the TrimSampleComponent class.
		/// </summary>
		public TrimSampleComponent()
		  : base("Trim Sample", "Trim",
			  "Trim a sample's start and end point",
			  "Siren", "Editing")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
			pManager.AddNumberParameter("Start", "S", "Start (in seconds)", GH_ParamAccess.item);
			pManager.AddNumberParameter("End", "E", "End (in seconds)", GH_ParamAccess.item);

			pManager[1].Optional = true;
			pManager[2].Optional = true;
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

			double start = 0;
			double end = 0;

			DA.GetData(1, ref start);
			DA.GetData(2, ref end);

			var a = TimeSpan.FromSeconds(start);
			var b = waveIn.TotalTime - TimeSpan.FromSeconds(start + end);

			var trimmed = new OffsetSampleProvider(waveIn.ToSampleProvider())
			{
				SkipOver = TimeSpan.FromSeconds(start),
				Take = waveIn.TotalTime - TimeSpan.FromSeconds(start + end)
			};

			DA.SetData(0, trimmed);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.trim;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("5a282ab2-22af-452f-b968-d7dc75ba8d96"); }
		}
	}
}