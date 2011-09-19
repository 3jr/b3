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

        OpenCVManager manager = new OpenCVManager();

        int xres = 320;
        int yres = 240;

        Forms.PictureBox kinectBox = new Forms.PictureBox();
        Draw.Bitmap image;

        public void Init(object sender, RoutedEventArgs e)
        {
            //UseDepthAndPlayerIndex and UseSkeletalTracking
            nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking);

            //register for event
            nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);

            //DepthAndPlayerIndex ImageType
            nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240,
                ImageType.DepthAndPlayerIndex);

            angleSlider.Value = ((double)(nui.NuiCamera.ElevationAngle - Camera.ElevationMinimum) * 10.0) / (double)(Camera.ElevationMaximum - Camera.ElevationMinimum);

            #region InitBitmap
            
            kinectBox = new Forms.PictureBox();
            image = new Draw.Bitmap(xres, yres);
            kinectBox.Dock = Forms.DockStyle.Fill;
            kinectBox.SizeMode = Forms.PictureBoxSizeMode.StretchImage;
            kinectImage.Child = kinectBox;
            #endregion
        }

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            byte[] test = GenerateColoredBytes(e.ImageFrame);

            image = KinectHelper.BitmapExtensions.ToBitmap(test, xres, yres);

            manager.GetPoints(image);

            kinectBox.Image = image;
        }

        private byte[] GenerateColoredBytes(ImageFrame imageFrame)
        {
            int height = yres;
            int width = xres;

            //Depth data for each pixel
            Byte[] depthData = imageFrame.Image.Bits;

            //colorFrame contains color information for all pixels in image
            //Height x Width x 4 (Red, Green, Blue, empty byte)
            Byte[] colorFrame = new byte[imageFrame.Image.Height * imageFrame.Image.Width * 4];

            int[,] depthMap = new int[width, height];

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

                    //var distance = GetDistance(depthData[depthIndex], depthData[depthIndex + 1]);
                    var distance = GetDistanceWithPlayerIndex(depthData[depthIndex], depthData[depthIndex + 1]);

                    depthMap[x, y] = (byte)((distance / (10 * minSlider.Value)));
                    byte color = 0;
                    #region To byte[] clor
                    color = (byte)(depthMap[x, y]);// - ((depthMap[x - 1, y] + depthMap[x, y - 1] + depthMap[x, y + 1] + depthMap[x + 1, y]) / 4));

                    colorFrame[index + RedIndex] = color;
                    colorFrame[index + GreenIndex] = color;
                    colorFrame[index + BlueIndex] = color;
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
