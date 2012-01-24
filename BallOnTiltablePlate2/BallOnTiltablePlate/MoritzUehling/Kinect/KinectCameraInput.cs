using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect;
using BallOnTiltablePlate.MoritzUehling.UI;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;
using Manager = BallOnTiltablePlate.MoritzUehling.Kinect;
using System.Diagnostics;
using Microsoft.Research.Kinect.Nui;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace BallOnTiltablePlate.MoritzUehling.Kinect
{
    //[BallOnPlateItemInfo("Moritz", "Uehling", "Kinect Cam Input", "0.3")]
    public class KinectCameraInput : IBallInput
    {
        #region Managing Stuff
        public event EventHandler<BallInputEventArgs> DataRecived;

        public void Start()
        {
            
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        KinectSettingsWindows settingsWindow;
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return settingsWindow; }
        }
        #endregion

        #region Variables
        //Kinect
		Runtime nui;
        Rectangle plateArea;
		Vector3D Ball;

        ImageManager manager;

        //Depth Data Map
        int[,] depthMap;
        int xres = 640, yres = 480;
        #endregion

        #region Properties
        public Runtime Kinect { get { return nui; } }
        public int[,] KinectDepthMap { get { return depthMap; } }
        #endregion

        public KinectCameraInput()
        {
            InitKinect();

            manager = new ImageManager(xres, yres);
            settingsWindow = new KinectSettingsWindows(this);
        }

        public void InitKinect()
        {
            try
            {
				nui = Microsoft.Research.Kinect.Nui.Runtime.Kinects[0];

                //UseDepthAndPlayerIndex and UseSkeletalTracking
                Kinect.Initialize(RuntimeOptions.UseDepth);

                //register for event
                Kinect.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(Kinect_DepthFrameReady);

                //DepthAndPlayerIndex ImageType
                Kinect.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution640x480, ImageType.Depth);

                depthMap = new int[xres, yres];
            }
            catch
            {
            }
        }

        #region Kinect Working stuff
        void Kinect_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            FillImageMap(e.ImageFrame);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            ProcessPlate();

            watch.Stop();
            Debug.WriteLine(watch.Elapsed.TotalMilliseconds);

            //FillImageMap(e.ImageFrame);

			DataRecived(this, new BallInputEventArgs());    
        }
        #endregion

        #region KinectHelpers
        private int GetDistance(int x, int y, PlanarImage p)
        {
			int n = (y * xres + x) * 2;
			int distance = (p.Bits[n + 0] | p.Bits[n + 1] << 8);

            return distance;
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
					if (x > 0 && y > 0 && x + 1 < width && y + 1 < height)
					{
						depthMap[x, y] = GetDistance(x, y, imageFrame.Image);
					}

                    //jump two bytes at a time
                    depthIndex += 2;
                }
            }
        }

        #endregion

        #region Process
        public void ProcessPlate()
        {
            Ball = manager.GetBall(depthMap, settingsWindow.Point1, settingsWindow.Point2, settingsWindow.Point3, (int)(settingsWindow.limitSlider.Value));
        }
        #endregion
    }
}
