using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Siren.Audio
{
    public class SpeechComponent : GH_Component
    {
        private string _voice;
        private List<InstalledVoice> _installedVoices;
        private SpeechSynthesizer _speechSynthesizer;

        /// <summary>
        /// Initializes a new instance of the SpeechComponent class.
        /// </summary>
        public SpeechComponent()
          : base("Speak", "Spk",
              "Speak some words.",
              "Siren", "Oscillators")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Text to speak", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new CachedSoundParameter(), "Wave", "W", "Wave input", GH_ParamAccess.item);
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            _speechSynthesizer = new SpeechSynthesizer();
            _installedVoices = _speechSynthesizer.GetInstalledVoices().ToList();
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            base.RemovedFromDocument(document);
            _speechSynthesizer.Dispose();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var text = "";
            if (!DA.GetData(0, ref text)) return;

            ISampleProvider sound;

            using (var stream = new MemoryStream())
            using (var speaker = new SpeechSynthesizer())
            {
                speaker.Volume = 100;
                speaker.Rate = 0;

                speaker.SetOutputToWaveStream(stream);
                speaker.Speak(text);


                var cachedSound = CachedSound.FromByteArray(stream.ToArray());

                sound = new WdlResamplingSampleProvider(cachedSound.ToSampleProvider(), SirenSettings.SampleRate);
            }

            DA.SetData(0, sound);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            var voiceMenu = Menu_AppendItem(menu, "Voices", null, true);
            var voices = _installedVoices.Select(v => v.VoiceInfo.Name);
            foreach (var voice in voices)
            {
                var option = new ToolStripMenuItem()
                {
                    Text = voice,
                    Checked = (voice == _voice)
                };
                voiceMenu.DropDownItems.Add(option);
            }
            voiceMenu.DropDownItemClicked += MenuVoiceClicked;
        }

        private void MenuVoiceClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            _voice = ((ToolStripMenuItem)e.ClickedItem).Text;
            ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("voice", _voice);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            reader.TryGetString("voice", ref _voice);
            return base.Read(reader);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Speak;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("CFB520D4-1B04-462D-BEDF-D9237BA048DB");
    }
}
