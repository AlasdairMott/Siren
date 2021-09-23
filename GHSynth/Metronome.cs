using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using System.Timers;

namespace GHSynth
{
	public class Metronome : GH_Component
	{
		private Timer timer;
		private int beat;

		/// <summary>
		/// Initializes a new instance of the Metronome class.
		/// </summary>
		public Metronome()
		  : base("Metronome", "M",
			  "Description",
			  "GHSynth", "Subcategory")
		{
			beat = 0;
			timer = new Timer(1000);
			timer.Elapsed += OnTimedEvent;

		}

		private void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			//ExpireDownStreamObjects();
			//CollectData();
			beat += 1;
			beat %= 4;
			ExpireSolution(true);
		}
		
		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddNumberParameter("Tempo", "T", "Tempo of the metronome", GH_ParamAccess.item);
			pManager.AddBooleanParameter("Enabled", "E", "Enable the metronome", GH_ParamAccess.item);
			pManager[0].Optional = true;
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddIntegerParameter("1/1", "1/1", "Timber bang", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			double tempo = 120;
			DA.GetData(0, ref tempo);

			bool enabled = false;
			if (!DA.GetData(1, ref enabled)) return;
			if (!enabled) return;

			DA.SetData(0, beat);

			timer.Enabled = true;

			timer.Interval = 60000 / tempo;

			
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.metronome;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("160fccd5-5b08-49fa-8f03-4294148a16a0"); }
		}
	}
}