using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace BallOnTiltablePlate.JanRapp.Input
{
    static class ImageProcessing
    {
        public static Vector InvalidVector = new Vector(double.NaN, double.NaN);

        public struct Output
        {
            public Output(
                Dictionary<string, DisplayDescribtion> displays,
                Vector ballPosition,
                byte[] prettyPicture)
            {
                this.displays = displays;
                this.ballPosition = ballPosition;
                this.prettyPicture = prettyPicture;
            }
            public readonly Dictionary<string, DisplayDescribtion> displays;
            public readonly Vector ballPosition;
            public readonly byte[] prettyPicture;
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

        public static Vector Average(byte[] depthData, int depthHorizontalResulotion, Int32Rect clip,
            Dictionary<string, DisplayDescribtion> displays
            )
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

            System.Diagnostics.Debug.WriteLine("Average over depth took:" + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
            //////////////////////////////////////////////////////////////////////////////////////////

            Vector average = new Vector((double)averageX / lenght, (double)averageY / lenght);

            //if (!displays.ContainsKey("average"))
            //{
            //    displays.Add("average",
            //        new DisplayDescribtion()
            //        {
            //            Display = new TextBlock(),
            //            ToDisplay = (display, data) => display.Text = string.Format("Average: {0:##.##}, {1:##.##}", data.X, data.Y)
            //        });
            //}

            //displays["average"].Data = average;

            return average;
        }

        public static Vector BallPositionFast(byte[] depthData, int depthHorizontalResulotion,
            Vector centerPosition, int centerDepth, 
            float tolerance, Int32Rect clip, int minHeightAnormalities, int visibilityMultiplier,
            Dictionary<string, DisplayDescribtion> displays
            )
        {
            Vector average = ImageProcessing.Average(depthData, depthHorizontalResulotion, clip, displays);

            ////////////////////////////////////////////////////////
            byte[] deptArr = new byte[clip.Width * clip.Height];   ////
            byte[] deltaXArr = new byte[clip.Width * clip.Height];   ////
            byte[] deltaYArr = new byte[clip.Width * clip.Height];   ////
            byte[] anormalXArr = new byte[clip.Width * clip.Height];   ////
            byte[] anormalYArr = new byte[clip.Width * clip.Height];   ////
            byte[] hightAnormaltiesArr = new byte[clip.Width * clip.Height]; ////
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
                    deltaXArr[index] = (byte)Math.Abs(deltaX * visibilityMultiplier);// + 127);             /////
                    deltaYArr[index] = (byte)Math.Abs(deltaY * visibilityMultiplier);// + 127);             /////
                    anormalXArr[index] = (byte)Math.Abs(anormalX * visibilityMultiplier);// + 127);         /////
                    anormalYArr[index] = (byte)Math.Abs(anormalY * visibilityMultiplier);// + 127);         /////
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

        static void CreateOrFillImageDisplay(string name, byte[] imageData, Dictionary<string, DisplayDescribtion> displays, Int32Rect clip)
        {
            if(!displays.ContainsKey(name))
                displays.Add(name, new DisplayDescribtion()
                {
                    Display = new Expander() { Content = new Image() },

                    ToDisplay = (display, data) => display.Content.Source =
                        System.Windows.Media.Imaging.BitmapSource.Create(data.clip.Width, data.clip.Height, 96, 96,
                        System.Windows.Media.PixelFormats.Gray8, null, data.imageData, data.clip.Width)
                });

            displays[name].Data = new { imageData, clip };
        }

        public class DisplayDescribtion
        {
            public FrameworkElement Display { get; set; }
            public Action<dynamic, dynamic> ToDisplay;
            public object Data { get; set; }
       }
    }
}
