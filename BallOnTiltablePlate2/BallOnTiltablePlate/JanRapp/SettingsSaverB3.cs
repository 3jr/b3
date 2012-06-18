using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using BallOnTiltablePlate;
using JRapp;

namespace JRapp.WPF
{
    public class SettingsSaverB3 : SettingsSaver
    {
        static List<WeakReference> allSettingsSaverB3 = new List<WeakReference>();

        protected override void OnInitialized(EventArgs e)
        {
            SetSaveLocationToModule();
            base.OnInitialized(e);

            GlobalSettings.Instance.EnviromentVariableChanged += OnEnviromentVarChanged;
        }

        void SetSaveLocationToModule()
        {
            if (DesignMode)
            {
                this.IsEnabled = false;
                SaveLocation = @"C:\";
            }
            else
                SaveLocation = GetSaveFolder();
        }

        void OnEnviromentVarChanged(object sender, EventArgs e)
        {
            SetSaveLocationToModule();
        }

        private string GetSaveFolder()
        {
            FrameworkElement current = (FrameworkElement)this.Parent;
            IEnumerable<BallOnTiltablePlate.TimoSchmetzer.MainApp.ControlledSystemItem> items;
            while (true)
            {
                items = BallOnTiltablePlate.TimoSchmetzer.MainApp.ControlledSystemItems.CSItems.Where(i => i.Type == current.GetType());
                if (items.Count() > 0)
                    break;

                current = current.Parent as FrameworkElement;
                if (current == null)
                    return string.Empty; // throw new InvalidOperationException("SettingsSaverB3 must be used in the context of an IBallOnPlateItem of the BallOnTiltablePlate2 Project with the JanRapp.MainApp.MainWindow as Application.Current.MainWindow and BPItems must be loaded");
            }

            var item = items.First();

            return GlobalSettings.ItemSettingsFolder(item.Attribute);
        }

        public bool DesignMode
        {
            get
            {
                return (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "XDesProc");
            }
        }
    }
}
