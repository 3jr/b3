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
using Kinect = Microsoft.Research.Kinect.Nui;
using System.Threading.Tasks;
using System.Threading;
using Coding4Fun.Kinect.Wpf;
using BallOnTiltablePlate.JanRapp.Utilities;

namespace BallOnTiltablePlate.JanRapp.Input2
{
    /// <summary>
    /// Interaction logic for KinectInput2.xaml
    /// </summary>
    public partial class KinectInput2 : UserControl, IBallInput
    {
        Kinect.Runtime kinect;
        Task<Tuple<Vector, byte[]>> computaionTask;
        Dictionary<string, DisplayDescribtion> displays =
            new Dictionary<string, DisplayDescribtion>();

        readonly int KinectInputImageWidth;
        readonly int KinectInputImageHeight;
        readonly Vector KinectInputImageSize;

        public KinectInput2()
        {
            KinectInputImageWidth = 640;
            KinectInputImageHeight = 480;
            KinectInputImageSize = new Vector(KinectInputImageWidth, KinectInputImageHeight);

            InitializeComponent();
        }

        public void Start()
        {
            if (Kinect.Runtime.Kinects.Count == 0)
            {
                MessageBox.Show("Kinect not (properly) connected.");
                this.IsEnabled = false;
            }
            else
            {
                kinect = Kinect.Runtime.Kinects[0];
                kinect.DepthFrameReady += kinect_DepthFrameReady;
                kinect.Initialize(Kinect.RuntimeOptions.UseDepth);
                kinect.DepthStream.Open(Kinect.ImageStreamType.Depth, 4, Kinect.ImageResolution.Resolution640x480, Kinect.ImageType.Depth);
            }
        }

        public void Stop()
        {
            if (kinect != null)
            {
                kinect.DepthFrameReady -= kinect_DepthFrameReady;
                kinect.Uninitialize();
            }

            kinect = null;
        }

        private void kinect_DepthFrameReady(object sender, Kinect.ImageFrameReadyEventArgs e)
        {
            if (computaionTask == null || computaionTask.IsCompleted)
            {
                Vector center = CenterSelector.GetValueFromSize(KinectInputImageSize);

                DeapAtCenterDisplay.Text = string.Format("Death at Center: {0}", e.ImageFrame.GetDistance((int)center.X, (int)center.Y).ToString());


                Dictionary<string, object> state = new Dictionary<string, object>()
                {
                    {"twoByteDepthBits", e.ImageFrame.Image.Bits},
                    {"depthHorizontalResulotion", 640},
                    {"clip", ConvertUtil.ToIntRect(ClipSelector.GetValueFromSize(KinectInputImageSize))},
                };


                computaionTask = new Task<Tuple<Vector, byte[]>>(DoMainComputaionAsync, state);
                computaionTask.ContinueWith(DisplayComputation, TaskScheduler.FromCurrentSynchronizationContext());

                computaionTask.Start();
            }

        }

        Tuple<Vector, byte[]> DoMainComputaionAsync(object state)
        {
            var input = (Dictionary<string, object>)state;

            var ballPosition = ImageProcessing2.BallPositionFast(input, displays);

            return new Tuple<Vector, byte[]>(ballPosition, Input.PrettyPictureOfDepthData.PrettyPicture((byte[])input["twoByteDepthBits"]));
        }

        void DisplayComputation(Task<Tuple<Vector, byte[]>> task)
        {
            Vector ballPosition = task.Result.Item1;
            byte[] prettyPicture = task.Result.Item2;



            BallSelector.ValueCoordinates = ballPosition
                + (Vector)ClipSelector.ValueCoordinates.TopLeft;

            InputImage.Source = CreateMyStandartBitmapSource(prettyPicture, KinectInputImageWidth, KinectInputImageHeight);
            OutputImage.Source = CreateMyStandartBitmapSource(prettyPicture, KinectInputImageWidth, KinectInputImageHeight);

            Vector ballPos = ballPosition
                + (Vector)ClipSelector.ValueCoordinates.TopLeft
                - CenterSelector.ValueCoordinates;
            ballPos.Y = -ballPos.Y; //Y is upsidedown in regualar Math

            foreach (var item in displays)
            {
                if (!item.Value.Display.IsValueCreated)
                {
                    if (item.Key.StartsWith("OutputImageSelector"))
                        OutputImagePanel.Children.Add(item.Value.Display.Value);
                    else
                        MainPanel.Children.Add(item.Value.Display.Value);
                }

                item.Value.ToDisplay(item.Value.Display.Value, item.Value.Data);
            }

            BallPositionDisplay.Text = string.Format("Ball Position: {0:F3}, {1:F3}", ballPos.X, ballPos.Y);
        }

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

                HoveringColorDisplay.Text = string.Format("Hovering Color: {0}", pixels[0].ToString()); //should Be Green. I didn't use the beginning or end, since there is alpha somewhere;
            }
            catch (Exception)
            {
                //well happens
            }
        }

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

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        BitmapSource CreateMyStandartBitmapSource(byte[] data, int width, int height)
        {
            return BitmapSource.Create(width, height, 96.0, 96.0, PixelFormats.Gray8, null, data, width);
        }
    }
}
