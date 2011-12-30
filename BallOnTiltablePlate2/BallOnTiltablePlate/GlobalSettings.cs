using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BallOnTiltablePlate
{
    static class GlobalSettings
    {
        static GlobalSettings()
        {
            FPSOfAlgorithm = 20;
            PlateSize = .5;
        }

        public static double FPSOfAlgorithm { get; set; }

        public static double PlateSize { get; set;}

        internal static string ItemSettingsFolder(BallOnPlateItemInfoAttribute itemInfo)
        {
            return Path.Combine(Environment.CurrentDirectory, "ItemSettings",
                string.Format("{0}_{1}_{2}", itemInfo.AuthorFirstName, itemInfo.AuthorLastName, itemInfo.ItemName));
        }
    }
}
