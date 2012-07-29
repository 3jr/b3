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
        IControledSystemPreprocessorIO<IBallInput, IPlateOutput>,
        IControledSystemPreprocessor
    {
        System.Diagnostics.Stopwatch sinceLastUpdate = new System.Diagnostics.Stopwatch();

        public Vector Position { get; private set; }

        public Vector Velocity { get; private set; }

        public Vector Acceleration { get; private set; }

        public IBallInput Input { private get; set; }

        public IPlateOutput Output { private get; set; }

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        public PVASetable()
        {
            InitializeComponent();

            ReinitialiceStateObservers();
        }

        Vector integral;

        StateObserver SoX;
        StateObserver SoY;
        Vector lastTilt = new Vector();
        void Input_DataRecived(object sender, BallInputEventArgs e)
        {
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

            Vector newPosition = e.BallPosition;

            if (!newPosition.HasNaN())
            {
                for (int i = 0; i < 100; i++)
                {
                    SoX.NextStep(newPosition.X, lastTilt.X, deltaTime / 100);
                    SoY.NextStep(newPosition.Y, lastTilt.Y, deltaTime / 100);
                    Acceleration = new Vector(SoX.xhp[1], SoY.xhp[1]);
                    Velocity = new Vector(SoX.xh[1], SoY.xh[1]);
                    Position = new Vector(SoX.xh[0], SoY.xh[0]);
                }
            }
            else
            {
                Acceleration = VectorUtil.NaNVector;
                Velocity = VectorUtil.NaNVector;
                Position = VectorUtil.NaNVector;
            }

            #region Display of Values
            PositionDisplay.Text = "Position: " + Position.ToString();
            VelocityDisplay.Text = "Velocity: " + Velocity.ToString();
            DeltaTimeDisplay.Text = "DeltaTime: " + deltaTime.ToString();
            xhDispaly.Text = SoX.xh[0] + "\n\r" +
                SoX.xh[1] + "\n" +
                SoX.xh[2] + "\n" +
                SoX.xh[3] + "\n" +
                SoY.xh[0] + "\n" +
                SoY.xh[1] + "\n" +
                SoY.xh[2] + "\n" +
                SoY.xh[3] + "\n";

            if (this.IsVisible)
                History.FeedUpdate(Position, Velocity);
            AddDataToDiagramCreator();
            #endregion

            // PD Regeler
            if (!Position.HasNaN() && !Velocity.HasNaN())
            {
                Vector currentRelativePosition = this.Position - this.TargetPosition;
                Vector currentRelativeVelocity = this.Velocity - this.TargetVelocity;
                this.integral += currentRelativePosition * deltaTime;

                var tilt =
                    currentRelativePosition
                        * this.PositionFactor.Value + //P
                    this.integral
                        * this.IntegralFactor.Value + //I
                    currentRelativeVelocity
                        * this.VelocityFactor.Value; //D

                //this.InternalSetTilt(tilt);
                this.InternalSetTilt(1/gDB.Value * (tilt + this.TargetAcceleration));

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
                this.InternalSetTilt(new Vector());
                integral = new Vector();
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

        private void InternalSetTilt(Vector tilt)
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
    }
}
