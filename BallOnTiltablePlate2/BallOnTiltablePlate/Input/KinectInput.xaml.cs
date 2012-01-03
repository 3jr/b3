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
using Coding4Fun.Kinect.WPF;

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
            processor = new ImageProcessing(1, 1, 479, 639);
       }

        void kinect_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            ClipSelector.Image.Source = e.ImageFrame
            e.ImageFrame.Image
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
            args.BallPosition = new Vector(vec.X, vec.Y);

            if (DataRecived != null)
                DataRecived(this, args);
            if (DataRecived2D != null)
                DataRecived2D(this, args);
        }
        #endregion
    }
}
