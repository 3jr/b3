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
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using BallOnTiltablePlate.JanRapp.Utilities;
using System.Reflection;

namespace BallOnTiltablePlate.TimoSchmetzer.Simulation
{
    /// <summary>
    /// Interaction logic for Simulation3D.xaml
    /// </summary>
    [BallOnPlateItemInfo("Timo", "Schmetzer", "BasicSimulation", "0.1")]
    public partial class BasicSimulation : UserControl, IBallInput3D, IPlateOutput, IBallOnPlateItem, ISimulationState
    {
        DispatcherTimer timer;
        DateTime lastUpdateTime;
        bool stopped = true;

        PhysicsWrapper wrapper = new PhysicsWrapper();

        public BasicSimulation()
        {
            timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            InitializeComponent();

            timer.Tick += new EventHandler(timer_Tick);

            IEnumerable<Type> Calculators = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && typeof(IPhysicsCalculator).IsAssignableFrom(t))
                .Select(t => t)
                .ToArray();
            foreach(Type t in Calculators)
            {
                TreeViewItem treeitem = new TreeViewItem();
                treeitem.Header = t.FullName;
                treeitem.Tag = Activator.CreateInstance(t);
                PhysicsCalculatorList.Items.Add(treeitem);
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            Update((now - lastUpdateTime).TotalSeconds);


            SendData((Vector3D)Position);

            BallOnTiltablePlate.JanRapp.MainApp.MainWindow mainWindow = (BallOnTiltablePlate.JanRapp.MainApp.MainWindow)Application.Current.MainWindow;
            if (mainWindow != null)
                mainWindow.JugglerTimer();

            lastUpdateTime = now;
        }

        public void Update(double deltaSeconds)
        {
            if (PhysicsCalculatorList.SelectedItem != null)
            {
                IPhysicsCalculator Calc = (IPhysicsCalculator)(((TreeViewItem)PhysicsCalculatorList.SelectedItem).Tag);
                wrapper.RunSimulation(Calc, this, deltaSeconds);
            }
        }

        public void Start()
        {
            stopped = false;
            ToogelRunningBtn.IsEnabled = true;
        }

        public void Stop()
        {
            stopped = true;
            timer.Stop();
            ToogelRunningBtn.Content = "Play";
            ToogelRunningBtn.IsEnabled = false;
        }

        public void SetTilt(Vector tilt)
        {
            tilt = tilt.ToNoNaN();
            DesiredTiltVecBox.SetValue(BallOnTiltablePlate.JanRapp.Controls.Vector2DControl.ValueProperty, tilt);
        }

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        private void ToogleRunningCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                ToogelRunningBtn.Content = "Play";
            }
            else
            {
                lastUpdateTime = DateTime.Now;
                timer.Start();
                ToogelRunningBtn.Content = "Pause";
            }
        }

        private void ToogleRunningCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!stopped)
                e.CanExecute = true;
        }

        #region SimulationState

        public Vector PlateVelocity
        {
            get
            {
                double d = PlateVelocityDoubleBox.Value;

                return new Vector(d, d);
            }
        }

        public Vector DesiredTilt
        {
            get
            {
                return DesiredTiltVecBox.Value;
            }
        }

        public double Gravity
        {
            get
            {
                return GravityDoubleBox.Value;
            }
        }

        public double HitAttenuationFactor
        {
            get
            {
                return HitAttenuationFactorDoubleBox.Value;
            }
        }

        public double AbsoluteAbsorbtion
        {
            get
            {
                return AbsoluteHitAttenuationDoubleBox.Value;
            }
        }

        public Vector Tilt
        {
            get
            {
                return TiltVecBox.Value;
            }

            set
            {
                TiltVecBox.Value = value;
            }
        }

        public Point3D Position
        {
            get
            {
                Vector3D vec = PositionVecBox.Value;
                return new Point3D(vec.X, vec.Y, vec.Z);
            }
            set
            {
                PositionVecBox.Value = new Vector3D(value.X, value.Y, value.Z);
            }
        }

        public Vector3D Velocity
        {
            get
            {
                return VelocityVecBox.Value;
            }
            set
            {
                VelocityVecBox.Value = value;
            }
        }

        public Vector3D Acceleration
        {
            get
            {
                return AccelerationVecBox.Value;
            }
            set
            {
                AccelerationVecBox.Value = value;
            }
        }

        private void TiltVecBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Vector> e)
        {
            Visualizer3DCtrl.PlateTilt = new Vector(e.NewValue.X, e.NewValue.Y);
        }

        private void PositionVecBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Vector3D> e)
        {
            Visualizer3DCtrl.BallPositon = (Point3D)e.NewValue;
        }
        #endregion

        #region Event
        public event EventHandler<BallInputEventArgs3D> DataRecived;

        EventHandler<BallInputEventArgs> DataRecived2D;
        event EventHandler<BallInputEventArgs> IBallInput.DataRecived
        {
            add { DataRecived2D += value; }
            remove { DataRecived2D -= value; }
        }

        private void SendData(Vector3D vec)
        {
            var args = new BallInputEventArgs3D() { BallPosition3D = vec };
            args.BallPosition = new Vector(vec.X, vec.Y);

            if (DataRecived != null)
                DataRecived(this, args);
            if (DataRecived2D != null)
                DataRecived2D(this, args);
        }
        #endregion

        private void FpsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timer.Interval = new TimeSpan((long)(1000000 / FpsSlider.Value));
        }
    }
}
