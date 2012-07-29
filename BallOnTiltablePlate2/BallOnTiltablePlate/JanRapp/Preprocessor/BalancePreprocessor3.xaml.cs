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
    [ControledSystemModuleInfo("Jan", "Rapp", "Balance Preprocessor", "2.1")]
    public partial class BalancePreprocessor3 : UserControl, IControledSystemPreprocessorIO<IBallInput, IPlateOutput>, IBalancePreprocessor, IBasicPreprocessor, IControledSystemPreprocessor
    {
        #region Diagram
        private ExcelUtilities.ExcelDiagramCreator diagramcreator;
        private bool recording = false;
        private double time;
        private void AddDataToDiagramCreator()
        {
            if (recording)
            {
                time += StaticPeriod.Value;
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

        #region Interface Methods
        public void Start()
        {
            Input.DataRecived += Input_DataRecived;
            Reset();
        }

        public void Stop()
        {
            Input.DataRecived -= Input_DataRecived;
        }

        void IBasicPreprocessor.SetTilt(Vector tiltToAxis)
        {
            this.IsAutoBalancing = false;
            this.SetTilt(tiltToAxis);
        }
        #endregion

        #region Interface Properties and Fields
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
        #endregion

        StateObserver SoX, SoY;
        Vector lastTilt = new Vector();
        Vector integral;
        void Input_DataRecived(object sender, BallInputEventArgs e)
        {
            Vector newBallPos = e.BallPosition;

            if (Position.HasNaN() && !e.BallPosition.HasNaN())
            {
                SoX.xh[0] = e.BallPosition.X;
                SoX.xh[1] = 0;
                SoX.xh[2] = 0;
                SoX.xh[3] = 0;
                SoY.xh[0] = e.BallPosition.Y;
                SoY.xh[1] = 0;
                SoY.xh[2] = 0;
                SoY.xh[3] = 0;
            }

            double deltaTime = StaticPeriod.Value;

            if (!newBallPos.HasNaN())
            {
                this.SoX.NextStep(newBallPos.X, this.lastTilt.X, deltaTime);
                this.SoY.NextStep(newBallPos.Y, this.lastTilt.Y, deltaTime);

                this.Velocity = new Vector(this.SoX.xh[1], this.SoY.xh[1]);
                this.Position = newBallPos; // new Vector(SoX.xh[0], SoY.xh[0]);
            }
            else
            {
                this.Velocity = VectorUtil.NaNVector;
                this.Position = VectorUtil.NaNVector;
            }

            this.ValuesValid = !this.Position.HasNaN() && !this.Velocity.HasNaN();

            this.PositionDisplay.Text = "Position: " + this.Position.ToString();
            this.VelocityDisplay.Text = "Velocity: " + this.Velocity.ToString();
            this.DeltaTimeDisplay.Text = "DeltaTime: " + deltaTime.ToString();
            this.xhDispaly.Text = this.SoX.xh[0] + "\n\r" +
                this.SoX.xh[1] + "\n" +
                this.SoX.xh[2] + "\n" +
                this.SoX.xh[3] + "\n" +
                this.SoY.xh[0] + "\n" +
                this.SoY.xh[1] + "\n" +
                this.SoY.xh[2] + "\n" +
                this.SoY.xh[3] + "\n";

            if (this.IsVisible)
                this.History.FeedUpdate(Position, Velocity);

            AddDataToDiagramCreator();
            if (this.IsAutoBalancing)
            {
                if (this.ValuesValid)
                {
                    Vector currentRelativePosition = this.Position - this.TargetPosition;
                    this.integral += currentRelativePosition * deltaTime;
                    var tilt = currentRelativePosition * this.PositionFactor.Value +
                        integral * this.IntegralFactor.Value +
                        this.Velocity * this.VelocityFactor.Value;

                    this.IntegralDisplay.Text = "Integral: " + this.integral;

                    SetTilt(tilt);
                    if (recording)
                    {
                        diagramcreator.AddPoint("TiltX", new Point(time, tilt.X));
                        diagramcreator.AddPoint("TiltY", new Point(time, tilt.Y));
                    }
                }
                else
                {
                    this.SetTilt(new Vector());
                    this.integral = new Vector();
                }
            }
        }

        private void ReinitialiceStateObservers()
        {
            var g = -gDB.Value;
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
               {S1UsedInB.IsChecked ?? true ? -s1 : 1,  },
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

        public void Reset()
        {
            this.Position = VectorUtil.NaNVector;
            this.Velocity = VectorUtil.NaNVector;
            this.lastTilt = new Vector();
            this.SoX.xh = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(4, 0.0);
            this.SoY.xh = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(4, 0.0);
        }

        private void SetTilt(Vector tilt)
        {
            this.lastTilt = tilt;
            this.Output.SetTilt(tilt);
        }

        #region UI Events
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void gDB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ReinitialiceStateObservers();
        }

        private void gDB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Vector> e)
        {
            ReinitialiceStateObservers();
        }

        private void S1UsedInB_Checked_1(object sender, RoutedEventArgs e)
        {
            ReinitialiceStateObservers();
        }
        #endregion
    }
}
