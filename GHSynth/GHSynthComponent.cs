using Grasshopper.Kernel;
using Synth;
using System;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace GHSynth
{
	public class GHSynthComponent : GH_Component
	{
		public override GH_Exposure Exposure => GH_Exposure.hidden;

		/// <summary>
		/// Each implementation of GH_Component must provide a public 
		/// constructor without any arguments.
		/// Category represents the Tab in which the component will appear, 
		/// Subcategory the panel. If you use non-existing tab or panel names, 
		/// new tabs/panels will automatically be created.
		/// </summary>
		public GHSynthComponent()
		  : base("GHSynth", "Nickname",
			  "Description",
			  "GHSynth", "Subcategory")
		{
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddNumberParameter("Pitch", "P", "Pitch of the sin wave", GH_ParamAccess.item);
			pManager.AddNumberParameter("Duration", "T", "Duration of the wave", GH_ParamAccess.item);
			
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddGenericParameter("Sample", "S", "Sound sample", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object can be used to retrieve data from input parameters and 
		/// to store data in output parameters.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			double pitch = 1;
			if (!DA.GetData(0, ref pitch)) return;

			//convert V/O to Hz
			//A1: 1v is equivalent to 55kHz
			//A4: 4v is equivalent to 440kHz
			//A5: 5v is equivalenet to 880KHz
			//pitch = pitch * 55;
			float frequency = (float) Math.Pow(2, pitch - 1) * 55;

			double duration = 0.5;
			if (!DA.GetData(1, ref duration)) return;

			var sample = new BasicSynthesizer(frequency, (int) (duration * 44100));
			DA.SetData(0, sample);
		}

		/// <summary>
		/// Provides an Icon for every component that will be visible in the User Interface.
		/// Icons need to be 24x24 pixels.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.wave;

		/// <summary>
		/// Each component must have a unique Guid to identify it. 
		/// It is vital this Guid doesn't change otherwise old ghx files 
		/// that use the old ID will partially fail during loading.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("34088530-c2a8-44d0-b569-91bef5bdad58"); }
		}
	}
}
