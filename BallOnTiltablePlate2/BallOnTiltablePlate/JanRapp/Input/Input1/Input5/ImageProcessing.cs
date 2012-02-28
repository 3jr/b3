using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BallOnTiltablePlate.JanRapp.Input05
{
    static class ImageProcessing
    {
        public static Vector InvalidVector = new Vector(double.NaN, double.NaN);

        public struct Input
        {
            public Input(
                byte[] twoByteDepthBits,
                int depthHorizontalResulotion,

                Int32Rect clip,
                float tolerance,
                Requests requests,
                int minHightAnormalities
                )
            {
                this.twoByteDepthBits = twoByteDepthBits;
                this.depthHorizontalResulotion = depthHorizontalResulotion;

                this.clip = clip;
                this.tolerance = tolerance;
                this.requests = requests;
                this.minHightAnormalities = minHightAnormalities;
            }

            public byte[] twoByteDepthBits;
            public int depthHorizontalResulotion;
            public Int32Rect clip;
            public float tolerance;
            public Requests requests;
            public int minHightAnormalities;
        }

        public struct Output
        {
            public Output(
                byte[] regualar,
                byte[] depth,
                byte[] deltaX,
                byte[] deltaY,
                byte[] anormalyX,
                byte[] anormalyY,
                byte[] hightAnormalys,
                Vector ballPosition,
                Vector averageDelta,
                Int32Rect clip
                )
            {
                this.regualar = regualar;
                this.depth = depth;
                this.deltaX = deltaX;
                this.deltaY = deltaY;
                this.anormalyX = anormalyX;
                this.anormalyY = anormalyY;
                this.hightAnormalys = hightAnormalys;
                
                this.ballPosition = ballPosition;
                this.averageDelta = averageDelta;
                this.clip = clip;
            }

            public readonly byte[] regualar;
            public readonly byte[] depth;
            public readonly byte[] deltaX;
            public readonly byte[] deltaY;
            public readonly byte[] anormalyX;
            public readonly byte[] anormalyY;
            public readonly byte[] hightAnormalys;

            public readonly Vector ballPosition;
            public readonly Vector averageDelta;
            public readonly Int32Rect clip;
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

        public static Vector Average(byte[] depthData, int depthHorizontalResulotion, Int32Rect clip)
        {
            ///////////////////////////////////////////////////////////////////////////////////////////
            int left = clip.X;
            int top = clip.Y;
            int width = clip.Width;
            int height = clip.Height;
            int lenght = width * height;

            int widthOfData = depthHorizontalResulotion * 2; //2 bytes per pixel
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

            //System.Diagnostics.Debug.WriteLine("Average over depth took:" + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
            //////////////////////////////////////////////////////////////////////////////////////////

            return new Vector((double)averageX / lenght, (double)averageY / lenght);
        }

        public static Vector BallPositionFast(byte[] depthData, int depthHorizontalResulotion, Vector average, float tolerance, Int32Rect clip, int minHeightAnormalities,
            out byte[] deptArr, out byte[] deltaXArr, out byte[] deltaYArr, out byte[] anormalXArr, out byte[] anormalYArr, out byte[] hightAnormaltiesArr, int visibilytyMultiplier
            )
        {
            ////////////////////////////////////////////////////////
            hightAnormaltiesArr = new byte[clip.Width * clip.Height]; ////
            ////
            deptArr = new byte[clip.Width * clip.Height];   ////
            deltaXArr = new byte[clip.Width * clip.Height];   ////
            deltaYArr = new byte[clip.Width * clip.Height];   ////
            anormalXArr = new byte[clip.Width * clip.Height];   ////
            anormalYArr = new byte[clip.Width * clip.Height];   ////
            ////////////////////////////////////////////////////////

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            tolerance = tolerance * tolerance;

            int left = clip.X;
            int top = clip.Y;
            int width = clip.Width;
            int height = clip.Height;
            int length = width * height;
            int widthOfData = depthHorizontalResulotion * 2;// two bytes per pixel
            float averageX = (float)(average.X);
            float averageY = (float)(average.Y);
            int topLeftCornerOfResultInDepthData = (left * 2 + top * widthOfData); //2 bytes per pixel

            int ballX = 0, ballY = 0, ballPointsCount = 0;
            var previousRow = new int[width];//Lets Try ushort later!!!!!!

            int j = topLeftCornerOfResultInDepthData - widthOfData; //first previousRow is one above the topLeftCorner
            for (int i = 0; i < width; i++)//fill previousRow
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
                    if (lenghtOfAnormal > tolerance && dept > 800 && lastDept > 800 && previousRow[x] > 800)
                    {
                        ballX += x;
                        ballY += y;
                        ballPointsCount++;
                        ////////////////////////////////////////
                        hightAnormaltiesArr[x + y * width] = 255; ////
                        ////////////////////////////////////////
                    }
                    previousRow[x] = dept;
                    lastDept = dept;
                    /////////////////////////////////////////////////////////////////////////////////
                    int index = x + y * width;                                                  /////
                    deptArr[index] = (byte)((dept - 800) / 2);                                                /////
                    deltaXArr[index] = (byte)Math.Abs(deltaX * visibilytyMultiplier);// + 127);             /////
                    deltaYArr[index] = (byte)Math.Abs(deltaY * visibilytyMultiplier);// + 127);             /////
                    anormalXArr[index] = (byte)Math.Abs(anormalX * visibilytyMultiplier);// + 127);         /////
                    anormalYArr[index] = (byte)Math.Abs(anormalY * visibilytyMultiplier);// + 127);         /////
                    /////////////////////////////////////////////////////////////////////////////////

                }
                j += widthOfData - width * 2; //still 2 bytes per regular pixel
            }

            //System.Diagnostics.Debug.WriteLine("BallPositionFast: " + stopwatch.ElapsedMilliseconds);

            if (ballPointsCount > minHeightAnormalities)
                return new Vector(ballX / ballPointsCount, ballY / ballPointsCount);
            else
                return ImageProcessing.InvalidVector;
        }

        public static byte[] PrettyPicture(byte[] depthData)
        {
            byte[] result = new byte[depthData.Length / 2];

            //byte notValid =  unchecked((byte)(-800 / 2));

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            for (int i = 0, j = 0; j < depthData.Length; j += 2, i++)
            {
                result[i] = (byte)(((depthData[j] | depthData[j + 1] << 8) - 800) / 2);
                //if (result[i] == notValid)
                //    result[i] = 255;
            }

            //System.Diagnostics.Debug.WriteLine("PrettyPicture: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();

            return result;
        }

        //Older Stuff is Below

       //public static void DeltaToAverage(Microsoft.Research.Kinect.Nui.PlanarImage frame, Int32Rect clip, double tolerance, out byte[] resultX, out byte[] resultY, out Vector average, out Vector ballPos, out byte[] ballAllPos)
       // {
       //     //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
       //     int left = clip.X;
       //     int top = clip.Y;
       //     int width = clip.Width;
       //     int height = clip.Height;
       //     int length = width * height;

       //     byte[] depthData = frame.Bits;

       //     int widthOfData = frame.Width * 2; //2 bytes per pixel
       //     int topLeftCornerOfResultInDepthData = (left * 2 + top * widthOfData); //2 bytes per pixel
       //     //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

       //     resultX = new byte[lenght];
       //     resultY = new byte[lenght];
       //     ballAllPos = new byte[lenght];

       //     average = Average(frame.Bits, frame.Width, clip);
       //     byte averageX = (byte)average.X;
       //     byte averageY = (byte)average.Y;

       //     int ballX = 0, ballY = 0, ballCound = 0;

       //     //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
       //     System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
       //     stopwatch.Start();

       //     int lastdept = 0;
       //     int dept = 0;
       //     int j = topLeftCornerOfResultInDepthData;
       //     for (int y = 0; y < height; y++)
       //     {
       //         lastdept = depthData[j - 2] | depthData[j - 1] << 8; //2 bytes are merged to depth
       //         for (int x = 0; x < width; x++)
       //         {
       //             dept = depthData[j++] | depthData[j++] << 8;
       //             int delta = dept - lastdept;
       //             //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       //             int anormaly = delta - averageX;
       //             if (anormaly > tolerance || anormaly < -tolerance)
       //             {
       //                 ballX += x;
       //                 ballY += y;
       //                 ballCound++;
       //             }
       //             resultX[x + y * width] = (byte)(anormaly + 127);
       //             //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
       //             lastdept = dept;
       //         }
       //         j += widthOfData - width * 2;//2 bytes per pixel
       //     }

       //     System.Diagnostics.Debug.WriteLine("DeltaToAverage X:" + stopwatch.ElapsedMilliseconds);
       //     stopwatch.Restart();

       //     j = topLeftCornerOfResultInDepthData;
       //     for (int x = 0; x < width; x++)
       //     {
       //         lastdept = depthData[j - widthOfData] | depthData[j - widthOfData + 1] << 8;
       //         for (int y = 0; y < height; y++)
       //         {
       //             dept = depthData[j] | depthData[j + 1] << 8;
       //             j += widthOfData;
       //             int delta = dept - lastdept;
       //             //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       //             int anormaly = delta - averageY;
       //              if (anormaly > tolerance || anormaly < - tolerance)
       //              {
       //                  ballX += x;
       //                  ballY += y;
       //                  ballCound++;
       //              }
       //              resultY[x + y * width] = (byte)(anormaly + 127);
       //              //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
       //              lastdept = dept;
       //         }
       //         j = top * widthOfData + (x + left) * 2; //2 bytes per pixel
       //     }

       //     System.Diagnostics.Debug.WriteLine("DeltaToAverage Y:" + stopwatch.ElapsedMilliseconds);
       //     stopwatch.Restart();
       //     //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^


       //     ballX = 0;
       //     ballY = 0;
       //     ballCound = 0;
       //     for (int i = 0; i < lenght; i++)
       //     {
       //         int x = resultX[i] - 127;
       //         int y = resultY[i] - 127;

       //         double l = Math.Sqrt( x * x + y * y );

       //         if (l > tolerance)
       //         {
       //             ballX += i % width;
       //             ballY += i / width;
       //             ballCound++;

       //             ballAllPos[i] = 255; 
       //         }
       //     }

       //     System.Diagnostics.Debug.WriteLine("DeltaToAverage Lenght:" + stopwatch.ElapsedMilliseconds);

       //     if (ballCound == 0)
       //         ballPos = new Vector();
       //     else
       //         ballPos = new Vector(ballX / ballCound, ballY / ballCound);
       // }

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
