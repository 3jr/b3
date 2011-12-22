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
using BallOnTiltablePlate.TimoSchmetzer.Utilities;

namespace BallOnTiltablePlate.TimoSchmetzer.Physics
{
    /// <summary>
    /// Class, that does Physics cacultations.
    /// </summary>
    public class Physics3D
    {
        [Obsolete("Please use RunSimulation in PhysicSimulation3D.",false)]
        public void RunPhysics(IPhysicsState state, double elapsedSeconds)
        {
            //Temporary Redicect
            PhysicSimulation3D.RunSimulation(state, elapsedSeconds);
        }

        /// <summary>
        /// Calculates the New State of the Ball given by the current IPhysicsState.
        /// </summary>
        /// <param name="current">Current Physics State</param>
        /// <param name="elapsedSeconds">Seconds to elapse</param>
        /// <returns>State after elapsedSeconds</returns>
        public static void CalcPhysics(PhysicsState state, double elapsedSeconds)
        {
            #region stuff
            //Sinnlose aufrufe vermeiden
            if (elapsedSeconds == 0) { return; }
            //Vektor G festlegen
            Vector3D G = new Vector3D(0, 0, state.Gravity);
            #endregion

            #region SetNEWAngle
            state.Tilt += elapsedSeconds * state.PlateVelocity;
            #endregion

            #region CalcBallstate
            BallState bs;
            if (Utilities.Physics.IsHit(state))
            {
                bs = BallState.InAir;
            }
            else
            {
                if (Math.Abs(state.Position.Z - Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt))) > 0.01)
                {
                    bs = BallState.InAir;
                }
                else
                {
                    bs = BallState.RollOnPlate;
                }
            }
            #endregion

            #region CalcMovement
            if (bs == BallState.RollOnPlate)
            {
                state.Acceleration = Utilities.Physics.HangabtriebskraftBerechnen(state.Gravity, state.Tilt);
                double deltahight = Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt))
                    - Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt - elapsedSeconds*state.PlateVelocity));
                if (deltahight > 0)
                {
                    state.Acceleration += state.CentrifugalFactor * deltahight * Mathematics.CalcNormalVector(state.Tilt); 
                }
                CalcMovement(state, elapsedSeconds);
            }
            if (bs == BallState.InAir)
            {
                //Beschleunigung zur Hit-Berechnug setzen
                state.Acceleration = G;
                double nextHit = Utilities.Physics.CalcNextHit(state);

                if (nextHit > elapsedSeconds)
                {
                    //Ball rollt nicht auf der Platte. Hit nicht mehr in diesem Update.
                    CalcMovement(state, elapsedSeconds);
                }
                else
                {
                    //Hit in diesem Update.
                    CalcMovement(state, nextHit);
                    state = Utilities.Physics.Reflect(state);
                    CalcMovement(state, elapsedSeconds - nextHit);
                }
            }
            #endregion

            #region Debugout
            //System.Diagnostics.Debug.Print(state.Tilt.ToString());
            #endregion
        }

        /// <summary>
        /// Represents the Status of the Ball.
        /// Enum, die Ausdruekten soll, in welchen Zustand sich der Ball befindet.
        /// </summary>
        private enum BallState
        {
            InAir = 0,
            RollOnPlate = 1
        };

        /// <summary>
        /// Calculates Movement using s = 0.5att + v0t + s0, v= ...
        /// </summary>
        private static void CalcMovement(PhysicsState state, double ElapsedSeconds)
        {
            //Bewegungsgleichung
            state.Position += state.Velocity * ElapsedSeconds + 0.5 * state.Acceleration * ElapsedSeconds * ElapsedSeconds;
            state.Velocity += state.Acceleration * ElapsedSeconds;
        }
    }
}
