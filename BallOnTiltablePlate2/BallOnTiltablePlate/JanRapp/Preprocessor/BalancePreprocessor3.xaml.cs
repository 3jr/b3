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
    [ControledSystemModuleInfo("Jan", "Rapp", "Balance Preprocessor", "1.9")]
    public partial class BalancePreprocessor3 : UserControl, IControledSystemPreprocessorIO<IBallInput, IPlateOutput>, IBalancePreprocessor, IBasicPreprocessor, IControledSystemPreprocessor
    {
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

        public BalancePreprocessor3()
        {
            InitializeComponent();

            ReinitialiceStateObservers();
        }

        Vector integral;
        double deltaTime;

        StateObserver SoX;
        StateObserver SoY;
        Vector lastTilt = new Vector();
        void Input_DataRecived(object sender, BallInputEventArgs e)
        {
            if (Position.HasNaN() && !e.BallPosition.HasNaN())
            {
                SoX.xh[0] = e.BallPosition.X;
                SoX.xh[1] = 0;
                SoY.xh[0] = e.BallPosition.Y;
                SoY.xh[1] = 0;
            }

            deltaTime = (double)sinceLastUpdate.ElapsedMilliseconds / 1000.0;
            deltaTime = ((UseDelataTime.IsChecked ?? true) ? deltaTime : StaticPeriod.Value);

            Vector newPosition = e.BallPosition;

            if (!newPosition.HasNaN())
            {

                SoX.NextStep(newPosition.X, lastTilt.X, deltaTime);
                SoY.NextStep(newPosition.Y, lastTilt.Y, deltaTime);

                Velocity = new Vector(SoX.xh[1], SoY.xh[1]);
                Position = newPosition;
            }
            ValuesValid = !Position.HasNaN() && !Velocity.HasNaN();

            PositionDisplay.Text = "Position: " + Position.ToString();
            VelocityDisplay.Text = "Velocity: " + Velocity.ToString();
            DeltaTimeDisplay.Text = "DeltaTime: " + deltaTime.ToString();

            if (this.IsVisible)
                History.FeedUpdate(Position, Velocity);

            AddDataToDiagramCreator();
            if (IsAutoBalancing)
            {
                if (this.ValuesValid)
                {
                    Vector currentRelativePosition = this.Position - TargetPosition;
                    integral += currentRelativePosition * deltaTime;
                    var tilt = currentRelativePosition * PositionFactor.Value +
                        integral * IntegralFactor.Value +
                        this.Velocity * VelocityFactor.Value;

                    IntegralDisplay.Text = "Integral: " + integral;

                    //if (Math.Abs(tilt.X) > GlobalSettings.Instance.MaxTilt)
                    //    tilt.X = GlobalSettings.Instance.MaxTilt * Math.Sign(tilt.X);

                    //if (Math.Abs(tilt.Y) > GlobalSettings.Instance.MaxTilt)
                    //    tilt.Y = GlobalSettings.Instance.MaxTilt * Math.Sign(tilt.Y);

                    Output.SetTilt(tilt);
                    if (recording)
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

            sinceLastUpdate.Restart();
        }

        public void Reset()
        {
            Position = VectorUtil.NaNVector;
            Velocity = VectorUtil.NaNVector;
            lastTilt = new Vector();
            SoX.xh = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(4, 0.0);
            SoY.xh = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(4, 0.0);
            sinceLastUpdate.Restart();
        }

        public void SetTilt(Vector tiltToAxis)
        {
            lastTilt = GlobalSettings.Instance.ToValidTilt(tiltToAxis);
            IsAutoBalancing = false;
            Output.SetTilt(tiltToAxis);
            if (recording)
            {
                diagramcreator.AddPoint("JugglerTiltX", new Point(time, tiltToAxis.X));
                diagramcreator.AddPoint("JugglerTiltY", new Point(time, tiltToAxis.Y));
            }
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

        private void gDB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ReinitialiceStateObservers();
        }

        private void gDB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Vector> e)
        {
            ReinitialiceStateObservers();
        }

        private void ReinitialiceStateObservers()
        {
            var g = gDB.Value;
            var s1 = SDB.Value.X;
            var s2 = SDB.Value.Y;

            var l1_1 = L1DB.Value.X;
            var l1_2 = L1DB.Value.Y;
            var l2_1 = L2DB.Value.X;
            var l2_2 = L2DB.Value.Y;

            var A = new double[,]{
               {0,  1,  0,  0,  },
               {0,  0,  g,  0,  },
               {0,  0,  0,  1,  },
               {0,  0,  s1, s2, },
            };

            var B = new double[,]{
               {0,  },
               {0,  },
               {0,  },
               {1,  },
            };

            var C = new double[,]{
               {1,  0,  0,  0,  },
            };

            var L = new double[,]{
               {l1_1,  },
               {l1_2,  },
               {l2_1,  },
               {l2_2,  },
            };

            SoX = new StateObserver(A, B, C, L, new double[] { 0, 0, 0, 0 });
            SoY = new StateObserver(A, B, C, L, new double[] { 0, 0, 0, 0 });
        }
    }
}
