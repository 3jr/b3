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
using System.Windows.Threading;
using BallOnTiltablePlate.JanRapp;
using BallOnTiltablePlate.JanRapp.MainApp;
using BallOnTiltablePlate.JanRapp.Utilities;
using BallOnTiltablePlate.TimoSchmetzer.Simulation;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;


namespace BallOnTiltablePlate.TimoSchmetzer.Preprocessor
{
    public interface IPVSetable : IControledSystemPreprocessor
    {
        Vector TargetPosition { get; set; }
        Vector TargetVelocity { get; set; }
    }

    /// <summary>
    /// Interaction logic for BasicPreprocessor.xaml
    /// </summary>
    [ControledSystemModuleInfo("Timo", "Schmetzer", "PVSetable", "0.1")]
    public partial class PVSetable : UserControl,
        IPVSetable,
        IControledSystemPreprocessor,
        IControledSystemPreprocessorIO<IBallInput, IPlateOutput>
    {
        public PVSetable()
        {
            InitializeComponent();

            ReinitialiceStateObservers();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Tick += timer_Tick;
        }

        #region Interface Properties
        public Vector TargetPosition
        {
            get { return TargetPositionVecBox.Value; }

            set
            {
                if (TargetPositionVecBox.Value != value)
                {
                    TargetPositionVecBox.Value = value;
                }
            }
        }

        public Vector TargetVelocity
        {
            get { return TargetVelocityVecBox.Value; }
            set { TargetVelocityVecBox.Value = value; }
        }

        public Vector Position { get; private set; }

        public Vector Velocity { get; private set; }

        public IBallInput Input { private get; set; }

        public IPlateOutput Output { private get; set; }

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }
        #endregion

        #region Interface Methods
        private double UTime;
        public void Start()
        {
            UTime = GlobalSettings.Instance.UpdateTime;
            GlobalSettings.Instance.UpdateTime = 0;
            Input.DataRecived += (Input_DataRecived);
            Reset();
            timer.Start();
        }

        public void Stop()
        {
            Input.DataRecived -= Input_DataRecived;
            timer.Stop();
            GlobalSettings.Instance.UpdateTime = UTime;
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
                time += GlobalSettings.Instance.UpdateTime/*StaticPeriod.Value*/;
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

        #region StateObserver
        StateObserver SoX, SoY;
        Vector lastTilt;

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
            ReinitialiceStateObservers();
            Position = new Vector(double.NaN, double.NaN);
            Velocity = new Vector(double.NaN, double.NaN);
            SimulatedPosition = new Vector(double.NaN, double.NaN);
            SimulatedVelocity = new Vector(double.NaN, double.NaN);
            LastSimulationTilt = new Vector(double.NaN, double.NaN);
        }
        #endregion

        #region Input
        private DateTime LastInput;
        void Input_DataRecived(object sender, BallInputEventArgs e)
        {
            Vector newBallPos = e.BallPosition;
            if (!newBallPos.HasNaN())
            {
                if (Position.HasNaN() && !newBallPos.HasNaN())
                {
                    double[] resetX = new double[SoX.xh.Count];
                    resetX[0] = e.BallPosition.X;
                    SoX.xh.SetValues(resetX);
                    double[] resetY = new double[SoY.xh.Count];
                    resetY[0] = e.BallPosition.Y;
                    SoY.xh.SetValues(resetY);
                }

                double deltaTime = (DateTime.Now - LastInput).TotalSeconds;
                LastInput = DateTime.Now;

                if (!newBallPos.HasNaN())
                {
                    this.SoX.NextStep(newBallPos.X, this.lastTilt.X, deltaTime);
                    this.SoY.NextStep(newBallPos.Y, this.lastTilt.Y, deltaTime);

                    this.Velocity = new Vector(this.SoX.xh[1], this.SoY.xh[1]);
                    this.Position = new Vector(this.SoX.xh[0], this.SoY.xh[0]);
                    if (!(sender == this))
                    {
                        lock (lockobj)
                        {
                            LastSimulation = DateTime.Now;
                            SimulatedPosition = Position;
                            SimulatedVelocity = Velocity;
                            timer_Tick(this, null);
                        }
                    }
                }
                else
                {
                    this.Velocity = VectorUtil.NaNVector;
                    this.Position = VectorUtil.NaNVector;
                }
                lastTilt = LastSimulationTilt;

                #region Display
                this.PositionDisplay.Text = "Position: " + this.Position.ToString();
                this.VelocityDisplay.Text = "Velocity: " + this.Velocity.ToString();
                this.DeltaTimeDisplay.Text = "DeltaTime: " + deltaTime.ToString();
                this.xhDispaly.Text = ""
                    + this.SoX.xh[0] + "\n"
                    + this.SoX.xh[1] + "\n"
                    // + this.SoX.xh[2] + "\n"
                    // + this.SoX.xh[3] + "\n"
                    + this.SoY.xh[0] + "\n"
                    + this.SoY.xh[1] + "\n"
                    // + this.SoY.xh[2] + "\n"
                    // + this.SoY.xh[3] + "\n"
                ;

                //if (this.IsVisible)
                //    History.FeedUpdate(Position, Velocity);

                AddDataToDiagramCreator();
                #endregion
            }
        }
        #endregion

