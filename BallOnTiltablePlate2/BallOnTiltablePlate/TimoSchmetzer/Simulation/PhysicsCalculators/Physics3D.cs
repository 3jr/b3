using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;
using System.Diagnostics;

namespace BallOnTiltablePlate.TimoSchmetzer.Simulation.PhysicsCalculators
{
    /// <summary>
    /// Class, that does Physics cacultations.
    /// Author: Timo Schmetzer
    /// Version 0.1
    /// </summary>
    public class Physics3D : IPhysicsCalculator
    {
        /// <summary>
        /// Calculates the New State of the Ball given by the current IPhysicsState.
        /// </summary>
        /// <param name="current">Current Physics State</param>
        /// <param name="elapsedSeconds">Seconds to elapse</param>
        /// <returns>State after elapsedSeconds</returns>
        public void CalcPhysics(IPhysicsState state, double elapsedSeconds)
        {
            #region stuff
            //Sinnlose aufrufe vermeiden
            if (elapsedSeconds == 0) { return; }
            //Vektor G festlegen
            Vector3D G = new Vector3D(0, 0, state.Gravity);
            #endregion

            #region CalcBallstate
            BallState bs;
            if (Math.Abs(state.Position.Z - Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt))) > 0.1)
            {
                bs = BallState.InAir;
            }
            else
            {
                if (Utilities.Physics.WouldHit(state))
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
            #region RollOnPlate
            if (bs == BallState.RollOnPlate)
            {
                Vector3D vN = Physics.CalcNormalPart(state.Velocity, state.Tilt);
                state.Velocity -= vN;
                Point3D newPlatePosition = Mathematics.TransformVectorToDifferentTilt(state.Position, Mathematics.ToSequentialTilt(state.Tilt), Mathematics.ToSequentialTilt(state.Tilt + elapsedSeconds * state.PlateVelocity));
                Vector3D deltas = (newPlatePosition - state.Position);
                state.Acceleration = (2.0 * (deltas - vN*elapsedSeconds)) / (elapsedSeconds * elapsedSeconds);
                state.Tilt += elapsedSeconds * state.PlateVelocity;
                state.Acceleration += Utilities.Physics.HangabtriebsbeschleunigungBerechnen(state.Gravity, state.Tilt);
                Utilities.Physics.CalcMovement(state, elapsedSeconds);
            }
            #endregion
            #region InAir
            if (bs == BallState.InAir)
            {
                #region SetNEWAngle
                state.Tilt += elapsedSeconds * state.PlateVelocity;
                #endregion
                //Beschleunigung zur Hit-Berechnug setzen
                state.Acceleration = G;
                double nextHit = Utilities.Physics.CalcNextHit(state);
                if (Mathematics.IsDownPlate(state.Position, Mathematics.CalcNormalVector(state.Tilt)) && (!Physics.AlreadyReflected(state)))
                {
                     state = Utilities.Physics.Reflect(state);
                }
                Utilities.Physics.CalcMovement(state, elapsedSeconds);
                 
            }
            #endregion
            #endregion
            #region Debug
            BasicSimulation.WriteToDiagram("hdist", Math.Abs(state.Position.Z - Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt))));
            if (bs == BallState.RollOnPlate)
            { BasicSimulation.WriteToDiagram("Ballstate", 0); }
            else { BasicSimulation.WriteToDiagram("Ballstate", 1); }
            BasicSimulation.WriteToDiagram("wouldhit", Utilities.Physics.WouldHit(state) ? 1 : 0);
            BasicSimulation.WriteToDiagram("angle", Utilities.Mathematics.AngleBetwennVectors(state.Velocity,Mathematics.CalcNormalVector(state.Tilt)));
            BasicSimulation.WriteToDiagram("vN", Physics.CalcNormalPart(state.Velocity, state.Tilt).Length);
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

    }
}
