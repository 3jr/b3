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
    public interface ICirclePreprocessor : IBasicPreprocessor, IControledSystemPreprocessor
    {
        double TargetOrthagonalFactor { set; get; }
        bool IsAutoBalancing { get; set; }
    }

    /// <summary>
    /// Interaction logic for BasicPreprocessor.xaml
    /// </summary>
    [ControledSystemModuleInfo("Timo","Schmetzer", "Circle Preprocessor", "0.1")]
    public partial class CirclePreprocessor : UserControl, IControledSystemPreprocessorIO<IBallInput, IPlateOutput>, ICirclePreprocessor, IBasicPreprocessor, IControledSystemPreprocessor
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

        public CirclePreprocessor()
        {
            InitializeComponent();
        }

        //Vector integral;
        //Vector lastRelativePositon;
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

            if (IsAutoBalancing)
            {
                if (this.ValuesValid)
                {
                    //Vector currentRelativePosition = this.Position - TargetPosition;
                    //integral += currentRelativePosition;
                    //var tilt = currentRelativePosition * PositionFactor.Value +
                    //    integral * IntegralFactor.Value * deltaTime +
                    //    (currentRelativePosition - lastRelativePositon) * VelocityFactor.Value;

                    Vector Pos = Position;
                    Pos.Normalize();
                    Vector vs = new Vector(-Pos.Y, Pos.X);
                    vs *= OrthagonalVelocityFactor.Value;

                    var tilt = VelocityFactor.Value * (Velocity - vs) + PositionFactor.Value * Position;

                    //lastRelativePositon = currentRelativePosition;
                    //IntegralDisplay.Text = "Integral: " + integral;

                    if (Math.Abs(tilt.X) > GlobalSettings.Instance.MaxTilt)
                        tilt.X = GlobalSettings.Instance.MaxTilt * Math.Sign(tilt.X);

                    if (Math.Abs(tilt.Y) > GlobalSettings.Instance.MaxTilt)
                        tilt.Y = GlobalSettings.Instance.MaxTilt * Math.Sign(tilt.Y);

                    Output.SetTilt(tilt);
                }
                else
                {
                    Output.SetTilt(new Vector());
                    //integral = new Vector();
                    //lastRelativePositon = VectorUtil.NaNVector;
                }
            }
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
            IsAutoBalancing = false;
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

        public double TargetOrthagonalFactor
        {
            get { return OrthagonalVelocityFactor.Value; }
            
            set
            {
                if (OrthagonalVelocityFactor.Value != value)
                {
                    //integral = new Vector();
                    OrthagonalVelocityFactor.Value = value;
                    //lastRelativePositon = VectorUtil.NaNVector;
                }
            }
        }

        public bool IsAutoBalancing
        {
            get { return IsAutoBalancingOnCheckBox.IsChecked ?? true; }
            set { IsAutoBalancingOnCheckBox.IsChecked = value; }
        }
    }
}
