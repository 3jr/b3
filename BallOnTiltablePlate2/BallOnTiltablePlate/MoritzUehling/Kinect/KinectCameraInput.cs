using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallOnTiltablePlate.MoritzUehling.Kinect
{
    class KinectCameraInput : IBallInput
    {
        public event EventHandler<BallInputEventArgs> DataRecived;

        public KinectSettingsWindow


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
            get { throw new NotImplementedException(); }
        }

        public object SettingsSave
        {
            get { throw new NotImplementedException(); }
        }

        public string ItemName
        {
            get { throw new NotImplementedException(); }
        }

        public string AuthorFirstName
        {
            get { throw new NotImplementedException(); }
        }

        public string AuthorLastName
        {
            get { throw new NotImplementedException(); }
        }

        public Version Version
        {
            get { throw new NotImplementedException(); }
        }
    }
}
