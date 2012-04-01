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
using BallOnTiltablePlate.JanRapp.Utilities;

namespace BallOnTiltablePlate.JanRapp.Juggler
{
    /// <summary>
    /// Interaction logic for BasicBalance.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan", "Rapp", "BasicBalance", "1.0")]
    public partial class BasicBalance1 : UserControl, IControledSystemProcessor<TestPreprocessor>
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
        DateTime lastUpdate;

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
            double deltaTime = (DateTime.Now - lastUpdate).TotalSeconds;
            Vector newPosition = e.BallPosition;            
            Vector newVelocity = (newPosition - position) / deltaTime;
                   acceleration = (velocity - newVelocity) / deltaTime;
            velocity = newVelocity;
            position = newPosition;
            lastUpdate = DateTime.Now;

            PositionDisplay.Text = "Position: " + position.ToString();
            VelocityDisplay.Text = "Velocity: " + velocity.ToString();
            AccelerationDisplay.Text = "Acceleration: " + acceleration.ToString();
        

        }

        public BasicBalance1()
        {
            InitializeComponent();
        }

        public void Update()
        {
            //var tilt = new Vector(
            //    AngleNeededToGetAccelerationOf(acceleration.X),
            //    AngleNeededToGetAccelerationOf(acceleration.Y)
            //    ).ToNoNaN() * AccelerationFactor.Value;

            //tilt += new Vector(
            //    AngleNeededToGetAccelerationOf(velocity.X),
            //    AngleNeededToGetAccelerationOf(velocity.Y)
            //    ).ToNoNaN() * VelocityFactor.Value;

            //tilt += new Vector(
            //    AngleNeededToGetAccelerationOf(position.X),
            //    AngleNeededToGetAccelerationOf(position.Y)
            //    ).ToNoNaN() * PositionFactor.Value;

            //tilt = new Vector(
            //    (velocity.X * VelocityFactor.Value),
            //    (velocity.Y * VelocityFactor.Value)
            //    ).ToNoNaN() * VelocityFactor.Value;

            Vector inductedVelo = position * PositionFactor.Value;

            var tilt = (velocity + inductedVelo)
                .ToNoNaN() * VelocityFactor.Value;
    

            IO.Output.SetTilt(tilt);
        }

        Vector AngleNeededToGetAccelerationOf(Vector acceleration)
        {
            return new Vector(AngleNeededToGetAccelerationOf(acceleration.X),
                AngleNeededToGetAccelerationOf(acceleration.Y));
        }

        double AngleNeededToGetAccelerationOf(double acceleration)
        {
            return Math.Asin(acceleration / 10.0);
        }

        private void ResetPrevData_Click(object sender, RoutedEventArgs e)
        {
            position = new Vector();
            velocity = new Vector();
        }
    }
}
