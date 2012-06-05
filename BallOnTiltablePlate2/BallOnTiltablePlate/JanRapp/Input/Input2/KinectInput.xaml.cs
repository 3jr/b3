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
using BallOnTiltablePlate.JanRapp.Input1;

namespace BallOnTiltablePlate.JanRapp.Input2
{
    /// <summary>
    /// Interaction logic for KinectInput.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan", "Rapp", "KinectInput", "1.8")]
    public partial class KinectInput : UserControl, IBallInput
    {
        Kinect.Runtime kinect;
        Task<ImageProcessing.Output> computaionTask;
        Dictionary<string, DisplayDescribtion> displays = 
            new Dictionary<string, DisplayDescribtion>();
        int framesSkipt = 0;

        int KinectInputImageWidth;
        int KinectInputImageHeight;
        Vector KinectInputImageSize;
        
        public KinectInput()
        {
            KinectInputImageWidth = 640;
            KinectInputImageHeight = 480;
            KinectInputImageSize = new Vector(KinectInputImageWidth, KinectInputImageHeight);

            InitializeComponent();

            Kinect.Runtime.Kinects.StatusChanged += Kinects_StatusChanged;
            TryInitializeKinectDevice();
        }

        void TryInitializeKinectDevice()
        {
            if (Kinect.Runtime.Kinects.Count == 0 || Kinect.Runtime.Kinects[0].Status != Kinect.KinectStatus.Connected)
            {
                MessageBox.Show("Kinect[0] not (properly) connected. Kinect Status: " + Kinect.Runtime.Kinects[0].Status.ToString());
                this.IsEnabled = false;
            }
            else
            {
                this.IsEnabled = true;
                kinect = Kinect.Runtime.Kinects[0];
                kinect.DepthFrameReady += kinect_DepthFrameReady;
                if (started)
                {
                    kinect.Initialize(Kinect.RuntimeOptions.UseDepth);
                    kinect.DepthStream.Open(Kinect.ImageStreamType.Depth, 4, Kinect.ImageResolution.Resolution640x480, Kinect.ImageType.Depth);
                }
            }
        }

        void Kinects_StatusChanged(object sender, Kinect.StatusChangedEventArgs e)
        {
            try
            {
                if(kinect == null)
                    TryInitializeKinectDevice();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kinect counldn't be initialized. Exception Message: " + ex.Message);
            }
        }

        void ChangeIdealSizeOfUI(Size vec)
        {
            CenterSelector.IdealSize = vec;
            ClipSelector.IdealSize = vec;
            OneSizeSelector.IdealSize = vec;
            BallSelector.IdealSize = vec;
        }

