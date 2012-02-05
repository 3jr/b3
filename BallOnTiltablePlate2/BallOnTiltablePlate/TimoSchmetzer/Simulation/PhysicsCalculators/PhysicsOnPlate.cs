﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;

namespace BallOnTiltablePlate.TimoSchmetzer.Simulation.PhysicsCalculators
{
    /// <summary>
    /// Class, that does Physics cacultations assuming Ball is always on Plate.
    /// Author: Timo Schmetzer
    /// Version 0.1
    /// </summary>
    public class PhysicsOnPlate : IPhysicsCalculator
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

            #region SetNEWAngle
            state.Tilt += elapsedSeconds * state.PlateVelocity;
            #endregion

            #region CalcMovement
                state.Position = new Point3D(state.Position.X,state.Position.Y,Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt)));
                state.Acceleration = Utilities.Physics.HangabtriebsbeschleunigungBerechnen(state.Gravity, state.Tilt);
                Utilities.Physics.CalcMovement(state, elapsedSeconds);
                state.Position = new Point3D(state.Position.X, state.Position.Y, Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt)));
            #endregion

            #region Debugout
            //(Nr.)	PositionX	PositionY	PositionZ	VelociyX	VelocityY	VelocityZ	AccelerationX	AccelerationY	AccelerationZ	AngleBetweenVec	elapsedSec	TiltX	TiltY	PlateVelX	PlateVelY   DeltaZBallPlate
                //System.Diagnostics.Debug.Print(state.Position.X + "\t" + state.Position.Y + "\t" + state.Position.Z + "\t" +
                //    state.Velocity.X + "\t" + state.Velocity.Y + "\t" + state.Velocity.Z + "\t"
                //    + state.Acceleration.X + "\t" + state.Acceleration.Y + "\t" + state.Acceleration.Z + "\t" +
                //    +BallOnTiltablePlate.TimoSchmetzer.Utilities.Mathematics.AngleBetwennVectors(BallOnTiltablePlate.TimoSchmetzer.Utilities.Mathematics.CalcNormalVector(state.Tilt), state.Velocity)
                //    + "\t" + elapsedSeconds + "\t" + state.Tilt.X + "\t" + state.Tilt.Y + "\t" + state.PlateVelocity.X
                //     + "\t" + state.PlateVelocity.Y + "\t" + Math.Abs(state.Position.Z - Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt))));
            #endregion
        }
    }
}
