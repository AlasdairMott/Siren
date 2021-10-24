﻿using System;
using System.Collections.Generic;
using System.Linq;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using NAudio.Wave.SampleProviders;
using Siren.Utilities;

namespace Siren.Components
{
    public class CVOscillatorComponent : GH_Component
    {
        protected int _selectedWave;
        protected List<WaveForm> _waveOptions = new List<WaveForm>()
        {
            new WaveForm("Sin", Properties.Resources.wavef_sin, SignalGeneratorType.Sin),
            new WaveForm("Sawtooth", Properties.Resources.wavef_saw, SignalGeneratorType.SawTooth),
            new WaveForm("Triangle", Properties.Resources.wavef_triangle, SignalGeneratorType.Triangle),
            new WaveForm("Square", Properties.Resources.wavef_square, SignalGeneratorType.Square)
        };

        protected struct WaveForm
        {
            public string Title { get; }
            public System.Drawing.Bitmap Icon { get; }
            public SignalGeneratorType Type { get; }
            public WaveForm(string title, System.Drawing.Bitmap icon, SignalGeneratorType type)
            {
                Title = title; Icon = icon; Type = type;
            }
        }

        /// <summary>
        /// Initializes a new instance of the CVOscillatorComponent class.
        /// </summary>
        public CVOscillatorComponent()
          : base("CV Oscillator", "Osc",
              "Produces an audible signal using a specific waveform type at the given pitch.",
              "Siren", "Oscillators")
        {
            _selectedWave = 0; // Sin default
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

        public override void CreateAttributes() // Setup custom inline icons within component
        {
            var waveIcons = _waveOptions.Select(o => o.Icon).ToList();
            m_attributes = new GH_ToggleAttributes(this, SetWaveformFromIcon, waveIcons, _selectedWave);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new WaveStreamParameter(), "", "", "Wave output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var cvIn = CachedSound.Empty;
            if (!DA.GetData(0, ref cvIn)) return;

            int octave = 0;
            double semi = 0;
            DA.GetData(1, ref octave);
            DA.GetData(2, ref semi);

            var signalGenerator = new SignalGenerator(SirenSettings.SampleRate, 1)
            {
                Type = _waveOptions[_selectedWave].Type,
                Frequency = 440,
                Gain = 0.25
            };

            var oscillator = new SampleProviders.OscillatorProvider(cvIn.ToSampleProvider(), signalGenerator, octave, semi);

            DA.SetData(0, oscillator);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.waveforms;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("31bfd5f1-556d-46a5-ab52-949aadf2372a"); }
        }

        protected void SetWaveformFromIcon(int indexOfClickedIcon)
        {
            _selectedWave = indexOfClickedIcon;
            ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("wavetype", _waveOptions[_selectedWave].Title);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            string waveformTitle = "";
            if (reader.TryGetString("wavetype", ref waveformTitle))
            {
                _selectedWave = _waveOptions.FindIndex(w => w.Title == waveformTitle);
                (m_attributes as GH_ToggleAttributes).IndexOfSelectedIcon = _selectedWave; // Need to refresh to pass newly-loaded state
            }

            return base.Read(reader);
        }
    }
}
