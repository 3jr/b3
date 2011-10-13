using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BallOnTiltablePlate.MoritzUehling.UI;
using Microsoft.Research.Kinect.Nui;

namespace BallOnTiltablePlate.MoritzUehling.Kinect
{
    [BallOnPlateItemInfo("Moritz", "Uehling", "Kinect Cam Input", "0.2")]
    class KinectCameraInput : IBallInput
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

        public System.Windows.FrameworkElement SettingsUI
        {
            get { return new KinectSettingsWindows(); }
        }
        #endregion

        #region Variables
        //Kinect
        Runtime nui = new Runtime();
        
        //Depth Data Map
        int[,] depthMap;
        int xres = 320, yres = 240;
        #endregion

        public KinectCameraInput()
        {

        }

        public void InitKinect()
        {
        
        }
    }
}
