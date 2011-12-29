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
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BallOnTiltablePlate.TimoSchmetzer.Preprocessor
{
    /// <summary>
    /// Interaction logic for Preprocessor.xaml
    /// </summary>
    [BallOnPlateItemInfo("Timo", "Schmetzer", "Preprocessor", "0.1")]
    public partial class Preprocessor : UserControl, IPreprocessor, IPreprocessorIO<IBallInput3D, IPlateOutput>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        public IBallInput3D Input { set { value.DataRecived += new EventHandler<BallInputEventArgs3D>(value_DataRecived); } }

        void value_DataRecived(object sender, BallInputEventArgs3D e)
        {
            _RecieveTime = DateTime.Now;
            _LastRecievedPosition = e.BallPosition3D;
        }

        private Vector3D _LastRecievedPosition;
        public Vector3D LastRecievedPosition { get { return _LastRecievedPosition; } }
        private DateTime _RecieveTime;
        public DateTime RecieveTime { get { return _RecieveTime; } }

        public Vector3D Position
        {
            get 
            {
                //TODO: Calc CURRENT Positions based on Recieved ones. Has to take care of elapsed time(RecieveTime).
                throw new NotImplementedException();
            }
        }

        public Vector3D Velocity
        {
            get
            {
                //TODO: Calc CURRENT Velocity based on Recieved ones. Has to take care of elapsed time(RecieveTime).
                throw new NotImplementedException();
            }
        }

        public Vector3D Acceleration
        {
            get
            {
                //TODO: Calc CURRENT Acceleration based on Recieved ones. Has to take care of elapsed time(RecieveTime).
                throw new NotImplementedException();
            }
        }

        public Vector Tilt
        {
            set { Output.SetTilt(value); }
        }

        public IPlateOutput Output { private get; set; }

        public Preprocessor()
        {
            InitializeComponent();
        }
    }
}
