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
            SaveLocation = GetSaveFolder();
            base.OnInitialized(e);

            GlobalSettings.Instance.EnviromentVariableChanged += OnEnviromentVarChanged;
        }

        void OnEnviromentVarChanged(object sender, EventArgs e)
        {
            SaveLocation = GetSaveFolder();
        }

        private string GetSaveFolder()
        {
            FrameworkElement current = (FrameworkElement)this.Parent;
            IEnumerable<BallOnTiltablePlate.JanRapp.MainApp.Helper.BPItemUI> items;
            while (true)
            {
                items = BallOnTiltablePlate.JanRapp.MainApp.Helper.BPItemUI.AllInitializedBPItems.Where(i => i.Type == current.GetType());
                if (items.Count() > 0)
                    break;

                current = (FrameworkElement)current.Parent;
                if (current == null)
                    return null; // throw new InvalidOperationException("SettingsSaverB3 must be used in the context of an IBallOnPlateItem of the BallOnTiltablePlate2 Project with the JanRapp.MainApp.MainWindow as Application.Current.MainWindow and BPItems must be loaded");
            }

            var item = items.First();

            return GlobalSettings.ItemSettingsFolder(item.Info);
        }
    }
}
