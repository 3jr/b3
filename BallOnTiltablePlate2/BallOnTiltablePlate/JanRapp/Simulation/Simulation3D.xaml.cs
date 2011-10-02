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

namespace BallOnTiltablePlate.JanRapp.Simulation
{

    /// <summary>
    /// Interaction logic for Simulation3D.xaml
    /// </summary>
    [BallOnPlateItemInfo("Timo", "Schmetzer", "Simulation", "0.1")]
    public partial class Simulation3D : UserControl, IBallInput3D, IPlateOutput, IBallOnPlateItem, IPhysicsState
    {
        DispatcherTimer timer;
        TimoSchmetzer.Physics.Physics3D physics = new TimoSchmetzer.Physics.Physics3D();
        DateTime lastUpdateTime;

        public Simulation3D()
        {
            timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            InitializeComponent();

            timer.Tick += new EventHandler(timer_Tick);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            physics.RunPhysics(this, (now - lastUpdateTime).TotalSeconds);
            lastUpdateTime = now;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void SetTilt(Vector tilt)
        {
            DesiredTiltVecBox.SetValue(Controls.Vector2DControl.ValueProperty, tilt);
        }

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
                ToogelRunningBtn.Content = "Start";
            }
            else
            {
                lastUpdateTime = DateTime.Now;
                timer.Start();
                ToogelRunningBtn.Content = "Stop";
            }
        }

        #region IPhysicsState

        public Vector PlateVelocity
        {
            get { throw new NotImplementedException(); }
        }

        public Vector DesiredTilt
        {
            get
            {
                return (Vector)DesiredTiltVecBox.GetValue(Controls.Vector2DControl.ValueProperty);
            }
        }

        public double Gravity
        {
            get
            {
                return (double)GravityDoubleBox.GetValue(Controls.DoubleBox.ValueProperty);
            }
        }

        public double HitAttenuationFactor
        {
            get
            {
                return (double)HitAttenuationFactorDoubleBox.GetValue(Controls.DoubleBox.ValueProperty);
            }
        }

        public double AbsoluteAbsorbtion
        {
            get
            {
                return (double)AbsoluteHitAttenuationDoubleBox.GetValue(Controls.DoubleBox.ValueProperty);
            }
        }

        public Vector Tilt
        {
            get
            {
                return (Vector)TiltVecBox.GetValue(Controls.Vector2DControl.ValueProperty);
            }

            set
            {
                TiltVecBox.SetValue(Controls.Vector2DControl.ValueProperty, value);
            }
        }

        public Point3D Position
        {
            get
            {
                Vector3D vec = ((Vector3D)PositionVecBox.GetValue(Controls.Vector3DControl.ValueProperty));
                return new Point3D(vec.X, vec.Y, vec.Z);
            }
            set
            {
                PositionVecBox.SetValue(Controls.Vector3DControl.ValueProperty, new Vector3D(value.X, value.Y, value.Z));
            }
        }

        public Vector3D Velocity
        {
            get
            {
                return (Vector3D)VelocityVecBox.GetValue(Controls.Vector3DControl.ValueProperty);
            }
            set
            {
                VelocityVecBox.SetValue(Controls.Vector3DControl.ValueProperty, value);
            }
        }

        public Vector3D Acceleration
        {
            get
            {
                return (Vector3D)AccelerationVecBox.GetValue(Controls.Vector3DControl.ValueProperty);
            }
            set
            {
                AccelerationVecBox.SetValue(Controls.Vector3DControl.ValueProperty, value);
            }
        }

        private void TiltVecBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Vector> e)
        {
            Visualizer3DCtrl.PlateTilt = new Vector(Utilities.Vectors.MathUtil.RadToDeg(e.NewValue.X), Utilities.Vectors.MathUtil.RadToDeg(e.NewValue.Y));
        }

        private void PositionVecBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Vector3D> e)
        {
            //Vector3D vec = (Vector3D)e.NewValue;
            //Visualizer3DCtrl.BallPositon = new Point3D(vec.X, vec.Y, vec.Z);

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

    public interface IPhysicsState
    {
        #region Constats
        /// <summary>
        /// Contains the local Gravity in (m)/(s^2) (with a negative algebraic sign)
        /// </summary>
        double Gravity { get; } //= -9.81;

        /// <summary>
        /// Contains which part of the original hight may be achieved.
        /// Enthaellt, welchen anteil der Ausgangshoehe bei Reflektion wieder erreicht werden soll.
        /// </summary>
        double HitAttenuationFactor { get; } // = 1;

        Vector PlateVelocity { get; }

        /// <summary>
        /// Absolute Velocity Reduction on a hit.
        /// Absolute Geschwindigkeitsreduktion bei Aufprall
        /// </summary>
        double AbsoluteAbsorbtion { get; } //= 0.08;

        #endregion

        #region Status Properties
        /// <summary>
        /// Contains the rotaition of the plate against the axis in Rad.
        /// Enthaellt die Kippung der Platte als Vector in x/y Richtung in Radiant.
        /// </summary>
        Vector Tilt { get; set; }

        Vector DesiredTilt { get; }

        /// <summary>
        /// Contains the Position [of the Ball] as Vector3D.
        /// Enthaellt die Position [des Balls] als Vector3D.
        /// </summary>
        Point3D Position { get; set; }

        /// <summary>
        /// Contains the Velocity of the Ball as Vector3D.
        /// Enthaellt die Geschwindigkeit [des Balls] als Vector3D.
        /// </summary>
        Vector3D Velocity { get; set; }

        /// <summary>
        /// Contains the Acceleration [of the Ball] as Vector3D.
        /// Enthaellt die Beschleunigung [des Balls] als Vector3D.
        /// </summary>
        Vector3D Acceleration { get; set; }
        #endregion
    }
}