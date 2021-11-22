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

        public override void CreateAttributes()
        {
            m_attributes = new CachedSoundAttributes(this);
        }

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

                if (_speed != 1.0f) {
                    cachedSound = CachedSound.Stretch(cachedSound, _speed);
                }

                modified.Append(new CachedSoundGoo(cachedSound));
            }
            m_data = modified;

            if (Kind == GH_ParamKind.floating)
            {

            }
            else
            {
                if (Attributes.Parent.GetType() == typeof(GH_ComponentAttributes))
                {
                    (Attributes.Parent as GH_ComponentAttributes).Owner.Message = "Modified";
                }
            }
        }

        protected override void Menu_AppendManageCollection(ToolStripDropDown menu) { }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            var gainTextBox = new ToolStripTextBox("Expression") { Text = _gain.ToString() };
            var gainDropdown = new ToolStripMenuItem("Gain", Properties.Resources.multiplication);
            gainDropdown.DropDownItems.Add(gainTextBox);
            gainDropdown.DropDownItems.Add("Commit Changes", Grasshopper.Plugin.GH_ResourceGate.OK_24x24);
            gainDropdown.DropDownItems.Add("Cancel Changes", Grasshopper.Plugin.GH_ResourceGate.Error_24x24);
            gainDropdown.DropDownItems[1].Click += (sender, e) => OnExpressionClick(sender, e, ref _gain);
            menu.Items.Add(gainDropdown);

            var offsetTextBox = new ToolStripTextBox("Expression") { Text = _offset.ToString() };
            var offsetDropdown = new ToolStripMenuItem("Offset", Properties.Resources.addition);
            offsetDropdown.DropDownItems.Add(offsetTextBox);
            offsetDropdown.DropDownItems.Add("Commit Changes", Grasshopper.Plugin.GH_ResourceGate.OK_24x24);
            offsetDropdown.DropDownItems.Add("Cancel Changes", Grasshopper.Plugin.GH_ResourceGate.Error_24x24);
            offsetDropdown.DropDownItems[1].Click += (sender, e) => OnExpressionClick(sender, e, ref _offset);
            menu.Items.Add(offsetDropdown);

            var speedTextBox = new ToolStripTextBox("Expression") { Text = _speed.ToString() };
            var speedDropdown = new ToolStripMenuItem("Speed", Properties.Resources.stretch);
            speedDropdown.DropDownItems.Add(speedTextBox);
            speedDropdown.DropDownItems.Add("Commit Changes", Grasshopper.Plugin.GH_ResourceGate.OK_24x24);
            speedDropdown.DropDownItems.Add("Cancel Changes", Grasshopper.Plugin.GH_ResourceGate.Error_24x24);
            speedDropdown.DropDownItems[1].Click += (sender, e) => OnExpressionClick(sender, e, ref _speed);
            menu.Items.Add(speedDropdown);

            var removeEffects = Menu_AppendItem(menu, "Remove Effects", (sender, e) => RemoveAudioEffects(), Modified);

            Menu_AppendSeparator(menu);
            var saveButton = Menu_AppendItem(menu, "Save File", OnSave, m_data.Count() > 0);
        }

        private void OnExpressionClick(object sender, EventArgs e, ref float value)
        {
            var text = (sender as ToolStripMenuItem).Owner.Items[0].Text;
            if (GH_Convert.ToDouble(text, out double v, GH_Conversion.Both))
            {
                value = (float)v;
                ExpireSolution(true);
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
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            double gain, offset;
            gain = offset = 0.0;
            if (reader.TryGetDouble("gain", ref gain)) _gain = (float)gain;
            if (reader.TryGetDouble("offset", ref offset)) _offset = (float)offset;

            return base.Read(reader);
        }

        public void RemoveAudioEffects()
        {
            _gain = 1.0f;
            _offset = 0.0f;
            _speed = 1.0f;
            ExpireSolution(true);
        }
    }
}
