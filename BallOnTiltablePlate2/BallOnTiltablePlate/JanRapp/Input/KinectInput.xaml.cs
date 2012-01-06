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
using System.Threading.Tasks;
using System.Threading;

namespace BallOnTiltablePlate.Input
{
    /// <summary>
    /// Interaction logic for KinectInput.xaml
    /// </summary>
    [BallOnPlateItemInfo("Jan", "Rapp", "KinectInput", "0.1")]
    public partial class KinectInput : UserControl, IBallInput3D
    {
        Runtime kinect;
        Task<ImageProcessing.Output> computaionTask;

        static int ThreadName = 0;

        public void ShowThread(string identifyer)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = ThreadName.ToString();
            ThreadName++;

            System.Diagnostics.Debug.WriteLine(identifyer + Thread.CurrentThread.Name);
        }

        public KinectInput()
        {
            InitializeComponent();

            kinect = Runtime.Kinects[0];
            kinect.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(kinect_DepthFrameReady);

            ShowThread("Constructor");

            //computaionTask = new Task<ImageProcessing.Output>(DoMainComputaionAsync);
            computaionTask.ContinueWith(DisplayComputation, TaskScheduler.FromCurrentSynchronizationContext());

            computaionTask.Start();
        }

        ImageProcessing.Input processorInput;

        void kinect_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            //OverAllImage.Source = BitmapSource.Create(640, 480, 96.0, 96.0, PixelFormats.Gray8, null, ImageProcessing.PrettyPicture(e.ImageFrame.Image.Bits), 640);

            if (computaionTask.IsCompleted)
            {
                
                var state = new ImageProcessing.Input(e.ImageFrame.Image.Bits, 640, ToIntRect(ClipSelector.Value), 0, ImageProcessing.Requests.None);
                computaionTask.Start();
            }

            //byte[] resultX;
            //byte[] resultY;
            //byte[] ballAllPos;
            //System.Windows.Vector average;
            //System.Windows.Vector ballPos;
            //ImageProcessing.DeltaToAverage(e.ImageFrame.Image, ClipSelector.Value, 10, out resultX, out resultY, out average, out ballPos, out ballAllPos);


            //HorizontalImage.Source = BitmapSource.Create((int)ClipSelector.Value.Width, (int)ClipSelector.Value.Height, 96, 96, PixelFormats.Gray8, null, resultX, (int)ClipSelector.Value.Width);
            //VerticalImage.Source = BitmapSource.Create((int)ClipSelector.Value.Width, (int)ClipSelector.Value.Height, 96, 96, PixelFormats.Gray8, null, resultY, (int)ClipSelector.Value.Width);
            //BallImage.Source = BitmapSource.Create((int)ClipSelector.Value.Width, (int)ClipSelector.Value.Height, 96, 96, PixelFormats.Gray8, null, ballAllPos, (int)ClipSelector.Value.Width);

            //statusbarText.Text = string.Format("Color: {0:###} | Ball Position: {1:###.##}, {2:###.##} | Clip: {3} | Average: {4:###.##}, {5:###.##}", 0, ballPos.X, ballPos.Y, ClipSelector.Value.ToString(), average.X, average.Y);

           //OverAllImage.Source = e.ImageFrame.ToBitmapSource();
            byte[] bits = e.ImageFrame.Image.Bits;
            for (int i = 1; i < 614400 - 1; i+=2)
			{
                bits[i] = (byte)(bits[i] << 4);
			}// maybe faster and more accurate with pointers

            OverAllImage.Source = BitmapSource.Create(640, 480, 96.0, 96.0, PixelFormats.Gray16 , null, bits, 640 * 2);
                                      //BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgr32, null, ColoredBytes, image.Width * PixelFormats.Bgr32.BitsPerPixel / 8);

            System.Diagnostics.Debug.WriteLine("ImageFrame.ToBitmapSource() took:" + stopwatch.ElapsedMilliseconds);
        }

        //ImageProcessing.Output DoMainComputaionAsync()
        //{

        //    return null;
        //}

        void DisplayComputation(Task<ImageProcessing.Output> task)
        {
            var output = task.Result;

            if (output.regualar != null)
                CreateMyStandartBitmapSource(output.regualar, 640, 480);
            if(output.deltaX != null)
                CreateMyStandartBitmapSource(output.regualar, 640, 480);


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
            kinect.Initialize(RuntimeOptions.UseDepth);
            kinect.DepthStream.Open(ImageStreamType.Depth, 3, ImageResolution.Resolution640x480, ImageType.Depth);
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

        public byte GetColorAt(int x, int y)
        {
            System.Drawing.Bitmap screenPixel = new System.Drawing.Bitmap(1, 1);
            System.Drawing.Graphics gfx = System.Drawing.Graphics.FromImage((System.Drawing.Image)screenPixel);
            gfx.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(1, 1));
            return screenPixel.GetPixel(0, 0).B;
        }
    }
}
