using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BallOnTiltablePlate.TimoSchmetzer.Simulation
{
    public class PhysicsWrapper : PhysicsState
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
        }

        private double _HitAttenuationFactor;
        public double HitAttenuationFactor
        {
            get
            {
                return _HitAttenuationFactor;
            }
        }

        private double _AbsoluteAbsorbtion;
        public double AbsoluteAbsorbtion
        {
            get
            {
                return _AbsoluteAbsorbtion;
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
        }
        #endregion

        #endregion

        /// <summary>
        /// Does interaction between PhysicsCalculators who only calc the Physical change on given Data
        /// and the Simulation who has Desired Parameters and Constants.
        /// </summary>
        /// <param name="Calculator">Physics Calculator to use for Calculations</param>
        /// <param name="Istate"></param>
        /// <param name="elapsedSeconds"></param>
        public void RunSimulation(PhysicsCalculator Calculator, SimulationState Istate, double elapsedSeconds)
        {
            #region calccalltimes
            double tcallx = (Istate.DesiredTilt.X - Istate.Tilt.X) / (Istate.PlateVelocity.X);
            double tcally = (Istate.DesiredTilt.Y - Istate.Tilt.Y) / (Istate.PlateVelocity.Y);
            double signx = Math.Sign(tcallx); 
            double signy = Math.Sign(tcally); 
            tcallx = Math.Abs(tcallx);
            tcally = Math.Abs(tcally);
            tcallx = tcallx > elapsedSeconds ? elapsedSeconds : tcallx;
            tcally = tcally > elapsedSeconds ? elapsedSeconds : tcally;
            #endregion
            #region createstate
            this._AbsoluteAbsorbtion = Istate.AbsoluteAbsorbtion;
            this._Acceleration = Istate.Acceleration;
            this._Gravity = Istate.Gravity;
            this._HitAttenuationFactor = Istate.HitAttenuationFactor;
            this._Position = Istate.Position;
            this._Tilt = Istate.Tilt;
            this._Velocity = Istate.Velocity;
            #endregion
            #region createcalcs
            if (tcallx >= tcally)
            {
                this._PlateVelocity = new Vector(signx * Istate.PlateVelocity.X, signy * Istate.PlateVelocity.Y);
                Calculator.CalcPhysics(this, tcally);
                this._PlateVelocity = new Vector(signx * Istate.PlateVelocity.X, 0);
                Calculator.CalcPhysics(this, tcallx - tcally);
                this._PlateVelocity = new Vector(0, 0);
                Calculator.CalcPhysics(this, elapsedSeconds - tcallx);
            }
            else
            {
                this._PlateVelocity = new Vector(signx * Istate.PlateVelocity.X, signy * Istate.PlateVelocity.Y);
                Calculator.CalcPhysics(this, tcallx);
                this._PlateVelocity = new Vector(0, signy * Istate.PlateVelocity.Y);
                Calculator.CalcPhysics(this, tcally - tcallx);
                this._PlateVelocity = new Vector(0, 0);
                Calculator.CalcPhysics(this, elapsedSeconds - tcally);
            }
            #endregion
            #region writeresultstoIstate
            Istate.Acceleration = this._Acceleration;
            Istate.Position = this._Position;
            Istate.Velocity = this._Velocity;
            Istate.Tilt = this._Tilt;
            #endregion
        }
    }
}
