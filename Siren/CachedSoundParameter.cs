using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using NAudio.Wave;

namespace Siren
{
    public class CachedSoundParameter : GH_PersistentParam<CachedSoundGoo>
    {
        private float _gain = 1.0f;
        private float _offset = 0.0f;

        public float Gain => _gain;
        public float Offset => _offset;

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
            if (_gain == 1.0 && _offset == 0.0)
            {
                StateTags.RemoveAll(t => t.GetType() == typeof(EffectsAppliedTag));
                return;
            }
            ModifyGoo();
        }

        private void ModifyGoo()
        {
            var modified = new Grasshopper.Kernel.Data.GH_Structure<CachedSoundGoo>();
            foreach (CachedSoundGoo w in m_data)
            {
                //var value = new SampleProviders.AttenuverterProvider(w.Value.ToSampleProvider(), _gain, _offset);
                var value = CachedSound.Stretch(w.Value, _gain);
                modified.Append(new CachedSoundGoo(value));
            }
            m_data = modified;

            var tag = new EffectsAppliedTag(this);
            StateTags.Add(tag);
        }

        protected override void Menu_AppendManageCollection(ToolStripDropDown menu) { }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            var enabled = m_data.Count() > 0;

            var gainTextBox = new ToolStripTextBox("Expression") { Text = _gain.ToString() };
            var gainDropdown = new ToolStripMenuItem("Gain", Properties.Resources.multiplication) { Enabled = enabled };
            gainDropdown.DropDownItems.Add(gainTextBox);
            gainDropdown.DropDownItems.Add("Commit Changes", Grasshopper.Plugin.GH_ResourceGate.OK_24x24, OnGain);
            gainDropdown.DropDownItems.Add("Cancel Changes", Grasshopper.Plugin.GH_ResourceGate.Error_24x24);
            menu.Items.Add(gainDropdown);

            var offsetTextBox = new ToolStripTextBox("Expression") { Text = _offset.ToString() };
            var offsetDropdown = new ToolStripMenuItem("Offset", Properties.Resources.addition) { Enabled = enabled };
            offsetDropdown.DropDownItems.Add(offsetTextBox);
            offsetDropdown.DropDownItems.Add("Commit Changes", Grasshopper.Plugin.GH_ResourceGate.OK_24x24, OnOffset);
            offsetDropdown.DropDownItems.Add("Cancel Changes", Grasshopper.Plugin.GH_ResourceGate.Error_24x24);
            menu.Items.Add(offsetDropdown);

            Menu_AppendSeparator(menu);
            var saveButton = Menu_AppendItem(menu, "Save File", OnSave, enabled);
        }
        private void OnGain(object sender, EventArgs e)
        {
            var text = (sender as ToolStripMenuItem).Owner.Items[0].Text;
            if (GH_Convert.ToDouble(text, out double gain, GH_Conversion.Both))
            {
                _gain = (float)gain;
                ExpireSolution(true);
            }
        }
        private void OnOffset(object sender, EventArgs e)
        {
            var text = (sender as ToolStripMenuItem).Owner.Items[0].Text;
            if (GH_Convert.ToDouble(text, out double offset, GH_Conversion.Both))
            {
                _offset = (float)offset;
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

        public override void RemoveEffects()
        {
            base.RemoveEffects();
            _gain = 1.0f;
            _offset = 0.0f;
        }
    }

    public class EffectsAppliedTag : IGH_StateTag
    {
        private readonly CachedSoundParameter _owner;

        public Bitmap Icon => Properties.Resources.multiplication;
        public string Name => "Effects";
        public string Description => "Effects are applied";
        public Rectangle Stage
        {
            get
            {
                var rectangle = _owner.Attributes.Bounds;
                return Rectangle.Round(rectangle);
            }
            set => throw new NotImplementedException();
        }
        string IGH_StateTag.StateDescription { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public EffectsAppliedTag(CachedSoundParameter owner)
        {
            _owner = owner;
        }

        public void Render(Graphics graphics)
        {
            graphics.DrawImage(Icon, Stage);
        }
    }
}
