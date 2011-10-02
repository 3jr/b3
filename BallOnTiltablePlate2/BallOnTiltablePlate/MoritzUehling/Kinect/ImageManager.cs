using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
namespace BallOnTiltablePlate.MoritzUehling.Kinect
{
    class OldImageManager
    {
        public OldImageManager(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        int width, height;


        int xMin = int.MaxValue;
        int xMax = int.MinValue;
        int yMin = int.MaxValue;
        int yMax = int.MinValue;

        int limit = 0;

        int[,] image;

        public Rectangle GetPoints(Bitmap bitmap, int[,] data, Point point, int limit)
        {
            //b = bitmap;
            int step = limit;
            this.limit = limit;

            xMin = int.MaxValue;
            xMax = int.MinValue;
            yMin = int.MaxValue;
            yMax = int.MinValue;

            image = data;

            Stopwatch watch = new Stopwatch();

            watch.Start();

            Fill(GetPixel(point.X, point.Y), point.X, point.Y, 0);
            watch.Stop();

            Debug.WriteLine(watch.Elapsed.TotalMilliseconds);



            return new Rectangle(xMin, yMin, xMax - xMin, (yMax - yMin));
        }

        Color green = Color.FromArgb(200, 255, 0, 255);
        private bool Fill(int color, int x, int y, int depth)
        {
            if (x == 0 && y == 0)
                return false;

            try
            {
                if (x < 0 || x > width - 1 || y < 0 || y > height - 1)
                    return false;

                int c = GetPixel(x, y);

                if (c == -1)
                    return false;

                if (Math.Abs(c - color) < limit)
                {

                    if (xMin > x)
                        xMin = x;

                    if (xMax < x)
                        xMax = x;

                    if (yMin > y)
                        yMin = y;

                    if (yMax < y)
                        yMax = y;

                    SetPixel(x, y, -1);

                    Fill(c, x + 1, y, depth + 1);
                    Fill(c, x - 1, y, depth + 1);

                    Fill(c, x, y - 1, depth + 1);
                    Fill(c, x, y + 1, depth + 1);
                    return true;
                }
                else
                {

                }

                return false;
            }
            catch
            {
                return false;
            }
        }


        const int BlueIndex = 0;
        const int GreenIndex = 1;
        const int RedIndex = 2;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color">0: Blue; 1: Green; 2: Red</param>
        /// <returns></returns>
        private int GetPixel(int x, int y)
        {
            return image[(width - 1) - x, y];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color">0: Blue; 1: Green; 2: Red</param>
        /// <param name="value"></param>
        private void SetPixel(int x, int y, int value)
        {

            image[(width - 1) - x, y] = value;
        }
    }
}
