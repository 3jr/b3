using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BallOnTiltablePlate.Input
{
    static class ImageProcessing
    {
        public struct Input
        {
            public Input(
                byte[] twoByteDepthBits,
                int depthHorizontalResulotion,

                Int32Rect clip,
                double tolerance,
                Requests requests
                )
            {
                this.twoByteDepthBits = twoByteDepthBits;
                this.depthHorizontalResulotion = depthHorizontalResulotion;

                this.clip = clip;
                this.tolerance = tolerance;
                this.requests = requests;
            }

            public byte[] twoByteDepthBits;
            public int depthHorizontalResulotion;
            public Int32Rect clip;
            public double tolerance;
            public Requests requests;
        }

        public struct Output
        {
            public Output(
                byte[] regualar,
                byte[] deltaX,
                byte[] deltaY,
                byte[] anormalyX,
                byte[] anormalyY,
                byte[] hightAnormalys,
                Vector ballPosition,
                Vector averageDelta,
                int width,
                int height
                )
            {
                this.regualar = regualar;
                this.deltaX = deltaX;
                this.deltaY = deltaY;
                this.anormalyX = anormalyX;
                this.anormalyY = anormalyY;
                this.hightAnormalys = hightAnormalys;
                
                this.ballPosition = ballPosition;
                this.averageDelta = averageDelta;
                this.width = width;
                this.height = height;
            }

            public readonly byte[] regualar;
            public readonly byte[] deltaX;
            public readonly byte[] deltaY;
            public readonly byte[] anormalyX;
            public readonly byte[] anormalyY;
            public readonly byte[] hightAnormalys;

            public readonly Vector ballPosition;
            public readonly Vector averageDelta;
            public readonly int width, height;
        }

        [Flags]
        public enum Requests
        {
            None = 0,
            Regular = 1 << 0,
            DeltaX = 1 << 1,
            DeltaY = 1 << 2,
            AnormalyX = 1 << 3,
            AnormalyY = 1 << 4,
            HightAnormalys = 1 << 5,
        }

        public static void DeltaToAverage(Microsoft.Research.Kinect.Nui.PlanarImage frame, Rect clip, double tolerance, out byte[] resultX, out byte[] resultY, out Vector average, out Vector ballPos, out byte[] ballAllPos)
        {
            //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
            int left =   (int)clip.Left;
            int top =    (int)clip.Top;
            int right =  (int)clip.Right;
            int bottom = (int)clip.Bottom;
            int width =  (int)clip.Width;
            int height = (int)clip.Height;
            int lenght = width* height;

            byte[] depthData = frame.Bits;

            int widthOfData = frame.Width * 2; //2 bytes per pixel
            int topLeftCornerOfResultInDepthData = (left * 2 + top * widthOfData); //2 bytes per pixel
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            resultX = new byte[lenght];
            resultY = new byte[lenght];
            ballAllPos = new byte[lenght];

            average = Average(frame, clip);
            byte averageX = (byte)average.X;
            byte averageY = (byte)average.Y;

            int ballX = 0, ballY = 0, ballCound = 0;

            //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            int lastdept = 0;
            int dept = 0;
            int j = topLeftCornerOfResultInDepthData;
            for (int y = 0; y < height; y++)
            {
                lastdept = depthData[j - 2] | depthData[j - 1] << 8; //2 bytes are merged to depth
                for (int x = 0; x < width; x++)
                {
                    dept = depthData[j++] | depthData[j++] << 8;
                    int delta = dept - lastdept;
                    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    int anormaly = delta - averageX;
                    if (anormaly > tolerance || anormaly < -tolerance)
                    {
                        ballX += x;
                        ballY += y;
                        ballCound++;
                    }
                    resultX[x + y * width] = (byte)(anormaly + 127);
                    //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
                    lastdept = dept;
                }
                j += widthOfData - width * 2;//2 bytes per pixel
            }

            System.Diagnostics.Debug.WriteLine("DeltaToAverage X:" + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            j = topLeftCornerOfResultInDepthData;
            for (int x = 0; x < width; x++)
            {
                lastdept = depthData[j - widthOfData] | depthData[j - widthOfData + 1] << 8;
                for (int y = 0; y < height; y++)
                {
                    dept = depthData[j] | depthData[j + 1] << 8;
                    j += widthOfData;
                    int delta = dept - lastdept;
                    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    int anormaly = delta - averageY;
                     if (anormaly > tolerance || anormaly < - tolerance)
                     {
                         ballX += x;
                         ballY += y;
                         ballCound++;
                     }
                     resultY[x + y * width] = (byte)(anormaly + 127);
                     //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
                     lastdept = dept;
                }
                j = top * widthOfData + (x + left) * 2; //2 bytes per pixel
            }

            System.Diagnostics.Debug.WriteLine("DeltaToAverage Y:" + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^


            ballX = 0;
            ballY = 0;
            ballCound = 0;
            for (int i = 0; i < lenght; i++)
            {
                int x = resultX[i] - 127;
                int y = resultY[i] - 127;

                double l = Math.Sqrt( x * x + y * y );

                if (l > tolerance)
                {
                    ballX += i % width;
                    ballY += i / width;
                    ballCound++;

                    ballAllPos[i] = 255; 
                }
            }

            System.Diagnostics.Debug.WriteLine("DeltaToAverage Lenght:" + stopwatch.ElapsedMilliseconds);

            if (ballCound == 0)
                ballPos = new Vector();
            else
                ballPos = new Vector(ballX / ballCound, ballY / ballCound);
        }

        static Vector BallPosfast(Microsoft.Research.Kinect.Nui.PlanarImage frame, Vector average, float tolerance, Int32Rect clip)
        {
            int left = clip.X;
            int top = clip.Y;
            int width = clip.Width;
            int height = clip.Height;
            int length = width * height;
            byte[] depthData = frame.Bits;
            if(frame.BytesPerPixel != 2) throw new ArgumentException("frame.BytesPerPixel must be 2");
            int widthOfData = frame.Width * 2;// two bytes per pixel
            float averageX = (float)(average.X);
            float averageY = (float)(average.Y);
            int topLeftCornerOfResultInDepthData = (left * 2 + top * widthOfData); //2 bytes per pixel

            int ballX = 0, ballY = 0, ballPointsCount = 0;
            var previousRow = new int[width];//Lets Try ushort later!!!!!!

            int j =  topLeftCornerOfResultInDepthData - widthOfData; //first previousRow is one above the topLeftCorner

            //fill previousRow
            for (int i = 0; i < width; i++)
            {
                previousRow[i] = depthData[j++] | depthData[j++] << 8;
            }

            j = topLeftCornerOfResultInDepthData;
            for (int y = 0; y < height; y++)
            {
                int lastDept = depthData[j - 2] | depthData[j - 1] << 8; //2 bytes are merged to depth
                for (int x = 0; x < width; x++)
                {
                    int dept = depthData[j++] | depthData[j++] << 8; //Lets Try creating vars outside loop
                    int deltaX = dept - lastDept;
                    int deltaY = dept - previousRow[x];
                    float anormalX = deltaX - averageX;
                    float anormalY = deltaY - averageY;
                    float lenghtOfAnormal = anormalX * anormalX + anormalY * anormalY;
                    if (lenghtOfAnormal > tolerance)
                    {
                        ballX += x;
                        ballY += y;
                        ballPointsCount++;
                    }
                }
                j += widthOfData - width * 2; //still 2 bytes per regular pixel
            }

            return new Vector(ballX / ballPointsCount, ballY / ballPointsCount);
        }

        public static Vector Average(Microsoft.Research.Kinect.Nui.PlanarImage frame, Rect clip)
        {
            ///////////////////////////////////////////////////////////////////////////////////////////
            int left = (int)clip.Left;
            int top = (int)clip.Top;
            int right = (int)clip.Right;
            int bottom = (int)clip.Bottom;
            int width = (int)clip.Width;
            int height = (int)clip.Height;
            int lenght = width * height;

            byte[] depthData = frame.Bits;

            int widthOfData = frame.Width * 2; //2 bytes per pixel
            int topLeftCornerOfResultInDepthData = (left * 2 + top * widthOfData); //2 bytes per pixel
            ///////////////////////////////////////////////////////////////////////////////////////////

            long averageX = 0;
            long averageY = 0;

            ///////////////////////////////////////////////////////////////////////////////////////////
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            int lastdept = 0;
            int dept = 0;
            int j = topLeftCornerOfResultInDepthData;
            for (int y = 0; y < height; y++)
            {
                lastdept = depthData[j - 2] | depthData[j - 1] << 8; //2 bytes are merged to depth
                for (int x = 0; x < width; x++)
                {
                    dept = depthData[j++] | depthData[j++] << 8;
                    int delta = dept - lastdept;
                    //////////////////////////////////////////////////////////////////////////////////
                    averageX += delta;
                    //////////////////////////////////////////////////////////////////////////////////
                    lastdept = dept;
                }
                j += widthOfData - width * 2;//2 bytes per pixel
            }

            j = topLeftCornerOfResultInDepthData;
            for (int x = 0; x < width; x++)
            {
                lastdept = depthData[j - widthOfData] | depthData[j - widthOfData + 1] << 8;
                for (int y = 0; y < height; y++)
                {
                    dept = depthData[j] | depthData[j + 1] << 8;
                    j += widthOfData;
                    int delta = dept - lastdept;
                    //////////////////////////////////////////////////////////////////////////////////
                    averageY += delta;
                    //////////////////////////////////////////////////////////////////////////////////
                    lastdept = dept;
                }
                j = top * widthOfData + (x + left) * 2; //2 bytes per pixel
            }

            System.Diagnostics.Debug.WriteLine("Average over depth took:" + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
            //////////////////////////////////////////////////////////////////////////////////////////

            return new Vector((double)averageX / lenght, (double)averageY / lenght);
        }


        public static byte[][] DeltaBytes(byte[] TwoByteDepthRaw, int widthOfData, int xLeft, int yTop, int xRight, int yBottom)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            widthOfData *= 2; //2 bytes per pixel

            int width = xRight - xLeft;
            int height = yBottom - yTop;
            byte[,] resultX = new byte[width, height];
            byte[,] resultY = new byte[width, height];

            System.Diagnostics.Debug.WriteLine("Creating Arrays:" + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();


            int topLeftCornerOfResultInDepthData = (xLeft * 2 + yTop * widthOfData); //2 bytes per pixel

            int lastdept = 0;
            int dept = 0;
            int j = topLeftCornerOfResultInDepthData;
            for (int y = 0; y < height; y++)
            {
                lastdept = TwoByteDepthRaw[j - 2] | TwoByteDepthRaw[j - 1] << 8; //2 bytes are merged to depth
                for (int x = 0; x < width; x++)
                {
                    dept = TwoByteDepthRaw[j] | TwoByteDepthRaw[j + 1] << 8;
                    j += 2;
                    resultX[x, y] = (byte)((dept - lastdept) * 1 + 127);
                    lastdept = dept;
                }
                j = topLeftCornerOfResultInDepthData + y * widthOfData;//2 bytes per pixel
            }

            j = topLeftCornerOfResultInDepthData;
            for (int x = 0; x < width; x++)
            {
                lastdept = TwoByteDepthRaw[j - widthOfData] | TwoByteDepthRaw[j - widthOfData + 1] << 8;
                for (int y = 0; y < height; y++)
                {
                    dept = TwoByteDepthRaw[j] | TwoByteDepthRaw[j + 1] << 8;
                    j += widthOfData;
                    resultY[x, y] = (byte)((dept - lastdept) * -1 + 127);
                    lastdept = dept;
                }
                j = topLeftCornerOfResultInDepthData + x * 2; //2 bytes per pixel
            }

            System.Diagnostics.Debug.WriteLine("Differentiate over depth took (Bitmap):" + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            byte[][] result = new byte[2][];

            result[0] = new byte[width * height];
            result[1] = new byte[width * height];

            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[0][i] = resultX[x, y];
                    result[1][i] = resultY[x, y];
                    i++;
                }
            }

            System.Diagnostics.Debug.WriteLine("Coping data:" + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();


            return result;
        }
    }
}
