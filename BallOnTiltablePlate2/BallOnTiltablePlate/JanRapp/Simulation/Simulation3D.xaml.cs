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

namespace BallOnTiltablePlate.JanRapp.Simulation
{
    //[BallOnPlateItemInfo("_Timo", "Schmetzer", "Simulation", "0.1")]
    //public class SimulationWrapper : IBallInput3D, IPlateOutput
    //{
    //    #region Base
    //    public System.Windows.FrameworkElement SettingsUI
    //    {
    //        get { return new FrameworkElement(); }
    //    }
    //    #endregion
    //    private volatile bool running;
    //    private volatile int TiltX;
    //    private volatile int TiltY;

    //    public void Start()
    //    {
    //        running = true;
    //        DateTime oldtime = DateTime.Now;
    //        PhysicsState state = new PhysicsState();
    //        Physics3D physics = new Physics3D();
    //        while (running)
    //        {
    //            DateTime newtime = DateTime.Now;
    //            state.SecondsToElapse = ((double)(newtime - oldtime).Ticks)/
    //                (double)(TimeSpan.TicksPerSecond);
    //            oldtime += (newtime - oldtime);
    //            //TODO: Input of UiModifiying of Fields.
    //            state.Tilt = new Vector(TiltX/10000,TiltY/10000);
    //            physics.RunPhysics(state);
    //            SendData((Vector3D)state.Position);
    //        }
    //    }

    //    public void Stop()
    //    {
    //        running = false;
    //    }

    //    public void SetTilt(Vector tilt)
    //    {
    //        TiltX = (int)tilt.X*10000;
    //        TiltY = (int)tilt.Y*10000;
    //    }

    //    public event EventHandler<BallInputEventArgs3D> DataRecived;
    //    EventHandler<BallInputEventArgs> DataRecived2D;
    //    event EventHandler<BallInputEventArgs> IBallInput.DataRecived
    //    {
    //        add { DataRecived2D += value; }
    //        remove { DataRecived2D -= value; }
    //    }

    //    private void SendData(Vector3D vec)
    //    {
    //        var args = new BallInputEventArgs3D() { BallPosition3D = vec };
    //        args.BallPosition = new Vector(vec.X, vec.Y);

    //        if (DataRecived != null)
    //            DataRecived(this, args);
    //        if (DataRecived2D != null)
    //            DataRecived2D(this, args);
    //    }
    //}

    /// <summary>
    /// Interaction logic for Simulation3D.xaml
    /// </summary>
    [BallOnPlateItemInfo("Timo", "Schmetzer", "Simulation", "0.1")]
    public partial class Simulation3D : UserControl, IBallInput3D, IPlateOutput, IBallOnPlateItem, IPhysicsState
    {
        public Simulation3D()
        {
            InitializeComponent();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void SetTilt(Vector tilt)
        {
            TiltVecBox.SetValue(Controls.Vector2DControl.ValueProperty, tilt);
        }

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

        public FrameworkElement SettingsUI
        {
            get { return this; }
        }

        public double g
        {
            get
            {
                return (double)GravityDoubleBox.GetValue(Controls.DoubleBox.ValueProperty);
            }
        }

        public double HightFactor
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
        }

        public Point3D Position
        {
            get
            {
                return (Point3D)PositionVecBox.GetValue(Controls.Vector3DControl.ValueProperty);
            }
            set
            {
                PositionVecBox.SetValue(Controls.Vector3DControl.ValueProperty, value);
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
            Visualizer3DCtrl.PlateTilt = e.NewValue;
        }

        private void PositionVecBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<Vector3D> e)
        {
            Visualizer3DCtrl.BallPositon = (Point3D)e.NewValue;
        }
    }

    public interface IPhysicsState
    {
        #region Constats
        /// <summary>
        /// Contains the local Gravity in (m)/(s^2) (with a negative algebraic sign)
        /// </summary>
        double g { get; } //= -9.81;

        /// <summary>
        /// Contains which part of the original hight may be achieved.
        /// Enthaellt, welchen anteil der Ausgangshoehe bei Reflektion wieder erreicht werden soll.
        /// </summary>
        double HightFactor { get; } // = 1;

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
        Vector Tilt { get; }

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
