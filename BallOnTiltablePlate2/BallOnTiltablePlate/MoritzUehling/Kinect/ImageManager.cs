using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

using Windows = System.Windows;
using Timo = BallOnTiltablePlate.TimoSchmetzer.Utilities;


namespace BallOnTiltablePlate.MoritzUehling.Kinect
{
    class ImageManager
    {
        public ImageManager(int width, int height)
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

        public Rectangle GetPoints(int[,] data, Point point, int limit)
        {
            //b = bitmap;
            int step = limit;
            this.limit = limit;

            xMin = int.MaxValue;
            xMax = int.MinValue;
            yMin = int.MaxValue;
            yMax = int.MinValue;

            if (point.X == 0 && point.Y == 0)
                return new Rectangle(Point.Empty, Point.Empty, Point.Empty, Point.Empty);

            image = (int[,])data.Clone();

            #region Füllen
            pointsToCheck.Enqueue(new PointInfo(point.X, point.Y, GetPixel(point.X, point.Y)));

            while (pointsToCheck.Count != 0)
            {
                Fill(pointsToCheck.Dequeue());
            }

            #endregion

            #region Ball finden...
            if (xMax > 0 && xMin > 0)
			{

				#region Rechteck erstellen
				//Rectangle rect = new Rectangle(CalcIntersect(returnVal[0], returnVal[3]), CalcIntersect(returnVal[0], returnVal[1]), CalcIntersect(returnVal[1], returnVal[2]), CalcIntersect(returnVal[2], returnVal[3]));


				Point middle = new Point((xMin + xMax) / 2, (yMax + yMin) / 2);

				Rectangle rect = new Rectangle(new Point(middle.X - 5, middle.Y - 5), new Point(middle.X + 5, middle.Y - 5), new Point(middle.X + 5, middle.Y + 5), new Point(middle.X - 5, middle.Y + 5));


				image = data;// (int[,])data.Clone();
				rect.points[0] = FindBall(data);


				#endregion

                return rect;

            }
            #endregion
			return new Rectangle(Point.Empty, Point.Empty, Point.Empty, Point.Empty);
        }


		private int distanceSqr(Point p1, Point p2)
		{
			return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
		}


		private Point FindBall(int[,] data)
		{
			limit -= 2;

			for (int x = xMin; x < xMax; x += 3)
			{
				for (int y = yMin; y < yMax; y += 3)
				{
					pointsToCheck.Enqueue(new PointInfo(x, y, GetPixel(x, y)));

					int filled = 0;

					while (pointsToCheck.Count != 0)
					{
						filled++;
						Fill(pointsToCheck.Dequeue());
					}

					if (filled > 10)
					{
						return new Point(x, y);
					}
				}
			}

			return new Point(0, 0);
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

        private Point CalcIntersect(Timo.Mathematics.LineEquation eq1, Timo.Mathematics.LineEquation eq2)
        {
            double x = (eq2.c - eq1.c) / (eq1.m - eq2.m);
            int y = (int)(eq1.m * x + eq1.c);
            return new Point((int)x, y);

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

    public struct Rectangle
    {
        public Point[] points;

        public Rectangle(Point tl, Point tr, Point br, Point bl)
        {
            points = new Point[] { tl, tr, br, bl };
        }
    }

}
