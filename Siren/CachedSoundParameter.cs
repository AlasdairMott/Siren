using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using NAudio.Wave;

namespace Siren
{
    public class CachedSoundParameter : GH_PersistentParam<CachedSoundGoo>
    {
        private float _gain = 1.0f;
        private float _offset = 0.0f;
        private float _speed = 1.0f;
        public bool Modified => _gain != 1.0f || _offset != 0.0f || _speed != 1.0f;

        public CachedSoundParameter()
            : base(new GH_InstanceDescription(
                "Wave",
                "W",
                "Audio wave",
                "Siren",
                "Utilities"))
        {
        }

        protected override Bitmap Icon => Properties.Resources.wave;

        public override Guid ComponentGuid => new Guid("08a1577a-7dff-4163-a2c9-2dbd928626c4");

        protected override void OnVolatileDataCollected()
        {
            base.OnVolatileDataCollected();
            if (Modified) ModifyGoo();
        }

        private void ModifyGoo()
        {
            var modified = new Grasshopper.Kernel.Data.GH_Structure<CachedSoundGoo>();
            foreach (CachedSoundGoo w in m_data)
            {
                ISampleProvider value = new SampleProviders.AttenuverterProvider(w.Value.ToSampleProvider(), _gain, _offset);
                var cachedSound = new CachedSound(value);

                if (_speed != 1.0f)
                {
                    cachedSound = CachedSound.Stretch(cachedSound, _speed);
                }

                modified.Append(new CachedSoundGoo(cachedSound));
            }
            m_data = modified;
        }

        protected override void Menu_AppendManageCollection(ToolStripDropDown menu) { }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            AddEffect(menu, "Gain",   Properties.Resources.multiplication, _gain.ToString(),  (sender, e) => OnEffectClick(sender, e, ref _gain));
            AddEffect(menu, "Offset", Properties.Resources.addition,      _offset.ToString(), (sender, e) => OnEffectClick(sender, e, ref _offset));
            AddEffect(menu, "Speed",  Properties.Resources.stretch,       _speed.ToString(),  (sender, e) => OnEffectClick(sender, e, ref _speed));

            var removeEffects = Menu_AppendItem(menu, "Remove Effects", (sender, e) => RemoveAudioEffects(), Modified);

            Menu_AppendSeparator(menu);
            var saveButton = Menu_AppendItem(menu, "Save File", OnSave, m_data.Count() > 0);
        }

        private void AddEffect(ToolStripDropDown menu, string name, Bitmap icon, string text, EventHandler handler)
        {
            var dropdown = new ToolStripMenuItem(name, icon);
            dropdown.DropDownItems.Add(new ToolStripTextBox() { Text = text });
            dropdown.DropDownItems.Add("Commit Changes", Grasshopper.Plugin.GH_ResourceGate.OK_24x24);
            dropdown.DropDownItems.Add("Cancel Changes", Grasshopper.Plugin.GH_ResourceGate.Error_24x24);
            dropdown.DropDownItems[1].Click += handler;
            menu.Items.Add(dropdown);
        }

        private void OnEffectClick(object sender, EventArgs e, ref float value)
        {
            var text = (sender as ToolStripMenuItem).Owner.Items[0].Text;
            if (GH_Convert.ToDouble(text, out double v, GH_Conversion.Both))
            {
                value = (float)v;
                Expire();
            }
        }

        private void OnSave(object sender, EventArgs e)
        {
            var fd = new Rhino.UI.SaveFileDialog()
            {
                Title = "Save file",
                DefaultExt = "wav",
                Filter = "wav files (*.wav)|*.wav|All files (*.*)|*.*",
                FileName = "Audio"
            };

            var result = fd.ShowSaveDialog();
            if (result)
            {
                var goo = m_data.get_FirstItem(true);
                if (goo == null) return;
                goo.Value.SaveToFile(fd.FileName);
            }
        }

        private OpenFileDialog GH_OpenFileDialog(bool multiselect)
        {
            return new OpenFileDialog()
            {
                Title = "Open file",
                DefaultExt = ".wav",
                Filter = "wav files (*.wav)|*.wav|All files (*.*)|*.*",
                Multiselect = multiselect
            };
        }
        protected override GH_GetterResult Prompt_Singular(ref CachedSoundGoo value)
        {
            var fd = GH_OpenFileDialog(false);
            var result = fd.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    var audioFile = new AudioFileReader(fd.FileName);
                    value = new CachedSoundGoo(audioFile.ToSampleProvider());
                }
                catch (Exception e)
                {
                }
                return GH_GetterResult.success;
            }
            else return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<CachedSoundGoo> values)
        {
            values = new List<CachedSoundGoo>();

            var fd = GH_OpenFileDialog(true);
            var result = fd.ShowDialog();
            if (result == DialogResult.OK)
            {
                foreach (var filename in fd.FileNames)
                {
                    try
                    {
                        var audioFile = new AudioFileReader(filename);
                        values.Add(new CachedSoundGoo(audioFile.ToSampleProvider()));
                    }
                    catch (Exception e)
                    {
                    }
                }
                return GH_GetterResult.success;
            }
            else return GH_GetterResult.cancel;
        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetDouble("gain", _gain);
            writer.SetDouble("offset", _offset);
            writer.SetDouble("speed", _speed);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            double gain, offset, speed;
            gain = offset = speed = 0.0;
            if (reader.TryGetDouble("gain", ref gain)) _gain = (float)gain;
            if (reader.TryGetDouble("offset", ref offset)) _offset = (float)offset;
            if (reader.TryGetDouble("speed", ref speed)) _speed = (float)speed;

            return base.Read(reader);
        }

        public void RemoveAudioEffects()
        {
            _gain = 1.0f;
            _offset = 0.0f;
            _speed = 1.0f;
            Expire();
        }

        private void Expire()
        {
            if (Kind == GH_ParamKind.output)
            {
                (Attributes.Parent as GH_ComponentAttributes).Owner.ExpireSolution(true);
            }
            else
            {
                ExpireSolution(true);
            }
        }
    }
}
