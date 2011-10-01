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
        /// Calculates the New State of the Ball given by the current PhysicsState.
        /// </summary>
        /// <param name="current">Current Physics State</param>
        /// <returns>State after s.SecondsToElapse</returns>
        public void RunPhysics(IPhysicsState current)
        {
            //Sinnlose aufrufe vermeiden
            if (s.SecondsToElapse == 0) { return current; }
            //put current to s (for calls in other Methods)
            s = current;
            #region Debugout
            System.Diagnostics.Debug.Print(
                "t " + s.AbsoluteTime + "\t sp:" + s.Position.ToString() + "\t v:" + s.Velocity.ToString() + "\t acc:" + s.Acceleration.ToString()
                + "\t ntop:" + PhysicsUtilities.AbsoluteTimeNextHit(s) + "\t n:" + MathUtilities.CalcNormalisizedNormalVector(s.Tilt, true).ToString() +
                "\t els:" + s.SecondsToElapse);
            #endregion
            bool CentrifugalSpecificCalcNeeded = false;
            #region Test
            //TODO:Implement test
            //When the plate is not moved the calculation is never needed.
            CentrifugalSpecificCalcNeeded = LastTilt == s.Tilt ? false : CentrifugalSpecificCalcNeeded;
            LastTilt = s.Tilt;
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
            { DoOrdinaryCalculation(); }

            //Return values and set Field to Null
            current = s;
            s = null;
            return current;
        }
        #region Private
        #region InternHelpProperties

        #region DerivedConstants
        /// <summary>
        /// Contains the Locals Gravity as a Vector (= (0,0,g) ).
        /// Entheallt die Erdbeschleunigung als Vector3D (=0,0,g).
        /// </summary>
        private Vector3D G { get { return new Vector3D(0, 0, s.g); } }
        #endregion

        #region Assumptions
        //Currently unused.
        ///// <summary>
        ///// Repraesetiert die Beschleugnigungsart, die bei einer Bewegung der Blatte
        ///// [mit Ball auf Platte] angenommen werden soll.
        ///// </summary>
        //public AccelerationAssumption assumption;

        ///// <summary>
        ///// Repraesetiert die Beschleugnigungsart, die bei einer Bewegung der Blatte
        ///// [mit Ball auf Platte] angenommen werden soll.
        ///// </summary>
        //public enum AccelerationAssumption
        //{
        //    ConstantlyAccelerated = 1
        //};
        #endregion

        #region TestHelp
        /// <summary>
        /// Tilt on the last call of Run Physics.
        /// Used for testing on additional calculation.
        /// </summary>
        private Vector LastTilt;
        #endregion

        /// <summary>
        /// The state on which caluations are based.
        /// </summary>
        private PhysicsState s;

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
        #endregion
        #region PhysicalMethods
        /// <summary>
        /// Ordinary Calculation (if Plate not Moved)
        /// </summary>
        private void DoOrdinaryCalculation()
        {
            //Beschleunigung zur Hit-Berechnug setzen
            s.Acceleration = G;
            double nextHit = PhysicsUtilities.AbsoluteTimeNextHit(s);

            if (double.IsInfinity(nextHit))
            {
                //Ball rollt auf Platte (kein Hit)
                CalcMovement(s.SecondsToElapse, BallState.OnPlate);
            }
            //Ball rollt nicht auf der Platte. Hit nicht mehr in diesem Update.
            else if (nextHit > s.AbsoluteTime + s.SecondsToElapse)
                CalcMovement(s.SecondsToElapse, BallState.InAir);
            else
            {
                //Hit in diesem Update.
                CalcMovement(nextHit - s.AbsoluteTime, BallState.InAir);
                //Exakt auf Platte setzen
                Vector3D n = MathUtilities.CalcNormalisizedNormalVector(s.Tilt, true);
                double PlateZ = (-s.Position.X * n.X - s.Position.Y * n.Y) / (n.Z);
                s.Position.Z = PlateZ;
                s = PhysicsUtilities.Reflect(s);
                s.SecondsToElapse = s.SecondsToElapse - (nextHit - s.AbsoluteTime);
                RunPhysics(s);
            }
        }

        /// <summary>
        /// Berechnet, wie kugel sich weiterbewegt unter der annahme das der BallState dabei stets der gleiche bleibt.
        /// fuer verschiedene BallStates Methode mehrmals (mit den jew. Ballstates) aufrufen
        /// Zeit wird um elapsedSeconds erhoet.
        /// Keine Hit/Cenetrifugal berechnung.
        /// </summary>
        private void CalcMovement(double SecondsToElapse, BallState state)
        {
            //Beschleunigung berechnen
            switch (state)
            {
                case BallState.InAir:
                    s.Acceleration = G;
                    break;
                case BallState.OnPlate:
                    s.Acceleration = PhysicsUtilities.HangabtriebskraftBerechnen(s.g, s.Tilt, true);
                    break;
                default:
                    throw new SystemException("Sorry. This shouldn't happen.");
            }
            //Bewegungsgleichung
            s.Position += s.Velocity * s.SecondsToElapse + 0.5 * s.Acceleration * s.SecondsToElapse * s.SecondsToElapse;
            s.Velocity += s.Acceleration * s.SecondsToElapse;
            //Zeitsetzen
            s.AbsoluteTime += s.SecondsToElapse;
            if (state == BallState.OnPlate)
            {
                //Exakt auf Platte setzen
                Vector3D n = MathUtilities.CalcNormalisizedNormalVector(s.Tilt, true);
                double PlateZ = (-s.Position.X * n.X - s.Position.Y * n.Y) / (n.Z);
                s.Position.Z = PlateZ;
            }
        }
        #endregion
        #endregion
    }

    public static class PhysicsUtilities
    {
        /// <summary>
        /// Returns the AbsoluteTime, after which the Ball will hit the next time on the plate.
        /// Does not take care of whether the plate is moved. Use oly for unmoved Plates.
        /// double.positiveInfinity if 0 or never.
        /// Gibt die Absolute Zeit zurueck, zu der der Ball sich das naechste mal Auf der Platte befinden wird.
        /// </summary>
        /// <param name="state">The state for which to calc</param>
        /// <returns>See summary.</returns>
        public static double AbsoluteTimeNextHit(PhysicsState state) { return state.AbsoluteTime + PhysicsUtilities.CalcNextHit(state); }

        /// <summary>
        /// Indicates wheter the Ball, if being on the plate, would hit (true) or roll (false).
        /// Gibt zurueck ob, wenn der Ball auf der Platte waere er auftreffen wuerde(true) oder Rollen wuerde(false)
        /// </summary>
        /// <param name="state">The state for which to calc</param>
        /// <returns>See summary.</returns>
        [Obsolete("Not Reliable", true)]
        private static bool IsHit(PhysicsState state) { return !MathUtilities.Orthagonal(MathUtilities.CalcNormalisizedNormalVector(state.Tilt, true), state.Velocity); }

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
            state.Velocity = MathUtilities.Householdertransformation(state.Velocity, MathUtilities.CalcNormalisizedNormalVector(state.Tilt, true));
            //fertig geschwindigkeitsberechnug
            //ABSORBTION (Reduktion des Geschwindigkeitsbetrags)
            //Relativ
            //hoehe propertional Energie. Energie propertional geschw. im quadr.
            //Faktor mit dem bei einer Reflektion der Geschwindigkeitsvektor verkleinert/skaliert wird.
            double absorbtionsfaktor = Math.Sqrt(state.HightFactor);
            state.Velocity = MathUtilities.SkaleAbsoluteValueOfAVectorByFactor(state.Velocity, absorbtionsfaktor);
            //Absolut
            if (state.Velocity.Length > state.AbsoluteAbsorbtion)
            {
                state.Velocity = MathUtilities.ResizeAbsoluteValueOfAVector(state.Velocity, state.Velocity.Length - state.AbsoluteAbsorbtion);
            }
            else { state.Velocity = new Vector3D(0, 0, 0); }
            return state;
        }

        /// <summary>
        /// Berechnet den Zeitpunkt des naechsten auftreffens auf der Platte
        /// (ausgehend von abstime, d.h. wenn sich die Kugel bereits auf der Platte befindet 
        /// wird diese Lsg nicht berucksichtigt)
        /// Does not take care of whether the plate is moved. Use oly for unmoved Plates.
        /// </summary>
        /// <param name="state">The state for which to calc</param>
        /// <returns>naechsten Zeitpunkt des Auftreffens. findet sich keine, double.PositiveInfinity</returns>
        public static double CalcNextHit(PhysicsState state)
        {
            double[] solutions = PhysicsUtilities.CalcNextHitRawSolution(state);
            //too high accuracy
            System.Diagnostics.Debug.Print(solutions[1].ToString());
            solutions[0] += 11.1;
            solutions[0] -= 11.1;
            solutions[1] += 11.1;
            solutions[1] -= 11.1;
            System.Diagnostics.Debug.Print(solutions[1].ToString());
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
            Vector3D Normal = MathUtilities.CalcNormalisizedNormalVector(state.Tilt, true);
            double a = 0.5 * state.Acceleration.X * Normal.X + 0.5 * state.Acceleration.Y * Normal.Y + 0.5 * state.Acceleration.Z * Normal.Z;
            double b = state.Velocity.X * Normal.X + state.Velocity.Y * Normal.Y + state.Velocity.Z * Normal.Z;
            double c = state.Position.X * Normal.X + state.Position.Y * Normal.Y + state.Position.Z * Normal.Z;
            double[] solutions = MathUtilities.MNF(a, b, c);
            return solutions;
        }

        /// <summary>
        /// Hangabtriebskraft berechnen. Keine gute Uebersetzung fuer Hangabtribskraft gefunden.
        /// </summary>
        /// <param name="Tilt">Tilt</param>
        /// <param name="IsRad">true: Tilt is given in Rad. false: Tilt is given in degrees.</param>
        /// <returns>Hangabtriebskraft</returns>
        public static Vector3D HangabtriebskraftBerechnen(double g, Vector Tilt, bool IsRad)
        {
            if (!IsRad)
            { Tilt.X = MathUtil.RadToDeg(Tilt.X); Tilt.Y = MathUtil.RadToDeg(Tilt.Y); }
            double alphax = Tilt.X;
            double alphay = Tilt.Y;
            //Acceleration = G - (((Vector3D.DotProduct(G, n)) / (Vector3D.DotProduct(n, n))) * n);
            //Maple created code.
            Vector3D cg = new Vector3D();
            cg.X = g * Math.Cos(alphax) * Math.Pow(Math.Cos(alphay), 0.2e1) * Math.Sin(alphax);
            cg.Y = g * Math.Cos(alphax) * Math.Cos(alphay) * Math.Sin(alphay);
            cg.Z = -g * (-0.1e1 + Math.Pow(Math.Cos(alphax), 0.2e1) * Math.Pow(Math.Cos(alphay), 0.2e1));
            return cg;
        }
    }

    public static class MathUtilities
    {
        /// <summary>
        /// Berechnet die Nullstellen eines Polynoms der Form a*x*x + b*x +c
        /// anhand der Mitternachtsformel
        /// </summary>
        /// <param name="a">siehe summary</param>
        /// <param name="b">siehe summary</param>
        /// <param name="c">siehe summary</param>
        /// <returns>double[] mit loesungen</returns>
        public static double[] MNF(double a, double b, double c)
        {
            //Mitternachtsformel
            return new double[] { (-b + Math.Sqrt((b * b) - (4 * a * c))) / (2 * a), (-b - Math.Sqrt((b * b) - (4 * a * c))) / (2 * a) };
        }

        /// <summary>
        /// Gibt zurueck, ob die zwei Vector3D orthagonal zueinander stehen.
        /// </summary>
        /// <param name="a">1.Vector3D</param>
        /// <param name="b">2.Vector3D</param>
        /// <returns>bool; true wenn a orthagonal zu b, sonst false</returns>
        public static bool Orthagonal(Vector3D a, Vector3D b)
        {
            double x = Vector3D.DotProduct(a, b);
            return x == 0;
        }

        /// <summary>
        /// Indicates, whether the Ball is on the Plate.
        /// Gibt zurueck, ob der Ball auf der Platte ist.
        /// [for a Plate that goes through (0,0,0)]
        /// </summary>
        /// <param name="Position">Position of the Ball</param>
        /// <param name="NormalVector">NormalVector of the Plate</param>
        /// <returns>bool; true wenn Ball unter der Platte ist, sonst false</returns>
        public static bool IsOnPlate(Point3D Position, Vector3D NormalVector)
        {
            double x = Vector3D.DotProduct(NormalVector, (Vector3D)Position);
            return x == 0;
        }

        /// <summary>
        /// Indicates, whether the Ball is under the Plate.
        /// Gibt zurueck, ob der Ball unter der Platte ist.
        /// [for a Plate that goes through (0,0,0)]
        /// </summary>
        /// <param name="Position">Position of the Ball</param>
        /// <param name="NormalVector">NormalVector of the Plate</param>
        /// <returns>bool; true wenn Ball unter der Platte ist, sonst false</returns>
        public static bool IsDownPlate(Point3D Position, Vector3D NormalVector)
        {
            Vector3D n = NormalVector;
            double PlateZ = (-Position.X * n.X - Position.Y * n.Y) / (n.Z);
            return Position.Z < PlateZ;
        }

        /// <summary>
        /// Spiegelt einen Vektor an einer (im Ursprung residenten) Ebene mit Hilfe der Householdertransformation
        /// </summary>
        /// <param name="vector">Zu spieglender Vektor</param>
        /// <param name="NormalVector">Normalenvektor der Ebene</param>
        /// <returns>Gespiegelten Vektor</returns>
        public static Vector3D Householdertransformation(Vector3D vector, Vector3D NormalVector)
        {
            //Vektor an Ebnene der Plate Spiegeln

            //Householdertransformation
            //H = I - (2/vTv)*vvT
            //mit: I einheitsmatrix
            //v Normalenvektor
            //H Householder-Matrix

            //Einheitsmatrix R^3
            double[,] I = new double[3, 3];
            I[0, 0] = 1; I[0, 1] = 0; I[0, 2] = 0;
            I[1, 0] = 0; I[1, 1] = 1; I[1, 2] = 0;
            I[2, 0] = 0; I[2, 1] = 0; I[2, 2] = 1;

            //normalenvektor v Spaltenvektor
            double[,] v = new double[3, 1];
            v[0, 0] = NormalVector.X;
            v[1, 0] = NormalVector.Y;
            v[2, 0] = NormalVector.Z;

            //Transponierter Normalenzektor (Zeilenvektor)
            double[,] vT = new double[1, 3];
            vT[0, 0] = v[0, 0]; vT[0, 1] = v[1, 0]; vT[0, 2] = v[2, 0];

            //dyadisches Produkt/Tensorprodukt berechenen
            double[,] vvT = new double[3, 3];
            vvT[0, 0] = v[0, 0] * vT[0, 0]; vvT[0, 1] = v[0, 0] * vT[0, 1]; vvT[0, 2] = v[0, 0] * vT[0, 2];
            vvT[1, 0] = v[1, 0] * vT[0, 0]; vvT[1, 1] = v[1, 0] * vT[0, 1]; vvT[1, 2] = v[1, 0] * vT[0, 2];
            vvT[2, 0] = v[2, 0] * vT[0, 0]; vvT[2, 1] = v[2, 0] * vT[0, 1]; vvT[2, 2] = v[2, 0] * vT[0, 2];

            //Skararprodukt berechen (vT*v=v*v)
            double vTv = v[0, 0] * v[0, 0] + v[1, 0] * v[1, 0] + v[2, 0] * v[2, 0];

            //Faktor von vvT berechen
            double factor = 2 / vTv;

            //vvT mit Faktor multiplizieren
            vvT[0, 0] *= factor; vvT[0, 1] *= factor; vvT[0, 2] *= factor;
            vvT[1, 0] *= factor; vvT[1, 1] *= factor; vvT[1, 2] *= factor;
            vvT[2, 0] *= factor; vvT[2, 1] *= factor; vvT[2, 2] *= factor;

            //H = I - die erhaltene Matrix
            double[,] H = new double[3, 3];
            H[0, 0] = I[0, 0] - vvT[0, 0]; H[0, 1] = I[0, 1] - vvT[0, 1]; H[0, 2] = I[0, 2] - vvT[0, 2];
            H[1, 0] = I[1, 0] - vvT[1, 0]; H[1, 1] = I[1, 1] - vvT[1, 1]; H[1, 2] = I[1, 2] - vvT[1, 2];
            H[2, 0] = I[2, 0] - vvT[2, 0]; H[2, 1] = I[2, 1] - vvT[2, 1]; H[2, 2] = I[2, 2] - vvT[2, 2];

            //Housholdermatrix mit Vector multiplizieren um neue Geschwindigkeit zu erhalten (h*vel)
            double[,] NewVector = new double[3, 1];
            NewVector[0, 0] = H[0, 0] * vector.X + H[0, 1] * vector.Y + H[0, 2] * vector.Z;
            NewVector[1, 0] = H[1, 0] * vector.X + H[1, 1] * vector.Y + H[1, 2] * vector.Z;
            NewVector[2, 0] = H[2, 0] * vector.X + H[2, 1] * vector.Y + H[2, 2] * vector.Z;

            //Gespiegleten Vektor zurueckgeben
            return new Vector3D(NewVector[0, 0], NewVector[1, 0], NewVector[2, 0]);
        }

        /// <summary>
        /// Returns the Normal Vector of a plane with the specified turnations around the axis.
        /// </summary>
        /// <param name="Tilt">Rotation</param>
        /// <param name="IsRad">true: Tilt is given in Rad false: Tilt is given in degrees.</param>
        /// <returns>Normalisized Normal Vector</returns>
        public static Vector3D CalcNormalisizedNormalVector(Vector Tilt, bool IsRad)
        {
            if (!IsRad)
            { Tilt.X = MathUtil.RadToDeg(Tilt.X); Tilt.Y = MathUtil.RadToDeg(Tilt.Y); }
            double alphax = Tilt.X;
            double alphay = Tilt.Y;
            //Maple created code.
            Vector3D cg = new Vector3D();
            cg.X = -Math.Sin(alphax) * Math.Cos(alphay);
            cg.Y = -Math.Sin(alphay);
            cg.Z = Math.Cos(alphax) * Math.Cos(alphay);
            return cg;
        }

        /// <summary>
        /// Skaliert einen Vektorbetrag/Vector mit einer Zahl
        /// </summary>
        /// <param name="vector">Vektor</param>
        /// <param name="ScalingFacor">Zahl</param>
        /// <returns>Skalierter Vektor</returns>
        public static Vector3D SkaleAbsoluteValueOfAVectorByFactor(Vector3D vector, double ScalingFacor)
        { return vector * ScalingFacor; }

        /// <summary>
        /// Aendert den Vektor so, dass der Betrag gleich Amount ist, und die Richtung gleich bleibt.
        /// </summary>
        /// <param name="vector">vector</param>
        /// <param name="Amount">Amount</param>
        /// <returns>Changed Vector</returns>
        public static Vector3D ResizeAbsoluteValueOfAVector(Vector3D vector, double Amount)
        {
            //Tatsaechlichen Vektorbetrag ermitteln
            double VektorBetrag = vector.Length;
            //Verhaltnis gewuenscht/tatsaechlich ermitteln
            double Faktor = Amount / VektorBetrag;
            //Vektor skalieren
            return vector * Faktor;
        }

        /// <summary>
        /// Convert Funktion for a Angle. Taken from WPFConverter.
        /// </summary>
        /// <param name="degrees">Angle in Degree</param>
        /// <returns>Angle in radian</returns>
        public static double DegToRad(double degrees)
        {
            return (degrees / 180.0) * Math.PI;
        }

        /// <summary>
        /// Convert Funktion for a Angle. Taken from WPFConverter.
        /// </summary>
        /// <param name="radian">Angle in radian</param>
        /// <returns>Angle in Degree</returns>
        public static double RadToDeg(double radian)
        {
            return (radian / Math.PI) * 180.0;
        }
    }
}
