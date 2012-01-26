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
using BallOnTiltablePlate.TimoSchmetzer.Simulation;

namespace BallOnTiltablePlate.TimoSchmetzer.Utilities
{
    public static class Physics
    {
        /// <summary>
        /// Indicates wheter the Ball, being on the plate, would hit (true) or roll (false).
        /// Gibt zurueck ob, wenn der Ball auf der Platte waere er auftreffen wuerde(true) oder Rollen wuerde(false)
        /// </summary>
        /// <param name="state">The state for which to calc</param>
        /// <returns>See summary.</returns>
        public static bool WouldHit(PhysicsState state) 
        {  
            Vector3D n = Mathematics.CalcNormalVector(state.Tilt);
            double angle = Mathematics.AngleBetwennVectors(n, state.Velocity);
            return (angle > Mathematics.DegToRad(100) || angle < Mathematics.DegToRad(80));
        }

        /// <summary>
        /// Spiegelt den Geschwindigkeitsvektor mittels einer Householdertransformation.
        /// Geschwindikeitsvektor wird entsprechend von absorbitonsfaktor skaliert.
        /// AbsTimeReflected wird auf abstime gesetzt.
        /// </summary>
        /// <param name="state">The state for which to calc</param>
        /// <returns>Reflected State</returns>
        public static PhysicsState Reflect(PhysicsState state)
        {
            //Geschwindigkeit an Ebene der Platte Spiegln
            state.Velocity = Mathematics.Householdertransformation(state.Velocity, Mathematics.CalcNormalVector(state.Tilt));
            //fertig geschwindigkeitsberechnug
            //ABSORBTION (Reduktion des Geschwindigkeitsbetrags)
            //Relativ
            //hoehe propertional Energie. Energie propertional geschw. im quadr.
            //Faktor mit dem bei einer Reflektion der Geschwindigkeitsvektor verkleinert/skaliert wird.
            double absorbtionsfaktor = Math.Sqrt(state.HitAttenuationFactor);
            state.Velocity = Mathematics.SkaleAbsoluteValueOfAVectorByFactor(state.Velocity, absorbtionsfaktor);
            //Absolut
            if (state.Velocity.Length > state.AbsoluteAbsorbtion)
            {
                state.Velocity = Mathematics.ResizeAbsoluteValueOfAVector(state.Velocity, state.Velocity.Length - state.AbsoluteAbsorbtion);
            }
            else { state.Velocity = new Vector3D(0, 0, 0); }
            return state;
        }

        /// <summary>
        /// Berechnet den Zeitpunkt des naechsten auftreffens auf der Platte
        /// (ausgehend von abstime, d.h. wenn sich die Kugel bereits auf der Platte befindet 
        /// wird diese Lsg nicht berucksichtigt)
        /// Does not take care of whether the plate is moved. Use oly for unmoved Plates.
        /// double.positiveInfinity if 0 or never.
        /// </summary>
        /// <param name="state">The state for which to calc</param>
        /// <returns>naechsten Zeitpunkt des Auftreffens. findet sich keine, double.PositiveInfinity</returns>
        public static double CalcNextHit(PhysicsState state)
        {
            double[] solutions = Physics.CalcNextHitRawSolution(state);
            //too high accuracy
            //System.Diagnostics.Debug.Print(solutions[1].ToString());
            solutions[0] += 1111.1;
            solutions[0] -= 1111.1;
            solutions[1] += 1111.1;
            solutions[1] -= 1111.1;
            //System.Diagnostics.Debug.Print(solutions[1].ToString());
            if (double.IsNaN(solutions[0]) || double.IsInfinity(solutions[0]) || solutions[0] <= 0)
            {
                if (double.IsNaN(solutions[1]) || double.IsInfinity(solutions[1]) || solutions[1] <= 0)
                {
                    return double.PositiveInfinity;
                }
                else
                {
                    return solutions[1];
                }
            }
            else if (double.IsNaN(solutions[1]) || double.IsInfinity(solutions[1]) || solutions[1] <= 0)
            {
                if (double.IsNaN(solutions[0]) || double.IsInfinity(solutions[0]) || solutions[0] <= 0)
                {
                    return double.PositiveInfinity;
                }
                else
                {
                    return solutions[0];
                }
            }
            else if (solutions[0] > 0 && solutions[1] > 0)
            {
                return solutions[0] < solutions[1] ? solutions[0] : solutions[1];
            }
            else
            {
                throw new SystemException("Sorry. This shouldn't happen...");
            }
        }

