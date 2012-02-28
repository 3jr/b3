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
using Kinect = Microsoft.Research.Kinect.Nui;
using System.Threading.Tasks;
using System.Threading;

namespace BallOnTiltablePlate.JanRapp.Input05
{
    /// <summary>
    /// Interaction logic for KinectInput.xaml
        /// </summary>
    [BallOnPlateItemInfo("Jan", "Rapp", "KinectInput", "0.5")]
    public partial class KinectInput : UserControl, IBallInput
    {
        Kinect.Runtime kinect;
        Task<ImageProcessing.Output> computaionTask;

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
                kinect.DepthFrameReady += new EventHandler<Kinect.ImageFrameReadyEventArgs>(kinect_DepthFrameReady);
            }
        }

        void kinect_DepthFrameReady(object sender, Kinect.ImageFrameReadyEventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            if (computaionTask == null || computaionTask.IsCompleted)
            {
                
                var state = new ImageProcessing.Input(e.ImageFrame.Image.Bits, 640,
                    ToIntRect(ClipSelector.GetValueFromSize(new Vector(640,480))), (float)ToleranceDoubelBox.Value,
                    ImageProcessing.Requests.None, (int)MinHeightAnormalities.Value);
                computaionTask = new Task<ImageProcessing.Output>(DoMainComputaionAsync, state);
                computaionTask.ContinueWith(DisplayComputation, TaskScheduler.FromCurrentSynchronizationContext());

                computaionTask.Start();
            }

            //System.Diagnostics.Debug.WriteLine("kinect_DepthFrameReady: " + stopwatch.ElapsedMilliseconds);
        }

        ImageProcessing.Output DoMainComputaionAsync(object state)
        {
            ImageProcessing.Input input = (ImageProcessing.Input)state;

            byte[] deptArr;
            byte[] deltaX;
            byte[] deltaY;
            byte[] anormalX;
            byte[] anormalY;
            byte[] hightAnormalties;

            var average = ImageProcessing.Average(input.twoByteDepthBits, input.depthHorizontalResulotion, input.clip);

            var ballPosition = ImageProcessing.BallPositionFast(input.twoByteDepthBits, input.depthHorizontalResulotion, average, input.tolerance, input.clip, input.minHightAnormalities,
                 out deptArr, out deltaX, out deltaY, out anormalX, out anormalY, out hightAnormalties, 64);

            return new ImageProcessing.Output(ImageProcessing.PrettyPicture(input.twoByteDepthBits), deptArr, deltaX, deltaY, anormalX, anormalY, hightAnormalties, ballPosition, average, input.clip);
        }

        void DisplayComputation(Task<ImageProcessing.Output> task)
        {
            var output = task.Result;

            if(!double.IsNaN(output.ballPosition.X))
                SendData(output.ballPosition);

            AverageTextBox.Text = output.averageDelta.ToString();
            BallPositionTextBox.Text = output.ballPosition.ToString();
            ClipTextBox.Text = output.clip.ToString();

            BallSelector.ValueCoordinates = output.ballPosition + new System.Windows.Vector(output.clip.X, output.clip.Y);
            BallSelector2.SetValueFromSize(output.ballPosition, new Vector(output.clip.Width, output.clip.Height));

            if (output.regualar != null)
                OverAllImage.Source = CreateMyStandartBitmapSource(output.regualar, 640, 480);
            if (output.depth != null)
                DepthImage.Source = CreateMyStandartBitmapSource(output.depth, output.clip.Width, output.clip.Height);
            if (output.deltaX != null)
                DeltaXImage.Source = CreateMyStandartBitmapSource(output.deltaX, output.clip.Width, output.clip.Height);
            if (output.deltaX != null)
                DeltaYImage.Source = CreateMyStandartBitmapSource(output.deltaY, output.clip.Width, output.clip.Height);
            if (output.anormalyX != null)
                AnormalitiesXImage.Source = CreateMyStandartBitmapSource(output.anormalyX, output.clip.Width, output.clip.Height);
            if (output.anormalyY != null)
                AnormalitiesYImage.Source = CreateMyStandartBitmapSource(output.anormalyY, output.clip.Width, output.clip.Height);
            if (output.hightAnormalys != null)
                HeightAnormalitiesImage.Source = CreateMyStandartBitmapSource(output.hightAnormalys, output.clip.Width, output.clip.Height);

            BallSelector.Visibility = System.Windows.Visibility.Collapsed;
            ClipSelector.Visibility = System.Windows.Visibility.Collapsed;
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

                HoveringColorTextBox.Text = pixels[0].ToString(); //should Be Green. I didn't use the beginning or end, since there is alpha somewhere;
            }
            catch (Exception)
            { 
                //well happens
            } 
        }
    }
}
