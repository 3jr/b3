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
        protected override void OnInitialized(EventArgs e)
        {
            SaveLocation = GetSaveFolder();
            base.OnInitialized(e);
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
                    throw new InvalidOperationException("SettingsSaverB3 must be used in the context of an IBallOnPlateItem of the BallOnTiltablePlate2 Project with the JanRapp.MainApp.MainWindow as Application.Current.MainWindow and BPItems must be loaded");
            }

            var item = items.First();

            return Path.Combine(GlobalSettings.ItemSettingsFolder(item.Info), "SettingSaver");
        }
    }
}
