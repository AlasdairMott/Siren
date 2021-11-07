using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using NAudio.Wave;

namespace Siren
{
    public class CachedSoundGoo : GH_Goo<CachedSound>
    {
        public override bool IsValid => Value.Length > 0;

        public override string TypeName => "Cached Sound Goo" + Value.GetType().ToString();

        public override string TypeDescription => "Siren Audio data" + Value.WaveFormat.ToString();

        public CachedSoundGoo()
        {
            Value = CachedSound.Empty;
        }
        public CachedSoundGoo(CachedSound cachedSound)
        {
            if (cachedSound == null) cachedSound = CachedSound.Empty;
            Value = cachedSound;
        }

        #region casting methods
        public override bool CastTo<Q>(ref Q target)
        {
            if (typeof(Q).IsAssignableFrom(typeof(CachedSound)))
            {
                if (Value == null) target = default(Q);
                else target = (Q)(object)Value;
                return true;
            }
            target = default(Q);
            return false;
        }
        public override bool CastFrom(object source)
        {
            if (source == null) { return false; }
            if (typeof(CachedSound).IsAssignableFrom(source.GetType()))
            {
                Value = (CachedSound)source;
                return true;
            }
            else if (typeof(ISampleProvider).IsAssignableFrom(source.GetType()))
            {
                Value = new CachedSound(source as ISampleProvider);
                return true;
            }
            return false;
        }
        #endregion

        public override IGH_Goo Duplicate() => new CachedSoundGoo(Value.Clone());

        public override string ToString() => Value.ToString();

        public override bool Write(GH_IWriter writer)
        {
            if (Value != null)
            {
                writer.SetByteArray("cachedWaveBuffer", Value.ToByteArray());
            }
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("cachedWaveBuffer"))
            {
                var buffer = reader.GetByteArray("cachedWaveBuffer");
                Value = CachedSound.FromByteArray(buffer);
            }
            else Value = null;
            return true;
        }
    }

    public class WaveStreamParameter : GH_PersistentParam<CachedSoundGoo>
    {
        private float _gain = 1.0f;
        private float _offset = 0.0f;

        public WaveStreamParameter()
            : base(new GH_InstanceDescription(
                "Wave",
                "W",
                "Audio wave",
                "Siren",
                "Utilities"))
        { }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.wave;

        public override Guid ComponentGuid => new Guid("08a1577a-7dff-4163-a2c9-2dbd928626c4");

        protected override void OnVolatileDataCollected()
        {
            base.OnVolatileDataCollected();
            if (_gain == 1.0 && _offset == 0.0) return;
            ModifyGoo();
        }

        private void ModifyGoo()
        {
            var modified = new Grasshopper.Kernel.Data.GH_Structure<CachedSoundGoo>();
            foreach (CachedSoundGoo w in m_data)
            {
                var value = new SampleProviders.AttenuverterProvider(w.Value.ToSampleProvider(), _gain, _offset);
                var cachedSound = new CachedSound(value);
                modified.Append(new CachedSoundGoo(cachedSound));
            }
            m_data = modified;
        }

        protected override void Menu_AppendPromptOne(ToolStripDropDown menu) { }
        protected override void Menu_AppendPromptMore(ToolStripDropDown menu) { }
        protected override void Menu_AppendManageCollection(ToolStripDropDown menu) { }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            Menu_AppendSeparator(menu);

            var enabled = m_data.Count() > 0;
            var saveButton = Menu_AppendItem(menu, "Save File", OnSave, enabled);

            if (enabled)
            {
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
            }
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
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
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
        protected override GH_GetterResult Prompt_Singular(ref CachedSoundGoo value) => GH_GetterResult.cancel;

        protected override GH_GetterResult Prompt_Plural(ref List<CachedSoundGoo> values) => GH_GetterResult.cancel;

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
    }
}
