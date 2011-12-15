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
    /// </summary>
    public class Physics3D
    {
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
        public void CalcPhysics(PhysicsState state, double elapsedSeconds)
        {
            //Sinnlose aufrufe vermeiden
            if (elapsedSeconds == 0) { return; }

            //Beschleunigung zur Hit-Berechnug setzen
            Vector3D G = new Vector3D(0, 0, state.Gravity);
            state.Acceleration = G;
            double nextHit = Utilities.Physics.CalcNextHit(state);

            BallState bs;

            if (double.IsInfinity(nextHit))
            {
                if (state.Gravity < 0)
                {
                    //Ball rollt auf Platte (kein Hit)
                    //Bei gravitation muss der Ball auf der Platte rollen
                    CalcMovement(state, elapsedSeconds, BallState.RollOnPlate);
                }
                else
                {
                    //Bei Gravity 0 oder pos kann der Ball nicht auf der Platte rollen
                    //Bei den Gravitationen muss der Ball nicht auf der Platte sein um
                    //nicht irgendwann einen Hit zu Verursachen.
                    CalcMovement(state, elapsedSeconds, BallState.InAir);
                }
            }
            //Ball rollt nicht auf der Platte. Hit nicht mehr in diesem Update.
            else if (nextHit > elapsedSeconds)
                CalcMovement(state, elapsedSeconds, BallState.InAir);
            else
            {
                //Hit in diesem Update.
                CalcMovement(state, nextHit, BallState.InAir);
                if (Utilities.Physics.IsHit(state))
                {
                    state = Utilities.Physics.Reflect(state);
                }
                CalcMovement(state, elapsedSeconds - nextHit, BallState.InAir);

            }

            #region Debugout
            //System.Diagnostics.Debug.Print();
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
        /// Berechnet, wie kugel sich weiterbewegt unter der annahme das der BallState dabei stets der gleiche bleibt.
        /// fuer verschiedene BallStates Methode mehrmals (mit den jew. Ballstates) aufrufen
        /// </summary>
        private void CalcMovement(PhysicsState state, double ElapsedSeconds, BallState Ballstate)
        {
            Vector3D G = new Vector3D(0, 0, state.Gravity);
            //Beschleunigung berechnen
            switch (Ballstate)
            {
                case BallState.InAir:
                    state.Acceleration = G;
                    break;
                case BallState.RollOnPlate:
                    state.Acceleration = Utilities.Physics.HangabtriebskraftBerechnen(state.Gravity, state.Tilt);
                    break;
                default:
                    throw new SystemException("Sorry. This shouldn't happen.");
            }
            //Bewegungsgleichung
            state.Position += state.Velocity * ElapsedSeconds + 0.5 * state.Acceleration * ElapsedSeconds * ElapsedSeconds;
            state.Velocity += state.Acceleration * ElapsedSeconds;
        }
    }
}
