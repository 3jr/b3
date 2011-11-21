using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BallOnTiltablePlate
{
    static class GlobalSettings
    {
        public static double PlateSize;

        internal static string ItemSettingsFolder(BallOnPlateItemInfoAttribute itemInfo)
        {
            return Path.Combine(Environment.CurrentDirectory, "ItemSettings",
                string.Format("{0}_{1}_{2}", itemInfo.AuthorFirstName, itemInfo.AuthorLastName, itemInfo.ItemName));
        }
    }
}
