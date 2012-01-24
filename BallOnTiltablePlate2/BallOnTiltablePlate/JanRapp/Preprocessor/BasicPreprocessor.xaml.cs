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
    public interface IBasicPreprocessor : IPreprocessor
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
    [BallOnPlateItemInfo("Jan","Rapp", "Basic Preprocessor", "1.0")]
    public partial class BasicPreprocessor : UserControl, IPreprocessorIO<IBallInput, IPlateOutput>, IBasicPreprocessor
    {
        DateTime lastUpdate;

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
            double deltaTime = (DateTime.Now - lastUpdate).TotalSeconds;

            Vector newPosition = e.BallPosition;
            Vector newVelocity = (newPosition - Position) / deltaTime;
                   Acceleration = (Velocity - newVelocity) / deltaTime;

            Velocity = newVelocity;
            Position = newPosition;
            lastUpdate = DateTime.Now;
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
            lastUpdate = DateTime.Now;
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
