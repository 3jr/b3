using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using BallOnTiltablePlate.JanRapp.Utilities;
using Ionic.Zip;

namespace BallOnTiltablePlate
{
    class GlobalSettings
    {
        public const string b3SettingsSaverSaveLocationVariableName = "b3SettingsSaverSaveLocation";
        public const string b3SettingsSaverBackupZipVariableName = "b3SettingsSaverBackupLocation";
        public const string b3SettingSaverBackupExtension = ".b3SettingsSaverBackup";

        static GlobalSettings()
        {
            Instance = new GlobalSettings();
        }

        public GlobalSettings()
        {
            FPSOfAlgorithm = 20;
            HalfPlateSize = .25;
            MaxTilt = Math.PI * 5 / 64;
        }

        public static GlobalSettings Instance { get; private set; }

        public  EventHandler EnviromentVariableChanged;

        public double FPSOfAlgorithm { get; set; }

        // in Meter
        public double HalfPlateSize { get; set;}

        public static string SettingsFolder()
        {
            string enviromentSetSaveLocation = Environment.
                GetEnvironmentVariable(b3SettingsSaverSaveLocationVariableName, EnvironmentVariableTarget.User);

            if (Directory.Exists(SettinsSaverSaveLocation))
                return enviromentSetSaveLocation;
            
            MessageBox.Show("The enviroment Variable " + b3SettingsSaverSaveLocationVariableName + " is not set to a valid path, witch would be used to store the SettingsSaver Save files. This is also possible in the Global Settings under Settings.\n\rThe Current Directory is default and currently used.");
            return Environment.CurrentDirectory;
        }
        
        public static string ItemSettingsFolder(BallOnPlateItemInfoAttribute itemInfo)
        {
            return Path.Combine(SettingsFolder(),
                string.Format("{0}_{1}_{2}", itemInfo.AuthorFirstName, itemInfo.AuthorLastName, itemInfo.ItemName));
        }

        public Vector ToValidTilt(Vector tilt)
        {
            return new Vector(
                NumberUtil.Clamp(tilt.X, -this.MaxTilt, this.MaxTilt).ToNoNaN(),
                NumberUtil.Clamp(tilt.Y, -this.MaxTilt, this.MaxTilt).ToNoNaN()
                );
        }

        public double MaxTilt { get; set; }

        public static string SettinsSaverSaveLocation 
        { 
            get
            {
                return Environment.GetEnvironmentVariable(
                    GlobalSettings.b3SettingsSaverSaveLocationVariableName,
                    EnvironmentVariableTarget.User);
            }
        }

        public static string SettinsSaverBackupZip
        {
            get
            {
                return Environment.GetEnvironmentVariable(
                    GlobalSettings.b3SettingsSaverBackupZipVariableName,
                    EnvironmentVariableTarget.User);
            }
        }

        public static void BackupSettingsSaves()
        {
            string settinsSaverBackupZip = SettinsSaverBackupZip;

            if (string.IsNullOrWhiteSpace(settinsSaverBackupZip) ||
                !settinsSaverBackupZip.EndsWith(GlobalSettings.b3SettingSaverBackupExtension))
            {
                MessageBox.Show("The Enviroment variable \"" + GlobalSettings.b3SettingsSaverBackupZipVariableName + "\" must be set to a valid location with the correct extentsion (*" + GlobalSettings.b3SettingSaverBackupExtension + ") \n\r\nBecasue of this no Backup was created!");
                return;
            }

            using (ZipFile zip = new ZipFile(SettinsSaverBackupZip))
            {
                zip.AddDirectory(SettinsSaverSaveLocation, DateTime.Now.ToString("s"));
                zip.Save();
            }
        }
    }
}
