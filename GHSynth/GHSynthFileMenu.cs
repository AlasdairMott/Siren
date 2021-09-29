using System;
using System.Windows.Forms;

namespace GHSynth
{
    class GHSynthFileMenu
    {
        Eto.Forms.UITimer _timer;

        public void AddToMenu()
        {
            if (_timer != null)
                return;
            _timer = new Eto.Forms.UITimer();
            _timer.Interval = 1;
            _timer.Elapsed += SetupMenu;
            _timer.Start();
        }

        void SetupMenu(object sender, EventArgs e)
        {
            var editor = Grasshopper.Instances.DocumentEditor;
            if (null == editor || editor.Handle == IntPtr.Zero)
                return;

            var controls = editor.Controls;
            if (null == controls || controls.Count == 0)
                return;

            _timer.Stop();
            foreach (var ctrl in controls)
            {
                var menu = ctrl as Grasshopper.GUI.GH_MenuStrip;
                if (menu == null)
                    continue;

                var ghSynthMenu = new ToolStripMenuItem("GHSynth");
                menu.Items.Add(ghSynthMenu);

                var button_setSampleRate = new ToolStripMenuItem() { Text = "Sample Rate", Checked = false };
                button_setSampleRate.Click += SampleRateClicked;
                ghSynthMenu.DropDownItems.Add(button_setSampleRate);

            }
        }

        private void SampleRateClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}