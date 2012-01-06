using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BallOnTiltablePlate.JanRapp.Input
{
    static class BallPosition
    {
        //Most Code is the same as next Method !!!UPDATE!!!
        static Vector BallPositionFast(Microsoft.Research.Kinect.Nui.PlanarImage frame, Vector average, float tolerance, Int32Rect clip)
        {
            int left = clip.X;
            int top = clip.Y;
            int width = clip.Width;
            int height = clip.Height;
            int length = width * height;
            byte[] depthData = frame.Bits;
            if (frame.BytesPerPixel != 2) throw new ArgumentException("frame.BytesPerPixel must be 2");
            int widthOfData = frame.Width * 2;// two bytes per pixel
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

        //two changes and differend signatur to Mehtod above
        static Vector BallPositionFast(Microsoft.Research.Kinect.Nui.PlanarImage frame, Vector average, float tolerance, Int32Rect clip, out byte[] ballPoints)
        {
            /////////////////////////////////////////////////////
            ballPoints = new byte[clip.Width * clip.Height]; ////
            /////////////////////////////////////////////////////

            int left = clip.X;
            int top = clip.Y;
            int width = clip.Width;
            int height = clip.Height;
            int length = width * height;
            byte[] depthData = frame.Bits;
            if (frame.BytesPerPixel != 2) throw new ArgumentException("frame.BytesPerPixel must be 2");
            int widthOfData = frame.Width * 2;// two bytes per pixel
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
                    if (lenghtOfAnormal > tolerance)
                    {
                        ballX += x;
                        ballY += y;
                        ballPointsCount++;
                        /////////////////////////////////////
                        ballPoints[x + y * width] = 255; ////
                        /////////////////////////////////////
                    }
                }
                j += widthOfData - width * 2; //still 2 bytes per regular pixel
            }

            return new Vector(ballX / ballPointsCount, ballY / ballPointsCount);
        }

        //two changes and differend signatur to Mehtod above
        static Vector BallPositionFast(Microsoft.Research.Kinect.Nui.PlanarImage frame, Vector average, float tolerance, Int32Rect clip
            , out byte[] deptArr, out byte[] deltaXArr, out byte[] deltaYArr, out byte[] anormalXArr, out byte[] anormalYArr, out byte[] ballPointsArr, int visibilytyMultiplier)
        {
            ////////////////////////////////////////////////////////
            ballPointsArr = new byte[clip.Width * clip.Height]; ////
                                                                ////
            deptArr     = new byte[clip.Width * clip.Height];   ////
            deltaXArr   = new byte[clip.Width * clip.Height];   ////
            deltaYArr   = new byte[clip.Width * clip.Height];   ////
            anormalXArr = new byte[clip.Width * clip.Height];   ////
            anormalYArr = new byte[clip.Width * clip.Height];   ////
            ////////////////////////////////////////////////////////

            int left = clip.X;
            int top = clip.Y;
            int width = clip.Width;
            int height = clip.Height;
            int length = width * height;
            byte[] depthData = frame.Bits;
            if (frame.BytesPerPixel != 2) throw new ArgumentException("frame.BytesPerPixel must be 2");
            int widthOfData = frame.Width * 2;// two bytes per pixel
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
                    if (lenghtOfAnormal > tolerance)
                    {
                        ballX += x;
                        ballY += y;
                        ballPointsCount++;
                        ////////////////////////////////////////
                        ballPointsArr[x + y * width] = 255; ////
                        ////////////////////////////////////////
                    }
                    /////////////////////////////////////////////////////////////////////////////////
                    int index = x + y * width;                                                  /////
                    deptArr[index] = (byte)dept;                                                /////
                    deltaXArr[index] = (byte)(deltaX * visibilytyMultiplier + 127);             /////
                    deltaYArr[index] = (byte)(deltaY * visibilytyMultiplier + 127);             /////
                    anormalXArr[index] = (byte)(anormalX * visibilytyMultiplier + 127);         /////
                    anormalYArr[index] = (byte)(anormalY * visibilytyMultiplier + 127);         /////
                    /////////////////////////////////////////////////////////////////////////////////

                }
                j += widthOfData - width * 2; //still 2 bytes per regular pixel
            }

            return new Vector(ballX / ballPointsCount, ballY / ballPointsCount);
        }
    }
}
