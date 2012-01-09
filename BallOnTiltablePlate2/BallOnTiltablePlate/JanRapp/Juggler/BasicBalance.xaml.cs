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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BallOnTiltablePlate.JanRapp.MainApp.Helper;

namespace BallOnTiltablePlate.Juggler.JanRapp
{
    /// <summary>
    /// Interaction logic for BasicBalance.xaml
    /// </summary>
    [BallOnPlateItemInfo("Jan", "Rapp", "BasicBalance", "1.0")]
    public partial class BasicBalance : UserControl, IJuggler<TestPreprocessor>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        public TestPreprocessor IO { private get; set; }
        Vector position;
        Vector velocity;
        Vector acceleration;

        public void Start()
        {
            IO.Input.DataRecived += Input_DataRecived;
        }

        public void Stop()
        {
            IO.Input.DataRecived -= Input_DataRecived;
        }

        private void Input_DataRecived(object sender, BallInputEventArgs e)
        {
            Vector newPosition = e.BallPosition;            
            //Vector newVelocity = position - newPosition;
            //Vector acceleration = velocity - newVelocity;
            velocity = newPosition - position;
            position = newPosition;

            Positiondisplay.Text = "Position: " + position.ToString();
            VelocityDisplay.Text = "Velocity: " + velocity.ToString();
            AccelerationDisplay.Text = "Acceleration: " + acceleration.ToString();
        
            var tilt = new Vector(
                AngleNeededToGetAccelerationOf(velocity.X),
                AngleNeededToGetAccelerationOf(velocity.Y)
                ) * MagigValue.Value;

            IO.Output.SetTilt(tilt);
        }

        public BasicBalance()
        {
            InitializeComponent();
        }

        public void Update()
        {
        }

        double AngleNeededToGetAccelerationOf(double acceleration)
        {
            return Math.Asin(acceleration / 10.0);
        }
    }
}
