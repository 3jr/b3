using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BallOnTiltablePlate
{
    class GlobalSettings
    {
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

        public double FPSOfAlgorithm { get; set; }

        // in Meter
        public double HalfPlateSize { get; set;}

        internal static string ItemSettingsFolder(BallOnPlateItemInfoAttribute itemInfo)
        {
            return Path.Combine(Environment.CurrentDirectory, "ItemSettings",
                string.Format("{0}_{1}_{2}", itemInfo.AuthorFirstName, itemInfo.AuthorLastName, itemInfo.ItemName));
        }

        public double MaxTilt { get; set; }
    }
}