        #region Simulation
        private DispatcherTimer timer = new DispatcherTimer();
        private Vector SimulatedPosition;
        private Vector SimulatedVelocity;
        private Vector LastSimulationTilt;
        private DateTime LastSimulation;
        private object lockobj = new object();
        void timer_Tick(object sender, EventArgs e)
        {
            MainWindow mainWindow = (BallOnTiltablePlate.JanRapp.MainApp.MainWindow)Application.Current.MainWindow;
            if (mainWindow != null)
                mainWindow.JugglerTimer();
            if (!this.SimulatedPosition.HasNaN() && !this.SimulatedVelocity.HasNaN())
            {
                //Simulation
                lock (lockobj)
                {
                    double phiX = LastSimulationTilt.X;
                    double phiY = LastSimulationTilt.Y;
                    double tanPhiX = Math.Tan(phiX);
                    double tanPhiY = Math.Tan(phiY);
                    double g = -9.81;
                    Vector Acceleration = new Vector(
                        (0.6e0 * g * tanPhiX) / (1 + tanPhiX * tanPhiX + tanPhiY * tanPhiY),
                        (0.6e0 * g * tanPhiY) / (1 + tanPhiX * tanPhiX + tanPhiY * tanPhiY));
                    double ElapsedSeconds = (DateTime.Now - LastSimulation).TotalSeconds;
                    SimulatedPosition += SimulatedVelocity * ElapsedSeconds + 0.5 * Acceleration * ElapsedSeconds * ElapsedSeconds;
                    SimulatedVelocity += Acceleration * ElapsedSeconds;
                    LastSimulation = DateTime.Now;
                }
                // PD Regeler

                Vector currentRelativePosition = this.TargetPosition - this.SimulatedPosition;
                Vector currentRelativeVelocity = this.TargetVelocity - this.SimulatedVelocity;

                var tilt =
                    currentRelativePosition
                        * this.PositionFactor.Value + //P
                    currentRelativeVelocity
                        * this.VelocityFactor.Value; //D

                this.SetTilt(-tilt);
                //Update State Observer if no Input
                if ((DateTime.Now - LastInput).TotalMilliseconds > SOUpdateTheshold.Value)
                {
                    this.Input_DataRecived(this, new BallInputEventArgs() { BallPosition = this.SimulatedPosition });
                }
                #region Display
                if (this.IsVisible)
                    History.FeedUpdate(SimulatedPosition, SimulatedVelocity);
                if (recording)
                {
                    diagramcreator.AddPoint("TiltX", new Point(time, tilt.X));
                    diagramcreator.AddPoint("TiltY", new Point(time, tilt.Y));
                }
                if (Container.IsVisible)
                {
                    Vector displayPos = GetDisplayPos(SimulatedPosition);
                    Canvas.SetLeft(NextPositionEllipse, displayPos.X);
                    Canvas.SetTop(NextPositionEllipse, displayPos.Y);

                    Vector targetDisplayPos = GetDisplayPos(TargetPosition);
                    Canvas.SetLeft(CurrentTargetPositionEllipse, targetDisplayPos.X);
                    Canvas.SetTop(CurrentTargetPositionEllipse, targetDisplayPos.Y);
                }
                #endregion
            }
        }
        #endregion
        private void SetTilt(Vector tilt)
        {
            this.LastSimulationTilt = GlobalSettings.Instance.ToValidTilt(tilt);
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
        #region UIHelper
        private Vector GetDisplayPos(Vector v)
        {
            Vector halfeArea = new Vector(nextPositionInput.Width / 2, nextPositionInput.Height / 2);
            v.X /= GlobalSettings.Instance.HalfPlateSize;
            v.Y /= GlobalSettings.Instance.HalfPlateSize;
            v.X *= halfeArea.X;
            v.Y *= halfeArea.Y;
            v += halfeArea;

            v.Y = nextPositionInput.Height - v.Y;

            return v;
        }
        #endregion

        private void SimulationTimer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timer.Interval = new TimeSpan(0, 0, 0, 0, (int)SimulationTimer.Value);
        }
    }
}