        /// <summary>
        /// Returns solutions to the equation when Ball will hit the Plate.
        /// This Returns the RAW solutions as obtained from the equation.
        /// This solutions may be negative, double.NaN, ...
        /// If you search for a sensefull solution please use CalcNextHit().
        /// Does not take care of whether the plate is moved. Use oly for unmoved Plates.
        /// </summary>
        /// <param name="state">The state for which to calc</param>
        /// <returns>Raw Solution</returns>
        public static double[] CalcNextHitRawSolution(PhysicsState state)
        {
            Vector3D Normal = Mathematics.CalcNormalVector(state.Tilt);
            double a = 0.5 * state.Acceleration.X * Normal.X + 0.5 * state.Acceleration.Y * Normal.Y + 0.5 * state.Acceleration.Z * Normal.Z;
            double b = state.Velocity.X * Normal.X + state.Velocity.Y * Normal.Y + state.Velocity.Z * Normal.Z;
            double c = state.Position.X * Normal.X + state.Position.Y * Normal.Y + state.Position.Z * Normal.Z;
            double[] solutions = Mathematics.MNF(a, b, c);
            return solutions;
        }

        /// <summary>
        /// Hangabtriebskraft berechnen. Keine gute Uebersetzung fuer Hangabtribskraft gefunden.
        /// </summary>
        /// <param name="Tilt">Tilt is given in Rad.</param>
        /// <returns>Hangabtriebskraft</returns>
        public static Vector3D HangabtriebsbeschleunigungBerechnen(double g, Vector Tilt)
        {
            //AccelerationForce= G - (((Vector3D.DotProduct(G, n)) / (Vector3D.DotProduct(n, n))) * n);
            Vector3D AccelerationForce = new Vector3D();
            Vector3D G = new Vector3D(0,0,g);
            Vector3D n = Mathematics.CalcNormalVector(Tilt);
            AccelerationForce = G - (((Vector3D.DotProduct(G, n)) / (Vector3D.DotProduct(n, n))) * n);
            return AccelerationForce;
        }

        /// <summary>
        /// Same as Overload with Tilt but does not calc the Normal itself out of Tilt.
        /// </summary>
        /// <param name="g">Gravity</param>
        /// <param name="Normal">Normal of Plate</param>
        /// <returns>Hangabtriebskraft</returns>
        public static Vector3D HangabtriebsbeschleunigungBerechnen(double g, Vector3D Normal)
        {
            //AccelerationForce= G - (((Vector3D.DotProduct(G, n)) / (Vector3D.DotProduct(n, n))) * n);
            Vector3D AccelerationForce = new Vector3D();
            Vector3D G = new Vector3D(0, 0, g);
            Vector3D n = Normal;
            AccelerationForce = G - (((Vector3D.DotProduct(G, n)) / (Vector3D.DotProduct(n, n))) * n);
            return AccelerationForce;
        }

        /// <summary>
        /// Calculates Movement using s = 0.5att + v0t + s0, v= ...
        /// </summary>
        public static void CalcMovement(PhysicsState state, double ElapsedSeconds)
        {
            //Bewegungsgleichung
            state.Position += state.Velocity * ElapsedSeconds + 0.5 * state.Acceleration * ElapsedSeconds * ElapsedSeconds;
            state.Velocity += state.Acceleration * ElapsedSeconds;
        }
    }
}
