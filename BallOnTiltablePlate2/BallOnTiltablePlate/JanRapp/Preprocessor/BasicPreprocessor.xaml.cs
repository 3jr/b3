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
using BallOnTiltablePlate.JanRapp.Utilities;

namespace BallOnTiltablePlate.JanRapp.Preprocessor
{
    public interface IBasicPreprocessor : IControledSystemPreprocessor
    {
        Vector Position { get; }
        Vector Velocity { get; }
        Vector Acceleration { get; }
        bool ValuesValid { get; }
        void SetTilt(Vector tiltToAxis);
    }

    /// <summary>
    /// Interaction logic for BasicPreprocessor.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan","Rapp", "Basic Preprocessor", "1.0")]
    public partial class BasicPreprocessor : UserControl, IControledSystemPreprocessorIO<IBallInput, IPlateOutput>, IBasicPreprocessor
    {
        System.Diagnostics.Stopwatch sinceLastUpdate = new System.Diagnostics.Stopwatch();

        public Vector Position { get; private set; }

        public Vector Velocity { get; private set; }

        public Vector Acceleration { get; private set; }

        public bool ValuesValid { get; private set; }

        public IBallInput Input { get; set; }

        public IPlateOutput Output { get; set; }

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        public BasicPreprocessor()
        {
            InitializeComponent();
        }
        
        void Input_DataRecived(object sender, BallInputEventArgs e)
        {
            double deltaTime = (double)sinceLastUpdate.ElapsedMilliseconds / 1000.0;
            sinceLastUpdate.Restart();

            Vector newPosition = e.BallPosition;
            Vector newVelocity = (newPosition - Position) / deltaTime;
                   Acceleration = (newVelocity - Velocity) / deltaTime;

            Velocity = newVelocity;
            Position = newPosition;
            ValuesValid = !Position.HasNaN() && !Velocity.HasNaN() && !Acceleration.HasNaN();

            PositionDisplay.Text = "Position: " + Position.ToString();
            VelocityDisplay.Text = "Velocity: " + Velocity.ToString();
            AccelerationDisplay.Text = "Acceleration: " + Acceleration.ToString();
        }

        public void Reset()
        {
            Position = VectorUtil.NaNVector;
            Velocity = VectorUtil.NaNVector;
            Acceleration = VectorUtil.NaNVector;
            sinceLastUpdate.Restart();
        }

        public void SetTilt(Vector tiltToAxis)
        {
            Output.SetTilt(tiltToAxis);
        }

        public void Start()
        {
            Input.DataRecived += (Input_DataRecived);
            Reset();
        }

        public void Stop()
        {
            Input.DataRecived -= Input_DataRecived;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }
    }
}
