﻿using System;
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
using Kinect = Microsoft.Research.Kinect.Nui;
using System.Threading.Tasks;
using System.Threading;
using Coding4Fun.Kinect.Wpf;

namespace BallOnTiltablePlate.JanRapp.Input
{
    /// <summary>
    /// Interaction logic for KinectInput.xaml
    /// </summary>
    [BallOnPlateItemInfo("Jan", "Rapp", "KinectInput", "1.0")]
    public partial class KinectInput : UserControl, IBallInput
    {
        Kinect.Runtime kinect;
        Task<ImageProcessing.Output> computaionTask;
        Dictionary<string, ImageProcessing.DisplayDescribtion> displays = 
            new Dictionary<string,ImageProcessing.DisplayDescribtion>();

        public KinectInput()
        {
            InitializeComponent();

            if (Kinect.Runtime.Kinects.Count == 0)
            {
                MessageBox.Show("Kinect not (properly) connected. Restart App for now.");
                this.IsEnabled = false;
            }
            else
            {
                kinect = Kinect.Runtime.Kinects[0];
                kinect.DepthFrameReady += kinect_DepthFrameReady;
            }
        }

        void kinect_DepthFrameReady(object sender, Kinect.ImageFrameReadyEventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            if (computaionTask == null || computaionTask.IsCompleted)
            {
                Vector center = CenterSelector.GetValueFromSize(new Vector(640, 480));

                DepthAtCenterDisplay.Text = e.ImageFrame.GetDistance((int)center.X, (int)center.Y).ToString();
                
                var state = Tuple.Create(
                    e.ImageFrame.Image.Bits,
                    640,
                    CenterSelector.Value,
                    (int)CenterDepthBox.Value,
                    ToIntRect(ClipSelector.GetValueFromSize(new Vector(640,480))),
                    (float)ToleranceDoubelBox.Value,
                    (int)MinHeightAnormalities.Value
                    );

                computaionTask = new Task<ImageProcessing.Output>(DoMainComputaionAsync, state);
                computaionTask.ContinueWith(DisplayComputation, TaskScheduler.FromCurrentSynchronizationContext());

                computaionTask.Start();
            }

            System.Diagnostics.Debug.WriteLine("kinect_DepthFrameReady: " + stopwatch.ElapsedMilliseconds);
        }

        ImageProcessing.Output DoMainComputaionAsync(object state)
        {
            var input = (Tuple<byte[], int, Vector, int, Int32Rect, float, int>)state;
            byte[] twoByteDepthBits       = input.Item1;
            int depthHorizontalResulotion = input.Item2;
            Vector centerPosition         = input.Item3;
            int centerDepth               = input.Item4;
            Int32Rect clip                = input.Item5;
            float tolerance               = input.Item6;
            int minHightAnormalities      = input.Item7;


            var ballPosition = ImageProcessing.BallPositionFast(twoByteDepthBits, depthHorizontalResulotion,
                centerPosition, centerDepth,
                tolerance, clip, minHightAnormalities, 64,
                displays);

            return new ImageProcessing.Output(displays, ballPosition, PrettyPictureOfDepthData.PrettyPicture(twoByteDepthBits));
        }

        void DisplayComputation(Task<ImageProcessing.Output> task)
        {
            var output = task.Result;

            BallSelector.ValueCoordinates = output.ballPosition
                + (Vector)ClipSelector.ValueCoordinates.TopLeft;

            OverAllImage.Source = CreateMyStandartBitmapSource(output.prettyPicture, 640, 480);

            Vector ballPos = output.ballPosition
                + (Vector)ClipSelector.ValueCoordinates.TopLeft
                - CenterSelector.ValueCoordinates;
            ballPos.Y = -ballPos.Y; //Y is upsidedown in regualar Math

            BallPositionDisplay.Text = ballPos.ToString();
        }

        Int32Rect ToIntRect(Rect rect)
        {
            return new Int32Rect((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
        }

        BitmapSource CreateMyStandartBitmapSource(byte[] data, int width, int height)
        {
            return BitmapSource.Create(width, height, 96.0, 96.0, PixelFormats.Gray8, null, data, width);
        }

        public void Start()
        {
            kinect.Initialize(Kinect.RuntimeOptions.UseDepth);
            kinect.DepthStream.Open(Kinect.ImageStreamType.Depth, 4, Kinect.ImageResolution.Resolution640x480, Kinect.ImageType.Depth);
        }

        public void Stop()
        {
            kinect.Uninitialize();
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            Point pos = e.GetPosition(this);


            base.OnMouseMove(e);
        }

        #region Base
        public System.Windows.FrameworkElement SettingsUI { get { return this; } }
        #endregion

        #region Event
        public event EventHandler<BallInputEventArgs> DataRecived;

        private void SendData(Vector vec)
        {
            var args = new BallInputEventArgs() { BallPosition = vec };

            if (DataRecived != null)
                DataRecived(this, args);
        }

        //public event EventHandler<BallInputEventArgs3D> DataRecived;

        //EventHandler<BallInputEventArgs> DataRecived2D;
        //event EventHandler<BallInputEventArgs> IBallInput.DataRecived
        //{
        //    add { DataRecived2D += value; }
        //    remove { DataRecived2D -= value; }
        //}

        //private void SendData(Vector3D vec)
        //{
            //var args = new BallInputEventArgs3D() { BallPosition3D = vec };
            //args.BallPosition = new System.Windows.Vector(vec.X, vec.Y);

            //if (DataRecived != null)
            //    DataRecived(this, args);
            //if (DataRecived2D != null)
            //    DataRecived2D(this, args);
        //}
        #endregion

        public void Image_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                Image image = (Image)sender;
                Point pos = e.GetPosition(image);

                CroppedBitmap cb = new CroppedBitmap((BitmapSource)image.Source,
                    new Int32Rect((int)pos.X, (int)pos.Y, 1, 1));
                byte[] pixels = new byte[4];
                cb.CopyPixels(pixels, 4, 0);

                HoveringColorDisplay.Text = pixels[0].ToString(); //should Be Green. I didn't use the beginning or end, since there is alpha somewhere;
            }
            catch (Exception)
            { 
                //well happens
            } 
        }
    }
}