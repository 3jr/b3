using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BallOnTiltablePlate.JanRapp.General.Utilities
{
    public static class ConvertUtil
    {
        public static Int32Rect ToIntRect(Rect rect)
        {
            return new Int32Rect((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
        }
    }
}
