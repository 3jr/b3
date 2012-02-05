using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Media3D;
using System.Windows;

namespace BallOnTiltablePlate.TimoSchmetzer
{
    class Test
    {
    }
    class PhysicSimulationInAir
    {
        public static void RunSimulation(Object o, double deltaSeconds) { }
    }
    class PhysicSimulation3D
    {
        public static void RunSimulation(Object o, double deltaSeconds) { }
    }
    class PhysicSimulationOnPlate
    {
        public static void RunSimulation(BallOnTiltablePlate.JanRapp.Simulation.IPhysicsState o, double deltaSeconds) {
            BallOnTiltablePlate.TimoSchmetzer.Simulation.PhysicsWrapper wrapper = new BallOnTiltablePlate.TimoSchmetzer.Simulation.PhysicsWrapper();
            BallOnTiltablePlate.TimoSchmetzer.Simulation.PhysicsCalculators.PhysicsOnPlate calc = new BallOnTiltablePlate.TimoSchmetzer.Simulation.PhysicsCalculators.PhysicsOnPlate();
            TemporaryPhysicsState state = new TemporaryPhysicsState();
            state.AbsoluteAbsorbtion = o.AbsoluteAbsorbtion;
            state.Acceleration = o.Acceleration;
            state.DesiredTilt = o.DesiredTilt;
            state.Gravity = o.Gravity;
            state.HitAttenuationFactor = o.HitAttenuationFactor;
            state.PlateVelocity = o.PlateVelocity;
            state.Position = o.Position;
            state.Tilt = o.Tilt;
            state.Velocity = o.Velocity;
            wrapper.RunSimulation(calc, state, deltaSeconds);
            o.Acceleration = state.Acceleration;
            o.Position = state.Position;
            o.Tilt = state.Tilt;
            o.Velocity = state.Velocity;
        }
    }

    class TemporaryPhysicsState : BallOnTiltablePlate.TimoSchmetzer.Simulation.ISimulationState
    {
        #region PhysicsState

        #region Constants
        private double _Gravity;
        public double Gravity
        {
            get
            {
                return _Gravity;
            }
            set
            {
                _Gravity = value;
            }
        }

        private double _HitAttenuationFactor;
        public double HitAttenuationFactor
        {
            get
            {
                return _HitAttenuationFactor;
            }
            set
            {
                _HitAttenuationFactor = value;
            }
        }

        private double _AbsoluteAbsorbtion;
        public double AbsoluteAbsorbtion
        {
            get
            {
                return _AbsoluteAbsorbtion;
            }
            set
            {
                _AbsoluteAbsorbtion = value;
            }
        }
        #endregion

        #region Status Properties
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

        private Vector _DesiredTilt;
        public Vector DesiredTilt
        {
            get
            {
                return _DesiredTilt;
            }

            set
            {
                _DesiredTilt = value;
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

        private Vector _PlateVelocity;
        public Vector PlateVelocity
        {
            get
            {
                return _PlateVelocity;
            }
            set
            {
                _PlateVelocity = value;
            }
        }
        #endregion

        #endregion
    }
}
