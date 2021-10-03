﻿using System;
using System.Collections.Generic;
using System.Drawing;
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

                for (int i = 0; i < menu.Items.Count; i++)
                {
                    var menuitem = menu.Items[i] as ToolStripMenuItem;
                    if (menuitem != null && menuitem.Text == "GHSynth") return;
                }

                var ghSynthMenu = new ToolStripMenuItem("GHSynth");
                menu.Items.Add(ghSynthMenu);

                var intSettings = new List<(string, Bitmap)>() {
                    ("Sample Rate", Properties.Resources.Hz),
                    ("Time Scale", null),
                    ("Amplitude Scale", null),
                    ("Tempo", null)
                };

                foreach (var setting in intSettings) { 
                    var option = new ToolStripMenuItem()
                    {
                        Text = setting.Item1,
                        Image = setting.Item2
                    };
                    ghSynthMenu.DropDownItems.Add(option);
                }
                ghSynthMenu.DropDownItemClicked += intOptionClicked;

            }
        }

        private void intOptionClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var optionText = ((ToolStripMenuItem)e.ClickedItem).Text;
            double newRate;
            int min, max;
            switch (optionText) {
                case "Sample Rate": 
                    newRate = GHSynthSettings.SampleRate;
                    min = 8000; max = 44100;
                    break;
                case "Time Scale":  
                    newRate = GHSynthSettings.TimeScale;
                    min = 1; max = 100000;
                    break;
                case "Amplitude Scale": 
                    newRate = GHSynthSettings.AmplitudeScale;
                    min = 1; max = 100000;
                    break;
                case "Tempo": 
                    newRate = GHSynthSettings.Tempo;
                    min = 1; max = 240;
                    break;
                default: return;
            }

            var dialog_result = Rhino.UI.Dialogs.ShowNumberBox(
                optionText,
                optionText,
                ref newRate,
                min, 
                max);
            if (dialog_result && Grasshopper.Instances.ActiveCanvas.Document != null) 
                Grasshopper.Instances.ActiveCanvas.Document.ExpireSolution();
        }
    }
}