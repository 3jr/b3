using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallOnTiltablePlate.JanRapp.General.Utilities
{
    public static class NumberUtil
    {
        //Use T4 for that!!!

        public static double Clamp(double value, double bound1, double bound2)
        {
            var lowLimit = bound1 < bound2 ? bound1 : bound2;
            var hightLimit = bound1 > bound2 ? bound1 : bound2;

            if (value < lowLimit)
                value = lowLimit;
            if (value > hightLimit) 
                value = hightLimit;

            return value;
        }

        public static double Max(params double[] values)
        {
            double max = values[0];

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max)
                    max = values[i];
            }

            return max;
        }

        public static double Min(params double[] values)
        {
            double min = values[0];

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < min)
                    min = values[i];
            }

            return min;
        }

        public static int Clamp(int value, int bound1, int bound2)
        {
            var lowLimit = bound1 < bound2 ? bound1 : bound2;
            var hightLimit = bound1 > bound2 ? bound1 : bound2;

            if (value < lowLimit)
                value = lowLimit;
            if (value > hightLimit)
                value = hightLimit;

            return value;
        }

        public static int Max(params int[] values)
        {
            int max = values[0];

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max)
                    max = values[i];
            }

            return max;
        }

        public static int Min(params int[] values)
        {
            int min = values[0];

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < min)
                    min = values[i];
            }

            return min;
        }

    }
}
