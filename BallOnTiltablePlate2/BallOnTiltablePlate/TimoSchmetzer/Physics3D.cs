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

namespace BallOnTiltablePlate.TimoSchmetzer.Physics
{
    /// <summary>
    /// Class, that does Physics cacultations.
    /// Statringvalues must be written in the provided public Fields.
    /// Results can be obtained after execution of the RunPhysics Method out of the public fields.
    /// </summary>
    public class Physics3D
    {
        #region info
        public String Author = "Timo Schmetzer";
        public Version Version = new Version(0, 5);
        public String Comment = "Pre-Beta. Plate must'nt be turned while Ball is on plate.";
        #endregion

        /// <summary>
        /// Calculates the New State of the Ball given by the current IPhysicsState.
        /// </summary>
        /// <param name="current">Current Physics State</param>
        /// <param name="elapsedSeconds">Seconds to elapse</param>
        /// <returns>State after elapsedSeconds</returns>
        public void RunPhysics(IPhysicsState current, double elapsedSeconds)
        {
            //Sinnlose aufrufe vermeiden
            if (elapsedSeconds == 0) { return; }
            #region Debugout
            //System.Diagnostics.Debug.Print(
            //    "sp:" + s.Position.ToString() + "\t v:" + s.Velocity.ToString() + "\t acc:" + s.Acceleration.ToString()
            //    + "\t ntop:" + PhysicsUtilities.CalcNextHit(s) + "\t n:" + MathUtilities.CalcNormalisizedNormalVector(s.Tilt, true).ToString() +
            //    "\t els:" + ElapsedSeconds);
            #endregion
            bool CentrifugalSpecificCalcNeeded = false;
            #region Test
            //TODO:Implement test
            //When the plate is not moved the calculation is never needed, but you cannot be sure that the next state
            //comes from the Same Simulation so other Methods (independend of the IPhysicsstate) before must be found.
            #endregion Test
            if (CentrifugalSpecificCalcNeeded)
            {
                //TODO: heruntergehende Platte: integration ueber hangabtriebskraft
                //TODO: Beschleunigung der Platte auf Ball uebertragen und integrieren
                //beschleungung orthagonal zur Platte
                //TODO: Prinzipien in 3D anwenden.
                throw new NotImplementedException(); 
            }
            else
            { 
                DoOrdinaryCalculation(current, elapsedSeconds);
            }
        }

        /// <summary>
        /// Represents the Status of the Ball.
        /// Enum, die Ausdruekten soll, in welchen Zustand sich der Ball befindet.
        /// </summary>
        private enum BallState
        {
            InAir = 0,
            OnPlate = 1,
            DownPlate = 2
        };
        /// <summary>
        /// Berechnet, wie kugel sich weiterbewegt unter der annahme das der BallState dabei stets der gleiche bleibt.
        /// fuer verschiedene BallStates Methode mehrmals (mit den jew. Ballstates) aufrufen
        /// Zeit wird um elapsedSeconds erhoet.
        /// Keine Hit/Cenetrifugal berechnung.
        /// </summary>
        private void CalcMovement(IPhysicsState state, double ElapsedSeconds, BallState Ballstate)
        {
            Vector3D G = new Vector3D(0, 0, state.g);
            //Beschleunigung berechnen
            switch (Ballstate)
            {
                case BallState.InAir:
                    state.Acceleration = G;
                    break;
                case BallState.OnPlate:
                    state.Acceleration = Utilities.Physics.HangabtriebskraftBerechnen(state.g, state.Tilt, true);
                    break;
                default:
                    throw new SystemException("Sorry. This shouldn't happen.");
            }
            //Bewegungsgleichung
            state.Position += state.Velocity * ElapsedSeconds + 0.5 * state.Acceleration * ElapsedSeconds * ElapsedSeconds;
            state.Velocity += state.Acceleration * ElapsedSeconds;
            //Wenn auf Platte - Exakt setzen
            if (Ballstate == BallState.OnPlate)
            {
                //Exakt auf Platte setzen
                Vector3D n = Utilities.Mathematics.CalcNormalisizedNormalVector(state.Tilt, true);
                double PlateZ = (-state.Position.X * n.X - state.Position.Y * n.Y) / (n.Z);
                state.Position = new Point3D(state.Position.X, state.Position.Y, PlateZ);
            }
        }
        /// <summary>
        /// Ordinary Calculation (if Plate not Moved)
        /// </summary>
        private void DoOrdinaryCalculation(IPhysicsState state, double ElapsedSeconds)
        {
            //Beschleunigung zur Hit-Berechnug setzen
            Vector3D G = new Vector3D(0, 0, state.g);
            state.Acceleration = G;
            double nextHit = Utilities.Physics.CalcNextHit(state);

            if (double.IsInfinity(nextHit))
            {
                //Ball rollt auf Platte (kein Hit)
                CalcMovement(state, ElapsedSeconds, BallState.OnPlate);
            }
            //Ball rollt nicht auf der Platte. Hit nicht mehr in diesem Update.
            else if (nextHit > ElapsedSeconds)
                CalcMovement(state, ElapsedSeconds, BallState.InAir);
            else
            {
                //Hit in diesem Update.
                CalcMovement(state, nextHit, BallState.InAir);
                //Exakt auf Platte setzen
                Vector3D n = Utilities.Mathematics.CalcNormalisizedNormalVector(state.Tilt, true);
                double PlateZ = (-state.Position.X * n.X - state.Position.Y * n.Y) / (n.Z);
                new Point3D(state.Position.X, state.Position.Y, PlateZ);
                state = Utilities.Physics.Reflect(state);
                RunPhysics(state, ElapsedSeconds - nextHit);
            }
        }
    }
}
