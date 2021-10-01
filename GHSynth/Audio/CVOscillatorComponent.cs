using GH_IO.Serialization;
using Grasshopper.Kernel;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GHSynth.Components
{
	public class CVOscillatorComponent : GH_Component
	{
		private string wavetype;

		/// <summary>
		/// Initializes a new instance of the CVOscillatorComponent class.
		/// </summary>
		public CVOscillatorComponent()
		  : base("CVOscillatorComponent", "Nickname",
			  "Description",
			  "GHSynth", "Oscillators")
		{
			wavetype = "Sawtooth";
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddParameter(new WaveStreamParameter(), "Frequency", "F", "Frequency of the note", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Octave", "O", "Octave of the note", GH_ParamAccess.item);
			pManager.AddNumberParameter("Tuning", "T", "Note tuning", GH_ParamAccess.item);

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
			var cv = new RawSourceWaveStream(new byte[0], 0, 0, new WaveFormat());
			if (!DA.GetData(0, ref cv)) return;

			int octave = 0;
			double semi = 0;
			DA.GetData(1, ref octave);
			DA.GetData(2, ref semi);

			cv.Position = 0;

			SignalGeneratorType type;
			switch (wavetype)
			{
				case "Sin": type = SignalGeneratorType.Sin; break;
				case "Sawtooth": type = SignalGeneratorType.SawTooth; break;
				case "Triangle": type = SignalGeneratorType.Triangle; break;
				case "Square": type = SignalGeneratorType.Square; break;
				default: throw new ArgumentOutOfRangeException("wavetype not valid");
			}

			var signalGenerator = new SignalGenerator(GHSynthSettings.SampleRate, 1)
			{
				Type = type,
				Frequency = 440,
				Gain = 0.25
			};

			var oscillator = new SampleProviders.OscillatorProvider(cv.ToSampleProvider(), signalGenerator, octave, semi);

			var stream = NAudioUtilities.WaveProviderToWaveStream
				(oscillator,
				(int)cv.Length,
				cv.WaveFormat);
			cv.Position = 0;

			DA.SetData(0, stream);
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
			get { return new Guid("31bfd5f1-556d-46a5-ab52-949aadf2372a"); }
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
	}
}