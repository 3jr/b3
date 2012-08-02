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
    public interface IPVASetable : IControledSystemPreprocessor
    {
        Vector TargetPosition { get; set; }
        Vector TargetVelocity { get; set; }
        Vector TargetAcceleration { get; set; }
    }

    /// <summary>
    /// Interaction logic for BasicPreprocessor.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan", "Rapp", "PVASetable", "0.1")]
    public partial class PVASetable : UserControl,
        IPVASetable,
        IControledSystemPreprocessor,
        IControledSystemPreprocessorIO<IBallInput, IPlateOutput>
    {
        public PVASetable()
        {
            InitializeComponent();

            ReinitialiceStateObservers();
        }

        #region Interface Properties
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

        public Vector TargetVelocity
        {
            get { return TargetVelocityVecBox.Value; }
            set { TargetVelocityVecBox.Value = value; }
        }

        public Vector TargetAcceleration
        {
            get { return TargetAccelerationVecBox.Value; }
            set { TargetAccelerationVecBox.Value = value; }
        }

        public Vector Position { get; private set; }

        public Vector Velocity { get; private set; }

        public Vector Acceleration { get; private set; }

        public IBallInput Input { private get; set; }

        public IPlateOutput Output { private get; set; }

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        #region Interface Methods
        public void Start()
        {
            Input.DataRecived += (Input_DataRecived);
            Reset();
        }

        public void Stop()
        {
            Input.DataRecived -= Input_DataRecived;
        }
        #endregion

        #region Diagram
        private static ExcelUtilities.ExcelDiagramCreator diagramcreator;
        private static bool recording = false;
        private static double time = 0;
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

        StateObserver SoX, SoY;
        Vector integral;
        Vector lastTilt;
        void Input_DataRecived(object sender, BallInputEventArgs e)
        {
            Vector newBallPos = e.BallPosition;

            if (Position.HasNaN() && !newBallPos.HasNaN())
            {
                double[] resetX = new double[SoX.xh.Count];
                resetX[0] = e.BallPosition.X;
                SoX.xh.SetValues(resetX);
                double[] resetY = new double[SoY.xh.Count];
                resetY[0] = e.BallPosition.Y;
                SoY.xh.SetValues(resetY);
            }

            double deltaTime = StaticPeriod.Value;

            if (!newBallPos.HasNaN())
            {
                this.SoX.NextStep(newBallPos.X, this.lastTilt.X, deltaTime);
                this.SoY.NextStep(newBallPos.Y, this.lastTilt.Y, deltaTime);

                this.Acceleration = new Vector(this.SoX.xh[2], this.SoY.xh[2]);
                this.Velocity = new Vector(this.SoX.xh[1], this.SoY.xh[1]);
                this.Position = newBallPos; // new Vector(SoX.xh[0], SoY.xh[0]);
            }
            else
            {
                this.Acceleration = VectorUtil.NaNVector;
                this.Velocity = VectorUtil.NaNVector;
                this.Position = VectorUtil.NaNVector;
            }

            #region Display
            this.PositionDisplay.Text = "Position: " + this.Position.ToString();
            this.VelocityDisplay.Text = "Velocity: " + this.Velocity.ToString();
            this.DeltaTimeDisplay.Text = "DeltaTime: " + deltaTime.ToString();
            this.xhDispaly.Text = ""
                + this.SoX.xh[0] + "\n"
                + this.SoX.xh[1] + "\n"
                + this.SoX.xh[2] + "\n"
                + this.SoX.xh[3] + "\n"
                + this.SoY.xh[0] + "\n"
                + this.SoY.xh[1] + "\n"
                + this.SoY.xh[2] + "\n"
                + this.SoY.xh[3] + "\n"
            ;

            if (this.IsVisible)
                History.FeedUpdate(Position, Velocity);

            AddDataToDiagramCreator();
            #endregion

            // PD Regeler
            if (!this.Position.HasNaN() && !this.Velocity.HasNaN())
            {
                Vector currentRelativePosition = this.TargetPosition - this.Position;
                Vector currentRelativeVelocity = this.TargetVelocity - this.Velocity;
                this.integral += currentRelativePosition * deltaTime;

                var tilt =
                    currentRelativePosition
                        * this.PositionFactor.Value + //P
                    this.integral
                        * this.IntegralFactor.Value + //I
                    currentRelativeVelocity
                        * this.VelocityFactor.Value; //D

                //this.SetTilt(-tilt);
                this.SetTilt(1 / (-gDB.Value) * (tilt + this.TargetAcceleration));

                #region Display
                IntegralDisplay.Text = "Integral: " + integral;
                if (recording)
                {
                    diagramcreator.AddPoint("TiltX", new Point(time, tilt.X));
                    diagramcreator.AddPoint("TiltY", new Point(time, tilt.Y));
                }
                #endregion
            }
            else
            {
                this.SetTilt(new Vector());
                this.integral = new Vector();
            }
        }

        // No MotorDynamik
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
               {0,  1,  },
               {0,  0,  },
            };

            var B = new double[,]{
               {0,  },
               {g,  },
            };

            var C = new double[,]{
               {1,  0,  },
            };

            var L = new double[,]{
               {l1_1,  },
               {l1_2,  },
            };

            SoX = new StateObserver(A, B, C, L,
                new double[] { 0, 0, });
            SoY = new StateObserver(A, B, C, L,
                new double[] { 0, 0, });
        }
        // MotorDynamik
        private void ReinitialiceStateObservers2()
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
               {-s1,},
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
            this.integral = new Vector();
            ReinitialiceStateObservers();
        }

        private void SetTilt(Vector tilt)
        {
            this.lastTilt = GlobalSettings.Instance.ToValidTilt(tilt);
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
        #endregion
    }
}
