using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;
using Forms = System.Windows.Forms;
using Draw = System.Drawing;
using KinectHelper =  Coding4Fun.Kinect.WinForm;
using BallOnTiltablePlate.MoritzUehling.Kinect;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;

namespace BallOnTiltablePlate.MoritzUehling.UI
{
    /// <summary>
    /// Interaktionslogik für KinectSettingsWindows.xaml
    /// </summary>
    public partial class KinectSettingsWindows : UserControl
    {
        public KinectSettingsWindows()
        {
            InitializeComponent();
        }
        //Kinect Runtime
        Runtime nui = new Runtime();
        
        int angle = Camera.ElevationMinimum;

        OldImageManager manager;

        int xres = 320;
        int yres = 240;

        Forms.PictureBox kinectBox = new Forms.PictureBox();
        Draw.Bitmap image;

        int[,] depthMap;

        Draw.Point rectPoint;
        public void Init(object sender, RoutedEventArgs e)
        {
            //UseDepthAndPlayerIndex and UseSkeletalTracking
            nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex);

            //register for event
            nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);

            //DepthAndPlayerIndex ImageType
            nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240,
                ImageType.DepthAndPlayerIndex);

            angleSlider.Value = ((double)(nui.NuiCamera.ElevationAngle - Camera.ElevationMinimum) * 10.0) / (double)(Camera.ElevationMaximum - Camera.ElevationMinimum);

            rectPoint = new Draw.Point(0, 0);

            #region InitBitmap
            
            kinectBox = new Forms.PictureBox();
            kinectBox.MouseDown += new Forms.MouseEventHandler(kinectBox_MouseDown);
            image = new Draw.Bitmap(xres, yres);
            kinectBox.Dock = Forms.DockStyle.Fill;
            kinectBox.SizeMode = Forms.PictureBoxSizeMode.StretchImage;
            kinectImage.Child = kinectBox;
            #endregion

            manager = new OldImageManager(xres, yres);

            depthMap = new int[xres, yres];

        }
        void kinectBox_MouseDown(object sender, Forms.MouseEventArgs e)
        {
            rectPoint.X = (int)(e.X * (xres / (float)kinectBox.Width));
            rectPoint.Y = (int)(e.Y * (yres / (float)kinectBox.Height));
        }
        
        
        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            FillImageMap(e.ImageFrame);

            int[,] oldMap = new int[xres,yres];
            #region Rechteck
            Mathematics.LineEquation[] eqs = manager.GetPoints(image, depthMap, rectPoint, (int)(5 * limitSlider.Value));


            image = KinectHelper.BitmapExtensions.ToBitmap(GenerateImage(), xres, yres);

            Draw.Graphics g = Draw.Graphics.FromImage(image);

            if (eqs != null)
            {
                Draw.Point p1 = new Draw.Point(0, (int)eqs[0].c); //m * 0 + c = c
                Draw.Point p2 = new Draw.Point(xres, (int)(eqs[0].m * xres + eqs[0].c));
                g.DrawLine(new Draw.Pen(new Draw.SolidBrush(Draw.Color.Green), 1), p1, p2);
            }
            #endregion


            kinectBox.Image = image;
        }

        private void FillImageMap(ImageFrame imageFrame)
        {
            int height = yres;
            int width = xres;

            //Depth data for each pixel
            Byte[] depthData = imageFrame.Image.Bits;

            var depthIndex = 0;
            for (var y = 0; y < height; y++)
            {
                var heightOffset = y * width;

                for (var x = 0; x < width; x++)
                {

                    depthMap[x, y] = GetDistanceWithPlayerIndex(depthData[depthIndex], depthData[depthIndex + 1]);

                    //jump two bytes at a time
                    depthIndex += 2;
                }
            }
        }

        private byte[] GenerateImage()
        {
            int height = yres;
            int width = xres;


            //colorFrame contains color information for all pixels in image
            //Height x Width x 4 (Red, Green, Blue, empty byte)
            Byte[] colorFrame = new byte[yres * xres * 4];

            //Bgr32  - Blue, Green, Red, empty byte
            //Bgra32 - Blue, Green, Red, transparency
            //You must set transparency for Bgra as .NET defaults a byte to 0 = fully transparent

            //hardcoded locations to Blue, Green, Red (BGR) index positions      
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            var depthIndex = 0;
            for (var y = 0; y < height; y++)
            {
                var heightOffset = y * width;

                for (var x = 0; x < width; x++)
                {
                    var index = ((width - x - 1) + heightOffset) * 4;

                    byte color = 0;
                    #region To byte[] clor
                    if (depthMap[x, y] >= 0)
                    {
                        color = (byte)(depthMap[x, y] / (10 * minSlider.Value));
                        colorFrame[index + RedIndex] = color;
                        colorFrame[index + GreenIndex] = color;
                        colorFrame[index + BlueIndex] = color;
                    }
                    else
                    {
                        colorFrame[index + RedIndex] = 255;
                        colorFrame[index + GreenIndex] = 0;
                        colorFrame[index + BlueIndex] = 0;
                    }
                    #endregion

                    //jump two bytes at a time
                    depthIndex += 2;
                }
            }

            return colorFrame;
        }

        private int GetDistanceWithPlayerIndex(byte firstFrame, byte secondFrame)
        {
            //offset by 3 in first byte to get value after player index
            int distance = (int)(firstFrame >> 3 | secondFrame << 5);
            return distance;
        }
        
        private void minSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            nui.NuiCamera.ElevationAngle = Camera.ElevationMinimum + (int)((angleSlider.Value / 10) * (Camera.ElevationMaximum - Camera.ElevationMinimum));
        }
    }
}