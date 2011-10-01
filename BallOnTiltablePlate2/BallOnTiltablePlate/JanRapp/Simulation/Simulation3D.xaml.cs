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
    [BallOnPlateItemInfo("Timo","Schmetzer","Simulation", "0.1")]
    public partial class Simulation3D : UserControl, IBallInput3D, IPlateOutput, IBallOnPlateItem
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
            throw new NotImplementedException();
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
        Vector Tilt { get; set; }

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

        /// <summary>
        /// Run Physics gives Back the State of the System at the Time AbsoluteTime+s.SecondsToElapse
        /// equals parameter elapsedSeconds in older Versions.
        /// </summary>
        double SecondsToElapse { get; }
    }
}
