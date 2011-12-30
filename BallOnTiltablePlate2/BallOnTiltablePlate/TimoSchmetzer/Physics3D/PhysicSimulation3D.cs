using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using BallOnTiltablePlate.JanRapp.Utilities.Vectors;
using BallOnTiltablePlate.JanRapp.Simulation;

namespace BallOnTiltablePlate.TimoSchmetzer
{
    public class PhysicSimulation3D
    {
        #region info
        public String Author = "Timo Schmetzer";
        public Version Version = new Version(0, 6);
        public String Comment = "Numerical Simulation";
        #endregion

        public static void RunSimulation(IPhysicsState Istate, double elapsedSeconds)
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
            Physics.PhysicsState state = new Physics.PhysicsState();
            state.AbsoluteAbsorbtion = Istate.AbsoluteAbsorbtion;
            state.Acceleration = Istate.Acceleration;
            state.Gravity = Istate.Gravity;
            state.HitAttenuationFactor = Istate.HitAttenuationFactor;
            state.Position = Istate.Position;
            state.Tilt = Istate.Tilt;
            state.Velocity = Istate.Velocity;
            #endregion
            #region createcalcs
            if (tcallx >= tcally)
            {
                state.PlateVelocity = new Vector(signx*Istate.PlateVelocity.X, signy*Istate.PlateVelocity.Y);
                Physics.Physics3D.CalcPhysics(state, tcally);
                state.PlateVelocity = new Vector(signx*Istate.PlateVelocity.X, 0);
                Physics.Physics3D.CalcPhysics(state, tcallx - tcally);
                state.PlateVelocity = new Vector(0, 0);
                Physics.Physics3D.CalcPhysics(state, elapsedSeconds - tcallx);
            }
            else
            {
                state.PlateVelocity = new Vector(signx * Istate.PlateVelocity.X, signy * Istate.PlateVelocity.Y);
                Physics.Physics3D.CalcPhysics(state, tcallx);
                state.PlateVelocity = new Vector(0, signy * Istate.PlateVelocity.Y);
                Physics.Physics3D.CalcPhysics(state, tcally - tcallx);
                state.PlateVelocity = new Vector(0, 0);
                Physics.Physics3D.CalcPhysics(state, elapsedSeconds - tcally);
            }
            #endregion
            #region writeresultstoIstate
            Istate.Acceleration = state.Acceleration;
            Istate.Position = state.Position;
            Istate.Velocity = state.Velocity;
            Istate.Tilt = state.Tilt;
            #endregion
        }
    }
}
