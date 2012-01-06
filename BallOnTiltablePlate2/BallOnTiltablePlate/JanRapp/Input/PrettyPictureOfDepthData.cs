using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallOnTiltablePlate.JanRapp.Input
{
    static class PrettyPictureOfDepthData
    {
        public static byte[] PrettyPicture(byte[] depthData)
        {
            byte[] result = new byte[depthData.Length / 2];

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            for (int i = 0, j = 0; j < depthData.Length; j += 2, i++)
            {
                result[i] = (byte)(((depthData[j] | depthData[j + 1] << 8) - 800) / 2);
            }

            System.Diagnostics.Debug.WriteLine("PrettyPicture: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            return result;
        }
    }
}
