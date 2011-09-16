using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallOnTiltablePlate.MoritzUehling.Kinect
{
    class KinectCameraInput : IBallInput
    {
        public event EventHandler<BallInputEventArgs> DataRecived;



        public void Start()
        {
            //
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public System.Windows.FrameworkElement SettingsUI
        {
            get { return null; }
        }

        public object SettingsSave
        {
            get { return null; }
        }

        public string ItemName
        {
            get { return "Kinect Camera Input"; }
        }

        public string AuthorFirstName
        {
            get { return null; }
        }

        public string AuthorLastName
        {
            get { return null; }
        }

        public Version Version
        {
            get { return null; }
        }
    }
}
