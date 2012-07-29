using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for BalancePreprocessor2SO.xaml
    /// </summary>
    [ControledSystemModuleInfo("Jan", "Rapp", "Balance Preprocessor", "1.8")]
    public partial class BalancePreprocessor2SO : UserControl,
        IControledSystemPreprocessorIO<IBallInput, IPlateOutput>,
        IBalancePreprocessor, IBasicPreprocessor, IControledSystemPreprocessor
    {
        public BalancePreprocessor2SO()
        {
            InitializeComponent();
        }

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

        #region Interface Properties and Fieldswe
        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        IBallInput input;
        public IBallInput Input
        {
            set { input = value; }
        }

        IPlateOutput output;
        public IPlateOutput Output
        {
            set { output = value; }
        }

        Vector targetPositon;
        public Vector TargetPosition
        {
            get
            {
                return targetPositon;
            }
            set
            {
                targetPositon = value;
            }
        }

        bool isAutoBalancing;
        public bool IsAutoBalancing
        {
            get
            {
                return isAutoBalancing;
            }
            set
            {
                isAutoBalancing = true;
            }
        }

        public Vector Position { get; private set; }

        public Vector Velocity { get; private set; }

        public bool ValuesValid { get; private set; }
        #endregion

        #region Interface Methods
        public void Start()
        {
            input.DataRecived += input_DataRecived;
            ReinitialiceStateObservers();
        }

        public void Stop()
        {
            input.DataRecived -= input_DataRecived;
        }

        void IBasicPreprocessor.SetTilt(Vector tiltToAxis)
        {
            this.isAutoBalancing = false;
            this.SetTilt(tiltToAxis);
        }
        #endregion

        StateObserver SoX, SoY;
        private Vector lastTilt;
        private Vector integral;

        void input_DataRecived(object sender, BallInputEventArgs e)
        {
            Vector newBallPos = e.BallPosition;

            if (this.Position.HasNaN() && !newBallPos.HasNaN())
            {
                this.SoX.xh[0] = newBallPos.X;
                this.SoX.xh[1] = 0;
                this.SoY.xh[0] = newBallPos.Y;
                this.SoY.xh[1] = 0;
            }

            double deltaTime = StaticPeriod.Value;

            if (!newBallPos.HasNaN())
            {
                this.SoX.NextStep(newBallPos.X, lastTilt.X, deltaTime);
                this.SoY.NextStep(newBallPos.Y, lastTilt.Y, deltaTime);

                this.Velocity = new Vector(this.SoX.xh[1], SoY.xh[1]);
                this.Position = newBallPos; // new Vector(this.SoX.xh[0], SoY.xh[0]);
            }
            else 
            {
                this.Velocity = VectorUtil.NaNVector;
                this.Position = VectorUtil.NaNVector;
            }


            this.ValuesValid = !this.Position.HasNaN() && !this.Velocity.HasNaN();

            this.PositionDisplay.Text = this.Position.ToString();
            this.VelocityDisplay.Text = this.Velocity.ToString();
            this.DeltaTimeDisplay.Text = deltaTime.ToString();

            if (this.IsVisible)
                History.FeedUpdate(Position, Velocity);

            AddDataToDiagramCreator();
            if (this.IsAutoBalancing)
            {
                if (this.ValuesValid)
                {
                    Vector currentRelativePosition = this.Position -this.TargetPosition;
                    integral += currentRelativePosition * deltaTime;
                    var tilt = currentRelativePosition * this.PositionFactor.Value +
                        integral * this.IntegralFactor.Value +
                        this.Velocity * this.VelocityFactor.Value;

                    this.IntegralDisplay.Text = "Integral: " + integral;

                    this.SetTilt(tilt);
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
            double g = Gravity.Value;
            double l1 = LFactor.Value.X;
            double l2 = LFactor.Value.Y;

            var A = new double[,]{
               {0,  1,  },
               {0,  0,  },
            };

            var B = new double[,]{
               {0,  },
               {-g,  },
            };

            var C = new double[,]{
               {1,  0,  },
            };

            var L = new double[,]{
               {l1,  },
               {l2,  },
            };

            SoX = new StateObserver(A, B, C, L, new double[] { 0, 0, });
            SoY = new StateObserver(A, B, C, L, new double[] { 0, 0, });
        }

        private void Reset()
        {
            ReinitialiceStateObservers();
            integral = new Vector();

        }

        private void SetTilt(Vector tilt)
        {
            this.lastTilt = tilt;
            this.output.SetTilt(tilt);
        }

        #region UI Events
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void SoLFactor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Vector> e)
        {
            ReinitialiceStateObservers();
        }

        private void SoGFactor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ReinitialiceStateObservers();
        }
        #endregion
    }
}