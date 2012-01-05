using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace BallOnTiltablePlate.Input
{
    class ImageProcessing
    {
        int xLeft;
        int yTop;
        int xRight;
        int yBottom;
        int widthOfResult;
        int heigthOfResult;
        int lenghtOfResult;


        sbyte[,,] result;

        public ImageProcessing(int xLeft, int yTop, int xRight, int yBottom)
        {
            ChangeClip(xLeft, yTop, xRight, yBottom);
        }

        public void ChangeClip(int xLeft, int yTop, int xRight, int yBottom)
        {
            this.xLeft = xLeft;
            this.yTop = yTop;
            this.xRight = xRight;
            this.yBottom = yBottom;
            this.widthOfResult = xRight - xLeft;
            this.heigthOfResult = yBottom - yTop;
            this.lenghtOfResult = widthOfResult * heigthOfResult;

            this.result = new sbyte[widthOfResult, heigthOfResult, 3];
        }

        public sbyte[, ,] DeltaDepth(PlanarImage frame)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            double averageX;
            double averageY;

            byte[] depthData = frame.Bits;

            int widthOfData = frame.Width;
            int topLeftCornerOfResultInDepthData = (xLeft + yTop * widthOfData) * 2; //2 bytes per pixel

            int lastdept = 0;
            int dept = 0;
            int j = topLeftCornerOfResultInDepthData;
            for (int y = 0; y < heigthOfResult; y++)
            {
                lastdept = depthData[j - 2] | depthData[j - 1] << 8; //2 bytes are merged to depth
                for (int x = 0; x < widthOfResult; x++)
                {
                    dept = depthData[j++] | depthData[j++] << 8;
                    int delta = dept - lastdept;
                    result[x, y, 0] = (sbyte)delta;
                    averageX = delta / lenghtOfResult;
                    lastdept = dept;
                }
                j += widthOfData - widthOfResult * 2;//2 bytes per pixel
            }

            j = topLeftCornerOfResultInDepthData;
            for (int x = 0; x < widthOfResult; x++)
            {
                lastdept = depthData[j - widthOfData] | depthData[j - widthOfData + 1] << 8;
                for (int y = 0; y < heigthOfResult; y++)
                {
                    dept = depthData[j] | depthData[j + 1] << 8;
                    j += widthOfData;
                    int delta = dept - lastdept;
                    result[x, y, 1] = (sbyte)delta;
                    averageY = delta / lenghtOfResult;
                    lastdept = dept;
                }
                j = yTop * widthOfData + x * 2; //2 bytes per pixel
            }

            System.Diagnostics.Debug.WriteLine("Differentiate over depth took:" + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            for (int x = 0; x < widthOfResult; x++)
            {
                for (int y = 0; y < heigthOfResult; y++)
                {
                    result[x, y, 2] = (sbyte)Math.Sqrt(result[x, y, 0] * result[x, y, 0] + result[x, y, 1] * result[x, y, 1]);
                }
            }

            System.Diagnostics.Debug.WriteLine("Vector Length over depth took:" + stopwatch.ElapsedMilliseconds);

            return result;
        }

        public System.Windows.Vector Average(PlanarImage frame)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            long averageX = 0;
            long averageY = 0;

            byte[] depthData = frame.Bits;

            int widthOfData = frame.Width * 2; //2 bytes per pixel
            int topLeftCornerOfResultInDepthData = (xLeft * 2 + yTop * widthOfData); //2 bytes per pixel

            int lastdept = 0;
            int dept = 0;
            int j = topLeftCornerOfResultInDepthData;
            for (int y = 0; y < heigthOfResult; y++)
            {
                lastdept = depthData[j - 2] | depthData[j - 1] << 8; //2 bytes are merged to depth
                for (int x = 0; x < widthOfResult; x++)
                {
                    dept = depthData[j++] | depthData[j++] << 8;
                    int delta = dept - lastdept;
                    averageX += delta;
                    lastdept = dept;
                }
                j += widthOfData - widthOfResult * 2;//2 bytes per pixel
            }

            j = topLeftCornerOfResultInDepthData;
            for (int x = 0; x < widthOfResult; x++)
            {
                lastdept = depthData[j - widthOfData] | depthData[j - widthOfData + 1] << 8;
                for (int y = 0; y < heigthOfResult; y++)
                {
                    dept = depthData[j] | depthData[j + 1] << 8;
                    j += widthOfData;
                    int delta = dept - lastdept;
                    averageY += delta;
                    lastdept = dept;
                }
                j = yTop * widthOfData + (x + xLeft) * 2; //2 bytes per pixel
            }

            System.Diagnostics.Debug.WriteLine("Average over depth took:" + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            return new System.Windows.Vector((double)averageX / lenghtOfResult, (double)averageY / lenghtOfResult);
        }


        public static byte[][] DeltaBytes(byte[] TwoByteDepthRaw, int widthOfData, int xLeft, int yTop, int xRight, int yBottom)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            widthOfData *= 2; //2 bytes per pixel

            int widthOfResult = xRight - xLeft;
            int heigthOfResult = yBottom - yTop;
            byte[,] resultX = new byte[widthOfResult, heigthOfResult];
            byte[,] resultY = new byte[widthOfResult, heigthOfResult];

            System.Diagnostics.Debug.WriteLine("Creating Arrays:" + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();


            int topLeftCornerOfResultInDepthData = (xLeft * 2 + yTop * widthOfData); //2 bytes per pixel

            int lastdept = 0;
            int dept = 0;
            int j = topLeftCornerOfResultInDepthData;
            for (int y = 0; y < heigthOfResult; y++)
            {
                lastdept = TwoByteDepthRaw[j - 2] | TwoByteDepthRaw[j - 1] << 8; //2 bytes are merged to depth
                for (int x = 0; x < widthOfResult; x++)
                {
                    dept = TwoByteDepthRaw[j] | TwoByteDepthRaw[j + 1] << 8;
                    j += 2;
                    resultX[x, y] = (byte)((dept - lastdept) * 1 + 127);
                    lastdept = dept;
                }
                j = topLeftCornerOfResultInDepthData + y * widthOfData;//2 bytes per pixel
            }

            j = topLeftCornerOfResultInDepthData;
            for (int x = 0; x < widthOfResult; x++)
            {
                lastdept = TwoByteDepthRaw[j - widthOfData] | TwoByteDepthRaw[j - widthOfData + 1] << 8;
                for (int y = 0; y < heigthOfResult; y++)
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

            result[0] = new byte[widthOfResult * heigthOfResult];
            result[1] = new byte[widthOfResult * heigthOfResult];

            int i = 0;
            for (int y = 0; y < heigthOfResult; y++)
            {
                for (int x = 0; x < widthOfResult; x++)
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
