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

namespace BallOnTiltablePlate.MoritzUehling.Kinect
{
    [BallOnPlateItemInfo("Moritz", "Uehling", "Kinect Cam Input", "0.2")]
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

        ImageManager manager;

        //Depth Data Map
        int[,] depthMap;
        int xres = 320, yres = 240;
        #endregion

        #region Properties
        public Runtime Kinect { get { return nui; } }
        public Rectangle PlateArea { get { return plateArea; } }
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
                Kinect.Initialize(RuntimeOptions.UseDepthAndPlayerIndex);

                //register for event
                Kinect.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(Kinect_DepthFrameReady);

                //DepthAndPlayerIndex ImageType
                Kinect.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);

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

            FillImageMap(e.ImageFrame);

			//DataRecived(this, new BallInputEventArgs());    
        }
        #endregion

        #region KinectHelpers
        private int GetDistanceWithPlayerIndex(byte firstFrame, byte secondFrame)
        {
            //offset by 3 in first byte to get value after player index
            int distance = (int)(firstFrame >> 3 | secondFrame << 5);
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

                    depthMap[x, y] = GetDistanceWithPlayerIndex(depthData[depthIndex], depthData[depthIndex + 1]);

                    //jump two bytes at a time
                    depthIndex += 2;
                }
            }
        }

        #endregion

        #region Process
        public void ProcessPlate()
        {
            plateArea = manager.GetPoints(depthMap, settingsWindow.rectPoint, (int)(5 * 2));
        }
        #endregion
    }
}
