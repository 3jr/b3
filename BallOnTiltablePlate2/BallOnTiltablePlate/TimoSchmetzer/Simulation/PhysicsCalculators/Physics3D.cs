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
            bs = BallState.RollOnPlate;
            #region CalcMovement
            #region RollOnPlate
            if (bs == BallState.RollOnPlate)
            {
                Vector3D vN = Physics.CalcNormalPart(state.Velocity, state.Tilt);
                state.Velocity -= vN;
                try
                {
                    BasicSimulation.WriteToDiagram("vN", ((double)Math.Sign(vN.X / Mathematics.CalcNormalVector(state.Tilt).X)) * vN.Length);
                }
                catch(Exception){}
                //BasicSimulation.WriteToDiagram("test01",Vector3D.DotProduct(state.Velocity,Mathematics.CalcNormalVector(state.Tilt)));
                //Point3D newPlatePosition = Mathematics.TransformVectorToDifferentTilt(state.Position, Mathematics.ToSequentialTilt(state.Tilt), Mathematics.ToSequentialTilt(state.Tilt + elapsedSeconds * state.PlateVelocity));
                Vector3D newNormal = Mathematics.CalcNormalVector(state.Tilt + elapsedSeconds * state.PlateVelocity);
                Point3D newPlatePosition = state.Position + newNormal * -1 * (Vector3D.DotProduct(newNormal, (Vector3D)state.Position) / Vector3D.DotProduct(newNormal, newNormal));
                Vector3D deltas = (newPlatePosition - state.Position);
                Vector3D accn = (2.0 * (deltas - vN*elapsedSeconds)) / (elapsedSeconds * elapsedSeconds);
                state.Acceleration = (accn.Z > 0 ? accn : new Vector3D(0, 0, 0));
                BasicSimulation.WriteToDiagram("accwhZ", state.Acceleration.Z);
                BasicSimulation.WriteToDiagram("accwhX", state.Acceleration.X);
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
            BasicSimulation.WriteToDiagram("hdist", state.Position.Z - Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt)));
            BasicSimulation.WriteToDiagram("Ballstate", bs == BallState.RollOnPlate ? 0 : 1);
            //BasicSimulation.WriteToDiagram("wouldhit", Utilities.Physics.WouldHit(state) ? 1 : 0);
            BasicSimulation.WriteToDiagram("angle", Utilities.Mathematics.AngleBetwennVectors(state.Velocity,Mathematics.CalcNormalVector(state.Tilt)));
            BasicSimulation.WriteToDiagram("under", 1.396263401595464);
            BasicSimulation.WriteToDiagram("upper", 1.745329251994329);
            BasicSimulation.WriteToDiagram("newPosX", Mathematics.TransformVectorToDifferentTilt(state.Position, Mathematics.ToSequentialTilt(state.Tilt), Mathematics.ToSequentialTilt(state.Tilt + elapsedSeconds * state.PlateVelocity)).X);
            BasicSimulation.WriteToDiagram("newPosY", Mathematics.TransformVectorToDifferentTilt(state.Position, Mathematics.ToSequentialTilt(state.Tilt), Mathematics.ToSequentialTilt(state.Tilt + elapsedSeconds * state.PlateVelocity)).Y);
            BasicSimulation.WriteToDiagram("newPosZ", Mathematics.TransformVectorToDifferentTilt(state.Position, Mathematics.ToSequentialTilt(state.Tilt), Mathematics.ToSequentialTilt(state.Tilt + elapsedSeconds * state.PlateVelocity)).Z);
            #endregion
        }

        /// <summary>
        /// Represents the Status of the Ball.
        /// Enum, die Ausdrueckten soll, in welchen Zustand sich der Ball befindet.
        /// </summary>
        private enum BallState
        {
            InAir = 0,
            RollOnPlate = 1
        };

    }
}
