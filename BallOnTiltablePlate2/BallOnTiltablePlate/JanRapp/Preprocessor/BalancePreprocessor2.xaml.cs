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
    /// <summary>
    /// Interaction logic for BasicPreprocessor.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan","Rapp", "Balance Preprocessor", "2.0")]
    public partial class BalancePreprocessor2 : UserControl, IControledSystemPreprocessorIO<IBallInput, IPlateOutput>, IBalancePreprocessor, IBasicPreprocessor, IControledSystemPreprocessor
    {
        public BalancePreprocessor2()
        {
            InitializeComponent();
            lastTicks = DateTime.Now.Ticks;
        }

        System.Diagnostics.Stopwatch sinceLastUpdate = new System.Diagnostics.Stopwatch();

        public Vector Position { get; private set; }

        public Vector Velocity { get; private set; }

        public bool ValuesValid { get; private set; }

        public IBallInput Input { get; set; }

        public IPlateOutput Output { get; set; }

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        Vector integral;
        double deltaTime;
        long lastTicks;

        Vector estimationX = new Vector();
        Vector estimationY = new Vector();
        Vector lastTilt = new Vector();
        void Input_DataRecived(object sender, BallInputEventArgs e)
        {
            if (Position.HasNaN() && !e.BallPosition.HasNaN())
            {
                estimationX.X = e.BallPosition.X;
                estimationX.Y = 0;
                estimationY.X = e.BallPosition.Y;
                estimationY.Y = 0;
            }

            deltaTime = (double)sinceLastUpdate.ElapsedMilliseconds / 1000.0;
            deltaTime = ((UseDelataTime.IsChecked ?? true) ? deltaTime : StaticPeriod.Value);
            long currentTicks = DateTime.Now.Ticks;
            long deltaTicks = currentTicks - lastTicks;
            lastTicks = currentTicks;

            Vector newPosition = e.BallPosition;

            double g = Gravity.Value;

            Vector newEstimationX = new Vector();
            Vector newEstimationY = new Vector();

            newEstimationX.X = (estimationX.Y - LFactor.Value.X * (estimationX.X - newPosition.X)) * deltaTime + estimationX.X;
            newEstimationX.Y = (-g * lastTilt.X - LFactor.Value.Y * (estimationX.X - newPosition.X)) * deltaTime + estimationX.Y;
            newEstimationY.X = (estimationY.Y - LFactor.Value.X * (estimationY.X - newPosition.Y)) * deltaTime + estimationY.X;
            newEstimationY.Y = (-g * lastTilt.Y - LFactor.Value.Y * (estimationY.X - newPosition.Y)) * deltaTime + estimationY.Y;

            Velocity = new Vector(newEstimationX.Y, newEstimationY.Y);

            estimationX = newEstimationX;
            estimationY = newEstimationY;

            Position = newPosition;

            ValuesValid = !Position.HasNaN() && !Velocity.HasNaN();

            PositionDisplay.Text = "Position: " + Position.ToString();
            VelocityDisplay.Text = "Velocity: " + Velocity.ToString();
            DeltaTimeDisplay.Text = "DeltaTime: " + deltaTime.ToString();
            DeltaTicksDisplay.Text = "DeltaTicks: " + deltaTicks.ToString();

            if(this.IsVisible)
                History.FeedUpdate(Position, Velocity);
            
            AddDataToDiagramCreator();
            if (IsAutoBalancing)
            {
                if (this.ValuesValid)
                {
                    Vector currentRelativePosition = this.Position - TargetPosition;
                    integral += currentRelativePosition * deltaTime;
                    var tilt = currentRelativePosition * PositionFactor.Value +
                        integral * IntegralFactor.Value * deltaTime +
                        this.Velocity * VelocityFactor.Value;

                    IntegralDisplay.Text = "Integral: " + integral;

                    //if (Math.Abs(tilt.X) > GlobalSettings.Instance.MaxTilt)
                    //    tilt.X = GlobalSettings.Instance.MaxTilt * Math.Sign(tilt.X);

                    //if (Math.Abs(tilt.Y) > GlobalSettings.Instance.MaxTilt)
                    //    tilt.Y = GlobalSettings.Instance.MaxTilt * Math.Sign(tilt.Y);

                    this.InternalSetTilt(tilt);
                    if (recording)
                    {
                        diagramcreator.AddPoint("TiltX", new Point(time, tilt.X));
                        diagramcreator.AddPoint("TiltY", new Point(time, tilt.Y));
                    }
                }
                else
                {
                    this.InternalSetTilt(new Vector());
                    integral = new Vector();
                }
            }

            sinceLastUpdate.Restart();
        }

        public void Reset()
        {
            Position = VectorUtil.NaNVector;
            Velocity = VectorUtil.NaNVector;
            lastTilt = new Vector();
            estimationX = new Vector(); //VectorUtil.NaNVector;
            estimationY = new Vector(); //VectorUtil.NaNVector;
            sinceLastUpdate.Restart();
        }

        public void SetTilt(Vector tiltToAxis)
        {
            IsAutoBalancing = false;
            if (recording)
            {
                diagramcreator.AddPoint("JugglerTiltX", new Point(time, tiltToAxis.X));
                diagramcreator.AddPoint("JugglerTiltY", new Point(time, tiltToAxis.Y));
            }
            this.InternalSetTilt(tiltToAxis);
        }

        public void InternalSetTilt(Vector tilt)
        {
            lastTilt = GlobalSettings.Instance.ToValidTilt(tilt);
            Output.SetTilt(tilt);
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
