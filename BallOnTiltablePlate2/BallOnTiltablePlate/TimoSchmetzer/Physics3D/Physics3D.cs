using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using BallOnTiltablePlate.JanRapp.Simulation;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;

namespace BallOnTiltablePlate.TimoSchmetzer.Physics
{
    /// <summary>
    /// Class, that does Physics cacultations.
    /// </summary>
    public class Physics3D
    {
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

            #region CalcBallstate
            BallState bs;
            if (Math.Abs(state.Position.Z - Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt))) > 0.01)
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

            #region SetNEWAngle
            state.Tilt += elapsedSeconds * state.PlateVelocity;
            #endregion

            #region CalcMovement
            if (bs == BallState.RollOnPlate)
            {
                state.Acceleration = Utilities.Physics.HangabtriebskraftBerechnen(state.Gravity, state.Tilt);
                #region CalcPseudoConstainingForce
                {
                    if (Utilities.Mathematics.IsDownPlate(state.Position,Utilities.Mathematics.CalcNormalVector(state.Tilt)))
                    {
                        state.Acceleration +=
                            (2.0 * (Utilities.Mathematics.CalcFootOfPerpendicular(state.Position, Utilities.Mathematics.CalcNormalVector(state.Tilt))-state.Position))
                            / (elapsedSeconds * elapsedSeconds);
                    }

                }
                #endregion
                Utilities.Physics.CalcMovement(state, elapsedSeconds);

            }
            if (bs == BallState.InAir)
            {
                //Beschleunigung zur Hit-Berechnug setzen
                state.Acceleration = G;
                double nextHit = Utilities.Physics.CalcNextHit(state);

                if (nextHit > elapsedSeconds)
                {
                    //Ball rollt nicht auf der Platte. Hit nicht mehr in diesem Update.
                    Utilities.Physics.CalcMovement(state, elapsedSeconds);
                }
                else
                {
                    //Hit in diesem Update.
                    Utilities.Physics.CalcMovement(state, nextHit);
                     state = Utilities.Physics.Reflect(state);
                    CalcPhysics(state, elapsedSeconds - nextHit);
                }
            }
            #endregion

            #region Debugout
            //(Nr.)	PositionX	PositionY	PositionZ	VelociyX	VelocityY	VelocityZ	AccelerationX	AccelerationY	AccelerationZ	BallState	AngleBetweenVec	elapsedSec	TiltX	TiltY	PlateVelX	PlateVelY   DeltaZBallPlate
            //System.Diagnostics.Debug.Print(state.Position.X + "\t" + state.Position.Y + "\t" + state.Position.Z + "\t" +
            //    state.Velocity.X + "\t" + state.Velocity.Y + "\t" + state.Velocity.Z + "\t"
            //    + state.Acceleration.X + "\t" + state.Acceleration.Y + "\t" + state.Acceleration.Z + "\t" + bs
            //    + "\t" + BallOnTiltablePlate.TimoSchmetzer.Utilities.Mathematics.AngleBetwennVectors(BallOnTiltablePlate.TimoSchmetzer.Utilities.Mathematics.CalcNormalVector(state.Tilt), state.Velocity)
            //    + "\t" + elapsedSeconds + "\t" + state.Tilt.X + "\t" + state.Tilt.Y + "\t" + state.PlateVelocity.X
            //     + "\t" + state.PlateVelocity.Y + "\t" + (state.Position.Z - Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt))));
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
