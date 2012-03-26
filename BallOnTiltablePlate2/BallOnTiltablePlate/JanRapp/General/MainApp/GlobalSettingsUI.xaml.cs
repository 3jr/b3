using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;
using System.IO;

namespace BallOnTiltablePlate.JanRapp.MainApp
{
    /// <summary>
    /// Interaction logic for GeneralSettingsUI.xaml
    /// </summary>
    public partial class GlobalSettingsUI : UserControl
    {
        public GlobalSettingsUI()
        {
            InitializeComponent();
            this.GlobalSettingsSS.SaveLocation = GlobalSettings.SettingsFolder();
            this.SettingsSaverSaveLocation.Text = GlobalSettings.SettinsSaverSaveLocation;
            this.SettingsSaverBackupLocation.Text = GlobalSettings.SettinsSaverBackupZip;
        }

        private void SetPhysicalKinectAngle_Click(object sender, RoutedEventArgs e)
        {
            Runtime nui = Runtime.Kinects[0];
            nui.Initialize(RuntimeOptions.UseColor);
            nui.NuiCamera.ElevationAngle = (int)PhysicalKinectAngle.Value;
            nui.Uninitialize();
        }

        private void BrowseSaveLocation_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SettingsSaverSaveLocation.Text = fbd.SelectedPath;
            }
        }

        private void BrowseBackupLocation_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();

            sfd.AddExtension = true;
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = false;
            sfd.CreatePrompt = false;
            sfd.OverwritePrompt = false;
            sfd.InitialDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, @"\..");
            sfd.AutoUpgradeEnabled = true;
            sfd.Title = "Create and choose File to store the SettingsSaver Backups";
            sfd.Filter = "b3-SettingsSaver-Backup|*" + GlobalSettings.b3SettingSaverBackupExtension;
            sfd.DefaultExt = "*" + GlobalSettings.b3SettingSaverBackupExtension;

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SettingsSaverBackupLocation.Text = sfd.FileName;
            }
        }

        private void SetEnviromentVariables_Click(object sender, RoutedEventArgs e)
        {
            //Set on User Level
            System.Diagnostics.ProcessStartInfo elevatedProcessInfo = new System.Diagnostics.ProcessStartInfo();
            elevatedProcessInfo.FileName = "cmd";
            elevatedProcessInfo.Arguments =
                "\"/c SETX " + GlobalSettings.b3SettingsSaverSaveLocationVariableName + " \"" + SettingsSaverSaveLocation.Text + "\" " +
                "&& SETX " + GlobalSettings.b3SettingsSaverBackupZipVariableName + " \"" + SettingsSaverBackupLocation.Text + "\" ";
            elevatedProcessInfo.UseShellExecute = true;
            elevatedProcessInfo.Verb = "runas";
            elevatedProcessInfo.CreateNoWindow = true;

            var process = System.Diagnostics.Process.Start(elevatedProcessInfo);

            process.WaitForExit();

            if(GlobalSettings.Instance.EnviromentVariableChanged != null)
                GlobalSettings.Instance.EnviromentVariableChanged(this, new EventArgs());

            this.GlobalSettingsSS.SaveLocation = GlobalSettings.SettinsSaverSaveLocation;
        }
    }
}