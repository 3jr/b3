﻿using System;
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

        Queue<PointInfo> pointsToCheck = new Queue<PointInfo>();

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
            #region Füllen
            pointsToCheck.Enqueue(new PointInfo(point.X, point.Y, GetPixel(point.X, point.Y)));

            while (pointsToCheck.Count != 0)
            {
                Fill(pointsToCheck.Dequeue());
            }

            #endregion

            #region kanten finden...

            //Linke kante...

            #endregion


            watch.Stop();

            Debug.WriteLine(watch.Elapsed.TotalMilliseconds);
           

            return new Rectangle(xMin, yMin, xMax - xMin, (yMax - yMin));
        }

        Color green = Color.FromArgb(200, 255, 0, 255);
        private bool Fill(PointInfo p)
        {
            if (p.x == 0 && p.y == 0)
                return false;

            try
            {
                if (p.x < 0 || p.x > width - 1 || p.y < 0 || p.y > height - 1)
                    return false;

                int c = GetPixel(p.x, p.y);

                if (c == -1)
                    return true;

                if (c == 0)
                    return false;

                if (Math.Abs(c - p.neighborColor) < limit)
                {

                    if (xMin > p.x)
                        xMin = p.x;

                    if (xMax < p.x)
                        xMax = p.x;

                    if (yMin > p.y)
                        yMin = p.y;

                    if (yMax < p.y)
                        yMax = p.y;

                    SetPixel(p.x, p.y, -1);

                    pointsToCheck.Enqueue(new PointInfo(p.x + 1, p.y, c));
                    pointsToCheck.Enqueue(new PointInfo(p.x - 1, p.y, c));
                    pointsToCheck.Enqueue(new PointInfo(p.x, p.y + 1, c));
                    pointsToCheck.Enqueue(new PointInfo(p.x, p.y - 1, c));
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

    struct PointInfo
    {
        public int x;
        public int y;
        public int neighborColor;

        public PointInfo(int x, int y, int nColor)
        {
            this.x = x;
            this.y = y;
            neighborColor = nColor;
        }
    }
}
