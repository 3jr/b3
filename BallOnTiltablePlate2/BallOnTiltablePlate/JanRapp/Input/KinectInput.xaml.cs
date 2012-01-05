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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;

namespace BallOnTiltablePlate.Input
{
    /// <summary>
    /// Interaction logic for KinectInput.xaml
    /// </summary>
    [BallOnPlateItemInfo("Jan", "Rapp", "KinectInput", "0.1")]
    public partial class KinectInput : UserControl, IBallInput3D
    {
        Runtime kinect;
        ImageProcessing processor;

        public KinectInput()
        {
            InitializeComponent();

            kinect = Runtime.Kinects[0];
            kinect.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(kinect_DepthFrameReady);
            processor = new ImageProcessing(1, 1, 640, 480);
       }

        void kinect_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            
            //ClipSelector.Image.Source = e.ImageFrame.ToBitmapSource();
            byte[] bits = e.ImageFrame.Image.Bits;
            for (int i = 1; i < 614400 - 1; i+=2)
			{
                bits[i] = (byte)(bits[i] << 4);
			}// maybe faster and more accurate with pointers

            ClipSelector.Image.Source = BitmapSource.Create(640, 480, 96.0, 96.0, PixelFormats.Gray16 , null, bits, 640 * 2);
                                      //BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgr32, null, ColoredBytes, image.Width * PixelFormats.Bgr32.BitsPerPixel / 8);

            System.Diagnostics.Debug.WriteLine("ImageFrame.ToBitmapSource() took:" + stopwatch.ElapsedMilliseconds);

            System.Diagnostics.Debug.WriteLine(processor.Average(e.ImageFrame.Image));
        } 

        public void Start()
        {
            kinect.Initialize(RuntimeOptions.UseDepth);
            kinect.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution640x480, ImageType.Depth);
        }

        public void Stop()
        {
            kinect.Uninitialize();
        }

        private void SetNewClip_Click(object sender, RoutedEventArgs e)
        {
            processor.ChangeClip((int)ClipSelector.LeftX, (int)ClipSelector.TopY, (int)ClipSelector.RightX, (int)ClipSelector.BottomY);
        }

        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        #region Event
        public event EventHandler<BallInputEventArgs3D> DataRecived;

        EventHandler<BallInputEventArgs> DataRecived2D;
        event EventHandler<BallInputEventArgs> IBallInput.DataRecived
        {
            add { DataRecived2D += value; }
            remove { DataRecived2D -= value; }
        }

        private void SendData(Vector3D vec)
        {
            var args = new BallInputEventArgs3D() { BallPosition3D = vec };
            args.BallPosition = new System.Windows.Vector(vec.X, vec.Y);

            if (DataRecived != null)
                DataRecived(this, args);
            if (DataRecived2D != null)
                DataRecived2D(this, args);
        }
        #endregion
    }
}
