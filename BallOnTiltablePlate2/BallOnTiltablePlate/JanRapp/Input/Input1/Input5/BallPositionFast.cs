using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BallOnTiltablePlate.JanRapp.Input0_5
{
    static class BallPosition
    {
        //Most Code is the same as next Method !!!UPDATE!!!
        public static Vector BallPositionFast(byte[] depthData, int depthHorizontalResulotion, Vector average, float tolerance, Int32Rect clip, int minHeightAnormalities
            )
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

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
                    previousRow[x] = dept;
                    if (lenghtOfAnormal > tolerance)
                    {
                        ballX += x;
                        ballY += y;
                        ballPointsCount++;
                    }
                }
                j += widthOfData - width * 2; //still 2 bytes per regular pixel
            }

            System.Diagnostics.Debug.WriteLine("BallPositionFast: " + stopwatch.ElapsedMilliseconds);


            if (ballPointsCount > minHeightAnormalities)
                return new Vector(ballX / ballPointsCount, ballY / ballPointsCount);
            else
                return ImageProcessing.InvalidVector;
        }

        //two changes and differend signatur to Mehtod above
        public static Vector BallPositionFast(byte[] depthData, int depthHorizontalResulotion, Vector average, float tolerance, Int32Rect clip, int minHeightAnormalities,
            out byte[] hightAnormalties)
        {
            /////////////////////////////////////////////////////
            hightAnormalties = new byte[clip.Width * clip.Height]; ////
            /////////////////////////////////////////////////////

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

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
                    previousRow[x] = dept;
                    if (lenghtOfAnormal > tolerance)
                    {
                        ballX += x;
                        ballY += y;
                        ballPointsCount++;
                        /////////////////////////////////////
                        hightAnormalties[x + y * width] = 255; ////
                        /////////////////////////////////////
                    }
                }
                j += widthOfData - width * 2; //still 2 bytes per regular pixel
            }

            System.Diagnostics.Debug.WriteLine("BallPositionFast: " + stopwatch.ElapsedMilliseconds);

            if (ballPointsCount > minHeightAnormalities)
                return new Vector(ballX / ballPointsCount, ballY / ballPointsCount);
            else
                return ImageProcessing.InvalidVector;
        }

        //two changes and differend signatur to Mehtod above
        public static Vector BallPositionFast(byte[] depthData, int depthHorizontalResulotion, Vector average, float tolerance, Int32Rect clip, int minHeightAnormalities,
            out byte[] deptArr, out byte[] deltaXArr, out byte[] deltaYArr, out byte[] anormalXArr, out byte[] anormalYArr, out byte[] hightAnormaltiesArr, int visibilytyMultiplier)
        {
            ////////////////////////////////////////////////////////
            hightAnormaltiesArr = new byte[clip.Width * clip.Height]; ////
                                                                ////
            deptArr     = new byte[clip.Width * clip.Height];   ////
            deltaXArr   = new byte[clip.Width * clip.Height];   ////
            deltaYArr   = new byte[clip.Width * clip.Height];   ////
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
                    previousRow[x] = dept;
                    lastDept = dept;
                    if (lenghtOfAnormal > tolerance)
                    {
                        ballX += x;
                        ballY += y;
                        ballPointsCount++;
                        ////////////////////////////////////////
                        hightAnormaltiesArr[x + y * width] = 255; ////
                        ////////////////////////////////////////
                    }
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

            System.Diagnostics.Debug.WriteLine("BallPositionFast: " + stopwatch.ElapsedMilliseconds);

            if (ballPointsCount > minHeightAnormalities)
                return new Vector(ballX / ballPointsCount, ballY / ballPointsCount);
            else
                return ImageProcessing.InvalidVector;
        }
    }
}
