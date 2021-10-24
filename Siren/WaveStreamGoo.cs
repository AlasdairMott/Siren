using System;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using NAudio.Wave;

namespace Siren
{
    public class WaveStreamGoo : GH_Goo<CachedSound>
    {
        public override bool IsValid => Value.Length > 0;

        public override string TypeName => "Gooey type name" + Value.GetType().ToString();

        public override string TypeDescription => "Gooey type description" + Value.WaveFormat.ToString();

        public WaveStreamGoo()
        {
            Value = CachedSound.Empty;
        }
        public WaveStreamGoo(CachedSound stream)
        {
            if (stream == null) stream = CachedSound.Empty;
            Value = stream;
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

        public override IGH_Goo Duplicate()
        {
            return new WaveStreamGoo(Value.Clone());
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class WaveStreamParameter : GH_Param<WaveStreamGoo>
    {
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

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            var m_save = new ToolStripMenuItem("Save file")
            {
                Enabled = m_data.Count() > 0
            };
            menu.Items.Add(m_save);
            m_save.Click += button_OnSave;
        }

        private void button_OnSave(object sender, EventArgs e)
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
    }
}
