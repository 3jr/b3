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
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BallOnTiltablePlate.JanRapp.Simulation;

namespace BallOnTiltablePlate.TimoSchmetzer.Preprocessor
{
    /// <summary>
    /// Interaction logic for Preprocessor.xaml
    /// </summary>
    [BallOnPlateItemInfo("Timo", "Schmetzer", "SimulationTestPreprocessor", "0.1")]
    public partial class SimulationTestPreprocessor : UserControl, IPreprocessor, IPreprocessorIO<SimulationBase, SimulationBase>
    {
        #region Base
        public System.Windows.FrameworkElement SettingsUI
        {
            get { return null; }
        }
        #endregion

        public SimulationTestPreprocessor()
        {
            InitializeComponent();
        }

        public SimulationBase Input { private get; set; }
        public SimulationBase Output { private get; set; }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public ValueSavePhyicsState state
        {
            get
            {
                ValueSavePhyicsState state = new ValueSavePhyicsState();
                state.AbsoluteAbsorbtion = Input.AbsoluteAbsorbtion;
                state.Acceleration = Input.Acceleration;
                state.DesiredTilt = Input.DesiredTilt;
                state.Gravity = Input.Gravity;
                state.HitAttenuationFactor = Input.HitAttenuationFactor;
                state.PlateVelocity = Input.PlateVelocity;
                state.Position = Input.Position;
                state.Tilt = Input.Tilt;
                state.Velocity = Input.Velocity;
                return state;
            }
        }

        public Vector Tilt
        {
            set { Output.SetTilt(value); }
        }
    }
    public class ValueSavePhyicsState : IPhysicsState
    {
        private Vector _PlateVelocity;
        public Vector PlateVelocity
        {
            get
            {
                return _PlateVelocity;
            }
            set
            { _PlateVelocity = value; }
        }

        private Vector _DesiredTilt;
        public Vector DesiredTilt
        {
            get
            {
                return _DesiredTilt;
            }
            set { _DesiredTilt = value; }
        }

        private double _Gravity;
        public double Gravity
        {
            get
            {
                return _Gravity;
            }
            set
            { _Gravity = value; }
        }

        private double _HitAttenuationFactor;
        public double HitAttenuationFactor
        {
            get
            {
                return _HitAttenuationFactor;
            }
            set { _HitAttenuationFactor = value; }
        }

        private double _AbsoluteAbsorbtion;
        public double AbsoluteAbsorbtion
        {
            get
            {
                return _AbsoluteAbsorbtion;
            }
            set
            { _AbsoluteAbsorbtion = value; }
        }

        private Vector _Tilt;
        public Vector Tilt
        {
            get
            {
                return _Tilt;
            }
            set
            {
                _Tilt = value;
            }
        }

        private Point3D _Position;
        public Point3D Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = value;
            }
        }

        private Vector3D _Velocity;
        public Vector3D Velocity
        {
            get
            {
                return _Velocity;
            }
            set
            {
                _Velocity = value;
            }
        }

        private Vector3D _Acceleration;
        public Vector3D Acceleration
        {
            get
            {
                return _Acceleration;
            }
            set
            {
                _Acceleration = value;
            }
        }

        public ValueSavePhyicsState Clone()
        {
            return (ValueSavePhyicsState)this.MemberwiseClone();
        }

    }
}
