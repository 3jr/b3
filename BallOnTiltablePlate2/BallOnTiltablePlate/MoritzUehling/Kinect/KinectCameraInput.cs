using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BallOnTiltablePlate.MoritzUehling.UI;

namespace BallOnTiltablePlate.MoritzUehling.Kinect
{
    [BallOnPlateItemInfo("Moritz", "Uehling", "Kinect Cam Input", "0.1")]
    class KinectCameraInput : IBallInput
    {
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
    }
}
