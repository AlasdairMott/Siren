using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using NAudio.Wave;
using NAudio.Dsp;
using NAudio.Wave.SampleProviders;
using System.Windows.Forms;
using GH_IO.Serialization;

namespace GHSynth.Components
{
	public class OscillatorComponent : GH_Component
	{
		private string wavetype;

		/// <summary>
		/// Initializes a new instance of the OscillatorComponent class.
		/// </summary>
		public OscillatorComponent()
		  : base("OscillatorComponent", "Nickname",
			  "Description",
			  "GHSynth", "Subcategory")
		{
			wavetype = "Sawtooth";
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddNumberParameter("Frequency", "F", "Frequency of the note", GH_ParamAccess.item);
			pManager.AddNumberParameter("Duration", "D", "Duration of the note", GH_ParamAccess.item);
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
			int sampleRate = 44100;
			double frequency = 440;
			if (!DA.GetData(0, ref frequency)) return;

			double duration = 1.0;
			if (!DA.GetData(1, ref duration)) return;

			SignalGeneratorType type;
			switch (wavetype) 
			{
				case "Sin": type = SignalGeneratorType.Sin; break;
				case "Sawtooth": type = SignalGeneratorType.SawTooth; break;
				case "Triangle": type = SignalGeneratorType.Triangle; break;
				case "Square": type = SignalGeneratorType.Square; break;
				default: throw new ArgumentOutOfRangeException("wavetype not valid");
			}
			var oscillator = Oscillator(frequency, duration, type);
			var wave = NAudioUtilities.WaveProviderToWaveStream(
				oscillator.ToWaveProvider16().ToSampleProvider(), 
				oscillator.TakeSamples,
				new WaveFormat(sampleRate, 1));

			DA.SetData(0, wave);
		}

		/// <summary>
		/// Provides an Icon for the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon => Properties.Resources.sin;

		/// <summary>
		/// Gets the unique ID for this component. Do not change this ID after release.
		/// </summary>
		public override Guid ComponentGuid
		{
			get { return new Guid("3fc1b43e-fc6d-413e-a0fb-e9e4d696a449"); }
		}

		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
		{
			base.AppendAdditionalComponentMenuItems(menu);

			var m_oscillator = Menu_AppendItem(menu, "Waves", null, true);
			var waves = new List<string>() { "Sin", "Sawtooth", "Triangle", "Square" };
			foreach (var w in waves) {
				var option = new ToolStripMenuItem() { 
					Text = w, 
					Checked = (w == wavetype)
				};
				m_oscillator.DropDownItems.Add(option);
			}
			m_oscillator.DropDownItemClicked += MenuWaveClicked;
		}

		private void MenuWaveClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			wavetype = ((ToolStripMenuItem)e.ClickedItem).Text;
			ExpireSolution(true);
		}

		public override bool Write(GH_IWriter writer)
		{
			writer.SetString("wavetype", wavetype);
			return base.Write(writer);
		}

		public override bool Read(GH_IReader reader)
		{
			reader.TryGetString("wavetype", ref wavetype);
			return base.Read(reader);
		}

		public OffsetSampleProvider Oscillator(double frequency, double duration, SignalGeneratorType type) 
		{
			var sampleRate = 44100;
			var signalGenerator = new SignalGenerator(sampleRate, 1);
			signalGenerator.Type = type;
			signalGenerator.Frequency = frequency;
			signalGenerator.Gain = 0.25;
			var offsetProvider = new OffsetSampleProvider(signalGenerator);
			offsetProvider.TakeSamples = (int) (sampleRate * duration);
			return offsetProvider;
		}
	}
}