        System.Diagnostics.Stopwatch deltaFrameSw = new System.Diagnostics.Stopwatch();
        void kinect_DepthFrameReady(object sender, Kinect.ImageFrameReadyEventArgs e)
        {
            DeltaFrameDisplay.Text = "Delta Frame: " + deltaFrameSw.Elapsed;
            deltaFrameSw.Restart();
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();


            if (computaionTask == null || computaionTask.IsCompleted)
            {
                Vector center = CenterSelector.ValueFromIdealSize;

                DepthAtCenterDisplay.Text = string.Format("Depth At Selected Center : {0:g6}", e.ImageFrame.GetDistance((int)center.X, (int)center.Y).ToString());

                var rotationX = new Quaternion(new Vector3D(1, 0, 0), ProjectionAdjustRotaion.Value.X);
                var rotationY = new Quaternion(new Vector3D(0, 1, 0), ProjectionAdjustRotaion.Value.Y);
                var rotationZ = new Quaternion(new Vector3D(0, 0, 1), ProjectionAdjustRotaion.Value.Z);
                var rotation = new QuaternionRotation3D(rotationZ * rotationY * rotationX);

                Dictionary<string, object> state = new Dictionary<string, object>()
                {
                    {"twoByteDepthBits", e.ImageFrame.Image.Bits},
                    {"depthHorizontalResulotion", KinectInputImageWidth},
                    {"depthVerticalResulotion", KinectInputImageHeight},
                    {"centerPosition", CenterSelector.ValueFromIdealSize},
                    {"centerDepth", (int)CenterDepthBox.Value},
                    {"cameraConstant", CameraConstantBox.Value},
                    {"clip", ConvertUtil.ToIntRect(ClipSelector.ValueFromIdealSize)},
                    {"tolerance", (float)ToleranceDoubelBox.Value},
                    {"upperTolerance", (float)UpperToleranceDoubelBox.Value},
                    {"minHightAnormalities", (int)MinHeightAnormalities.Value},
                    {"sizeAtZeroTilt", OneSizeSelector.ValueFromIdealSize},
                    {"projectionAdjustRotation", rotation},
                    {"projectionAdjustScale", ProjectionAdjustScale.Value},
                    {"projectionAdjustTranslation", ProjectionAdjustTransalation.Value},
                    {"angleFactor", AngleFactor.Value},
                    {"axesSeperatly", AxesSeperalty.IsChecked ?? false},
                    {"projectionInverted", ProjectionInverted.IsChecked ?? false},
                    {"generatePrettyPictures", this.IsVisible},
                    {"useMedian", this.UseMedian.IsChecked ?? true},
                };

                computaionTask = new Task<ImageProcessing.Output>(DoMainComputaionAsync, state);
                computaionTask.ContinueWith(DisplayComputation, TaskScheduler.FromCurrentSynchronizationContext());

                computaionTask.Start();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Frame Skipped!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                framesSkipt++; 
                FramesSkippedDisplay.Text = "Skiped Frames: " + framesSkipt;
            }

            System.Diagnostics.Debug.WriteLine("kinect_DepthFrameReady: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();
        }

        byte[] prettyPicture;
        ImageProcessing.Output DoMainComputaionAsync(object state)
        {
            var input = (Dictionary<string, object>)state;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var ballPosition = ImageProcessing.BallPositionFast(input, displays);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("TimeDebug_BallPositionTotal", displays, "Ball Position Total: {0}", stopwatch.ElapsedMilliseconds);

            stopwatch.Restart();
            if ((bool)input["generatePrettyPictures"])
                prettyPicture = PrettyPictureOfDepthData.PrettyPicture((byte[])input["twoByteDepthBits"]);
            DisplayDescribtion.CreateOrUpdateTextBoxDisplay("TimeDebug_PrettyPicture", displays, "PrettyPicture: {0}", stopwatch.ElapsedMilliseconds);

            stopwatch.Stop();
            return new ImageProcessing.Output(displays.ToArray(), ballPosition, prettyPicture);
        }

        void DisplayComputation(Task<ImageProcessing.Output> task)
        {
            var output = task.Result;

            Vector centerPos = CenterSelector.ValueFromIdealSize;
            double widthThatIsPlateSizePixel = (OneSizeSelector.ValueFromIdealSize.Width / 2);
            double withThatIsPlateSizeMeter = GlobalSettings.Instance.HalfPlateSize;

            BallSelector.ValueFromIdealSize = output.ballPosition
                * widthThatIsPlateSizePixel
                / withThatIsPlateSizeMeter
                + centerPos;

            if (output.prettyPicture != null && this.IsVisible)
            {
                InputImage.Source = CreateMyStandartBitmapSource(output.prettyPicture, KinectInputImageWidth, KinectInputImageHeight);
                OutputImage.Source = CreateMyStandartBitmapSource(output.prettyPicture, KinectInputImageWidth, KinectInputImageHeight);
            }

            foreach (var item in output.displays)
            {
                if(!item.Value.Display.IsValueCreated)
                {
                    DisplayDescribtionToPanelMapping(item.Key).Children.Add(item.Value.Display.Value);
                }

                item.Value.ToDisplay(item.Value.Display.Value, item.Value.Data);
            }

            Vector ballPos = output.ballPosition;
            ballPos.Y = -ballPos.Y; //Y is upsidedown in regualar Math

            BallPositionDisplay.Text = string.Format("Ball Position: {0:g6},{1:g6}", ballPos.X, ballPos.Y);

            SendData(ballPos);
            MainApp.MainWindow mainWindow = (BallOnTiltablePlate.JanRapp.MainApp.MainWindow)Application.Current.MainWindow;
            if (mainWindow != null)
                mainWindow.JugglerTimer();

            GC.Collect(0, GCCollectionMode.Optimized);
        }

        Panel DisplayDescribtionToPanelMapping(string dDName)
        {
            if (dDName.StartsWith("OutputImageSelector_"))
                return OutputImagePanel;
            else if (dDName.StartsWith("Corner"))
                return CornerContainer;
            else if (dDName.StartsWith("TimeDebug_"))
                return TimeDegugContainer;
            else
                return MainPanel;
        }

        BitmapSource CreateMyStandartBitmapSource(byte[] data, int width, int height)
        {
            return BitmapSource.Create(width, height, 96.0, 96.0, PixelFormats.Gray8, null, data, width);
        }

        bool started = false;
        public void Start()
        {
            try
            {
                started = true;
                if (kinect != null)
                {
                    kinect.Initialize(Kinect.RuntimeOptions.UseDepth);
                    kinect.DepthStream.Open(Kinect.ImageStreamType.Depth, 4, Kinect.ImageResolution.Resolution640x480, Kinect.ImageType.Depth);
                }
                else
                    TryInitializeKinectDevice();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kinect counldn't be initialized. Exception Message: " + ex.Message);
            }
        }

        public void Stop()
        {
            try
            {
                started = false;
                if (kinect != null)
                    kinect.Uninitialize();
            }
            catch { }
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

                HoveringColorDisplay.Text = string.Format("Hovering Color: {0:g6}", pixels[1].ToString()); //should Be Green. I didn't use the beginning or end, since there is alpha somewhere;
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

        bool lowRes = false;
        private void LowRes_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;

            if (lowRes)
            {
                SetNewResoultion(640, 480);
                b.Content = "Swtich to Low Res 320x240";
                kinect.Uninitialize();
                kinect.Initialize(Kinect.RuntimeOptions.UseDepth);
                kinect.DepthStream.Open(Kinect.ImageStreamType.Depth, 4, Kinect.ImageResolution.Resolution640x480, Kinect.ImageType.Depth);
            }
            else
            {
                SetNewResoultion(320, 240);
                b.Content = "Swtich to Hight Res 640x480";
                kinect.Uninitialize();
                kinect.Initialize(Kinect.RuntimeOptions.UseDepth);
                kinect.DepthStream.Open(Kinect.ImageStreamType.Depth, 4, Kinect.ImageResolution.Resolution320x240, Kinect.ImageType.Depth);
            }

            lowRes = !lowRes;
        }

        void SetNewResoultion(int newWidth, int newHeight)
        {
            KinectInputImageSize = new Vector(newWidth, newHeight);
            KinectInputImageWidth = newWidth;
            KinectInputImageHeight = newHeight;

            Size s = new Size(newWidth, newHeight);

            CenterSelector.IdealSize = s;
            ClipSelector.IdealSize = s;
            OneSizeSelector.IdealSize = s;
            BallSelector.IdealSize = s;


            foreach (var dd in displays)
                DisplayDescribtionToPanelMapping(dd.Key).Children.Remove(dd.Value.Display.Value);

            displays.Clear();
        }
    }
}
