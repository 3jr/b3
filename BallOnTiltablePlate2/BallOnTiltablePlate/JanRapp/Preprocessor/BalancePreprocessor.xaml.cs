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
using BallOnTiltablePlate.TimoSchmetzer.Utilities;

namespace BallOnTiltablePlate.JanRapp.Preprocessor
{
    public interface IBalancePreprocessor : IBasicPreprocessor, IPreprocessor
    {
        Vector TargetPosition { set; get; }
        bool IsAutoBalancing { get; set; }
    }

    /// <summary>
    /// Interaction logic for BasicPreprocessor.xaml
    /// </summary>
    [BallOnPlateItemInfo("Jan","Rapp", "Balance Preprocessor", "1.0")]
    public partial class BalancePreprocessor : UserControl, IPreprocessorIO<IBallInput, IPlateOutput>, IBalancePreprocessor, IBasicPreprocessor, IPreprocessor
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

        public BalancePreprocessor()
        {
            InitializeComponent();
        }

        Vector integral;
        double integralOfAbsense;
        void Input_DataRecived(object sender, BallInputEventArgs e)
        {
            double deltaTime = (double)sinceLastUpdate.ElapsedMilliseconds / 1000.0;
            sinceLastUpdate.Restart();

            Vector newPosition = e.BallPosition;
            Vector newVelocity = (newPosition - Position) / ((UseDelataTime.IsChecked ?? true) ? deltaTime : 1);
            Acceleration = (newVelocity - Velocity) / ((UseDelataTime.IsChecked ?? true) ? deltaTime : 1);

            Velocity = newVelocity;
            Position = newPosition;
            ValuesValid = !Position.HasNaN() && !Velocity.HasNaN() && !Acceleration.HasNaN();

            PositionDisplay.Text = "Position: " + Position.ToString();
            VelocityDisplay.Text = "Velocity: " + Velocity.ToString();
            AccelerationDisplay.Text = "Acceleration: " + Acceleration.ToString();

            if(this.IsVisible)
                History.FeedUpdate(Position, Velocity, Acceleration);
            
            AddDataToDiagramCreator();
            if (IsAutoBalancing)
            {
                if (this.ValuesValid)
                {
                    Vector currentRelativePosition = this.Position - TargetPosition;
                    integral += currentRelativePosition * deltaTime;
                    integralOfAbsense += deltaTime;
                    var tilt = currentRelativePosition * PositionFactor.Value +
                        integral * IntegralFactor.Value +
                        currentRelativePosition.GetNormalized() * integralOfAbsense * IntegralOfAbsenseFactor.Value +
                        this.Velocity * VelocityFactor.Value;

                    IntegralDisplay.Text = "Integral: " + integral;

                    if (Math.Abs(tilt.X) > GlobalSettings.Instance.MaxTilt)
                        tilt.X = GlobalSettings.Instance.MaxTilt * Math.Sign(tilt.X);

                    if (Math.Abs(tilt.Y) > GlobalSettings.Instance.MaxTilt)
                        tilt.Y = GlobalSettings.Instance.MaxTilt * Math.Sign(tilt.Y);

                    Output.SetTilt(tilt);
                    if (recording && this.IsVisible)
                    {
                        diagramcreator.AddPoint("TiltX", new Point(time, tilt.X));
                        diagramcreator.AddPoint("TiltY", new Point(time, tilt.Y));
                    }
                }
                else
                {
                    Output.SetTilt(new Vector());
                    integral = new Vector();
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

        public Vector TargetPosition
        {
            get { return TargetPositionVecBox.Value; }
            
            set
            {
                if (TargetPositionVecBox.Value != value)
                {
                    integral = new Vector();
                    TargetPositionVecBox.Value = value;
                }
            }
        }

        public bool IsAutoBalancing
        {
            get { return IsAutoBalancingOnCheckBox.IsChecked ?? true; }
            set { IsAutoBalancingOnCheckBox.IsChecked = value; }
        }

        #region Diagram
        private static ExcelUtilities.ExcelDiagramCreator diagramcreator;
        private static bool recording = false;
        private static double time = 0;
        private void AddDataToDiagramCreator()
        {
            if (recording)
            {
                time += sinceLastUpdate.ElapsedMilliseconds / 1000.0;
                diagramcreator.AddPoint("PositionX", new Point(time, Position.X));
                diagramcreator.AddPoint("PositionY", new Point(time, Position.Y));
                diagramcreator.AddPoint("VelocityX", new Point(time, Velocity.X));
                diagramcreator.AddPoint("VelocityY", new Point(time, Velocity.Y));
                diagramcreator.AddPoint("AccelerationX", new Point(time, Acceleration.X));
                diagramcreator.AddPoint("AccelerationY", new Point(time, Acceleration.Y));
            }
        }
        private void CreateDiagram()
        {
            diagramcreator.AxisNameX = "Time";
            diagramcreator.AxisNameY = "Value";
            diagramcreator.DiagramTitle = "NoTitle";
            diagramcreator.GenerateAndShowDiagram();
        }
        private void ToogleRecordCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            recording = !recording;
            if (recording)
            {
                diagramcreator = new ExcelUtilities.ExcelDiagramCreator();
                time = 0;
                ToggleReccordBtn.Content = "Stop Recording";
            }
            else
            {
                System.Threading.Tasks.Task.Factory.StartNew(CreateDiagram);
                ToggleReccordBtn.Content = "Record";
            }
        }
        #endregion
    }
}
