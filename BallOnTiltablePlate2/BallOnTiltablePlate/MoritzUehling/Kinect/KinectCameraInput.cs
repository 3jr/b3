using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BallOnTiltablePlate.MoritzUehling.UI;

namespace BallOnTiltablePlate.MoritzUehling.Kinect
{
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

        #region Properties
		public System.Windows.FrameworkElement SettingsUI
        {
            get { return new KinectSettingsWindows(); }
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
            get { return "Moritz"; }
        }

        public string AuthorLastName
        {
            get { return "Uehling"; }
        }

        public Version Version
        {
            get { return new Version(0, 1, 1); }
        }
 
        #endregion
    }
}
