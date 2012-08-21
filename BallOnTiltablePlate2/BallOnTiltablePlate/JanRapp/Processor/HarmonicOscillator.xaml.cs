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
using BallOnTiltablePlate.JanRapp.Preprocessor;
using JRapp;
using BallOnTiltablePlate.JanRapp.Utilities;

namespace BallOnTiltablePlate.JanRapp.Processor
{
    /// <summary>
    /// Interaction logic for HarmonicOscillator.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan", "Rapp", "Harmonic Oscillator", "0.1")]
    public partial class HarmonicOscillator : UserControl, IControledSystemProcessor<IBasicPreprocessor>
    {
        public HarmonicOscillator()
        {
            InitializeComponent();
        }

        public IBasicPreprocessor IO { private get; set; }

        Vector intgralDeltaVelocity;
        Vector lastDeltaVelocity;
        System.Diagnostics.Stopwatch sinceLastUpdate = new System.Diagnostics.Stopwatch();

        public void Update()
        {
            if (IO.ValuesValid)
            {
                var tilt = new Vector();

                double omega = 2*Math.PI * Frequency.Value;

                double targetVelocityX = 0;
                double deltaVelocityX = 0;
                if (Math.Abs(IO.Position.X) < MaxAmplitude.Value)
                {
                    targetVelocityX = MaxAmplitude.Value * omega * Math.Sqrt(1 - IO.Position.X * IO.Position.X / (MaxAmplitude.Value * MaxAmplitude.Value));
                    deltaVelocityX = IO.Velocity.X - targetVelocityX;
                }

                targetVelocityXDisplay.Text = "targetVelocityX" + targetVelocityX;
                deltaVelocityXDisplay.Text = "deltaVelocityX" + deltaVelocityX;

                double targetVelocityY = 0;
                double deltaVelocityY = 0;
                if (Math.Abs(IO.Position.Y) < MaxAmplitude.Value)
                {
                    targetVelocityY = -MaxAmplitude.Value * omega * Math.Sqrt(1 - IO.Position.Y * IO.Position.Y / (MaxAmplitude.Value * MaxAmplitude.Value));
                    deltaVelocityY = IO.Velocity.Y - targetVelocityY;
                }

                targetVelocityYDisplay.Text = "targetVelocityY" + targetVelocityY;
                deltaVelocityYDisplay.Text = "deltaVelocityY" + deltaVelocityY;

                Vector deltaVelocity = new Vector(deltaVelocityX, deltaVelocityY);

                intgralDeltaVelocity += deltaVelocity;
                intgralDeltaVelocity = intgralDeltaVelocity.ToNoNaN();
                var deltaTime = (double)sinceLastUpdate.ElapsedMilliseconds / 1000.0;
                tilt += deltaVelocity * PositionFactor.Value +
                    intgralDeltaVelocity * IntegralFactor.Value * deltaTime +
                    (deltaVelocity - lastDeltaVelocity) * VelocityFactor.Value;
                lastDeltaVelocity = deltaVelocity;

                tilt = tilt.ToNoNaN();
                if (Math.Abs(deltaVelocityX) < Accuracy.Value)
                    if (Math.Abs(-omega * omega * IO.Position.X) < 10)
                        tilt.X += Math.Asin(-omega * omega * IO.Position.X / 10);
                    else
                        tilt.X += GlobalSettings.Instance.MaxTilt * Math.Sign(IO.Position.X);

                targetAccelerationXDisplay.Text = "-omega * omega * IO.Position.X: " + -omega * omega * IO.Position.X;
                isTargetAccelerationUsedXDisplay.Text = "Math.Abs(deltaVelocityX) < Accuracy.Value: " + (Math.Abs(deltaVelocityX) < Accuracy.Value);

                if(Math.Abs(deltaVelocityY) < Accuracy.Value)
                    if (Math.Abs(-omega * omega * IO.Position.Y) < 10)
                        tilt.Y += Math.Asin(-omega * omega * IO.Position.Y / 10);
                    else
                        tilt.Y += GlobalSettings.Instance.MaxTilt * Math.Sign(IO.Position.Y);

                targetAccelerationYDisplay.Text = "-omega * omega * IO.Position.Y: " + -omega * omega * IO.Position.Y;
                isTargetAccelerationUsedYDisplay.Text = "Math.Abs(deltaVelocityY) < Accuracy.Value: " + (Math.Abs(deltaVelocityY) < Accuracy.Value);

                if (Math.Abs(tilt.X) > GlobalSettings.Instance.MaxTilt)
                    tilt.X = GlobalSettings.Instance.MaxTilt * Math.Sign(tilt.X);

                if (Math.Abs(tilt.Y) > GlobalSettings.Instance.MaxTilt)
                    tilt.Y = GlobalSettings.Instance.MaxTilt * Math.Sign(tilt.Y);

                IO.SetTilt(tilt);
            }
        }

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
