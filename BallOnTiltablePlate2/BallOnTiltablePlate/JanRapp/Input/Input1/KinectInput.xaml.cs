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
using Coding4Fun.Kinect.Wpf;
using BallOnTiltablePlate.JanRapp.Utilities;
using BallOnTiltablePlate.JanRapp.Input;

namespace BallOnTiltablePlate.JanRapp.Input1
{
    /// <summary>
    /// Interaction logic for KinectInput.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan", "Rapp", "KinectInput", "1.0")]
    public partial class KinectInput : UserControl, IBallInput
    {
        Kinect.Runtime kinect;
        Task<ImageProcessing.Output> computaionTask;
        Dictionary<string, DisplayDescribtion> displays = 
            new Dictionary<string, DisplayDescribtion>();

        readonly int KinectInputImageWidth;
        readonly int KinectInputImageHeight;
        readonly Vector KinectInputImageSize;
        public KinectInput()
        {
            KinectInputImageWidth = 640;
            KinectInputImageHeight = 480;
            KinectInputImageSize = new Vector(KinectInputImageWidth, KinectInputImageHeight);

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
                Vector center = CenterSelector.GetValueFromSize(KinectInputImageSize);

                DepthAtCenterDisplay.Text = e.ImageFrame.GetDistance((int)center.X, (int)center.Y).ToString();

                var rotationX = new Quaternion(new Vector3D(1, 0, 0), ProjectionAdjustRotaion.Value.X);
                var rotationY = new Quaternion(new Vector3D(0, 1, 0), ProjectionAdjustRotaion.Value.Y);
                var rotationZ = new Quaternion(new Vector3D(0, 0, 1), ProjectionAdjustRotaion.Value.Z);
                var rotation = new QuaternionRotation3D(rotationZ * rotationY * rotationX);
                
                Dictionary<string, object> state = new Dictionary<string, object>()
                {
                    {"twoByteDepthBits", e.ImageFrame.Image.Bits},
                    {"depthHorizontalResulotion", 640},
                    {"centerPosition", CenterSelector.GetValueFromSize(KinectInputImageSize)},
                    {"centerDepth", (int)CenterDepthBox.Value},
                    {"cameraConstant", CameraConstantBox.Value},
                    {"clip", ConvertUtil.ToIntRect(ClipSelector.GetValueFromSize(KinectInputImageSize))},
                    {"tolerance", (float)ToleranceDoubelBox.Value},
                    {"minHightAnormalities", (int)MinHeightAnormalities.Value},
                    {"sizeAtZeroTilt", OneSizeSelector.GetValueFromSize(KinectInputImageSize)},
                    {"projectionAdjustRotation", rotation},
                    {"projectionAdjustScale", ProjectionAdjustScale.Value},
                    {"projectionAdjustTranslation", ProjectionAdjustTransalation.Value},
                    {"angleFactor", AngleFactor.Value},
                    {"axesSeperatly", AxesSeperalty.IsChecked ?? false},
                    {"projectionInverted", ProjectionInverted.IsChecked ?? false},
                };

                computaionTask = new Task<ImageProcessing.Output>(DoMainComputaionAsync, state);
                computaionTask.ContinueWith(DisplayComputation, TaskScheduler.FromCurrentSynchronizationContext());

                computaionTask.Start();
            }
            else
                System.Diagnostics.Debug.WriteLine("Frame Skipped!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

            System.Diagnostics.Debug.WriteLine("kinect_DepthFrameReady: " + stopwatch.ElapsedMilliseconds);
        }

        ImageProcessing.Output DoMainComputaionAsync(object state)
        {
            var input = (Dictionary<string, object>)state;

            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var ballPosition = ImageProcessing.BallPositionFast(input, displays);

            System.Diagnostics.Debug.WriteLine("Time for ball pos" + stopWatch.ElapsedMilliseconds);

            return new ImageProcessing.Output(displays.ToArray(), ballPosition, PrettyPictureOfDepthData.PrettyPicture((byte[])input["twoByteDepthBits"]));
        }

        void DisplayComputation(Task<ImageProcessing.Output> task)
        {
            var output = task.Result;

            Vector centerPos = CenterSelector.ValueCoordinates;
            double widthThatIsPlateSizePixel = (OneSizeSelector.GetValueFromSize(KinectInputImageSize).Width / 2);
            double withThatIsPlateSizeMeter = GlobalSettings.Instance.HalfPlateSize;

            BallSelector.ValueCoordinates = output.ballPosition
                * widthThatIsPlateSizePixel
                / withThatIsPlateSizeMeter
                + centerPos;

            if (output.prettyPicture != null)
            {
                InputImage.Source = CreateMyStandartBitmapSource(output.prettyPicture, KinectInputImageWidth, KinectInputImageHeight);
                OutputImage.Source = CreateMyStandartBitmapSource(output.prettyPicture, KinectInputImageWidth, KinectInputImageHeight);
            }

            foreach (var item in output.displays)
            {
                if(!item.Value.Display.IsValueCreated)
                {
                    if (item.Key.StartsWith("OutputImageSelector"))
                        OutputImagePanel.Children.Add(item.Value.Display.Value);
                    else
                        MainPanel.Children.Add(item.Value.Display.Value);
                }

                item.Value.ToDisplay(item.Value.Display.Value, item.Value.Data);
            }

            Vector ballPos = output.ballPosition;
            ballPos.Y = -ballPos.Y; //Y is upsidedown in regualar Math

            BallPositionDisplay.Text = ballPos.ToString();

            SendData(ballPos);
            ((BallOnTiltablePlate.JanRapp.MainApp.MainWindow)Application.Current.MainWindow).JugglerTimer();
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

        public void OneSizeBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Rect> e)
        {
            CenterPositionBox.Value = (Vector)e.NewValue.TopLeft + new Vector(e.NewValue.Width, e.NewValue.Height) / 2;
        }
    }
}
