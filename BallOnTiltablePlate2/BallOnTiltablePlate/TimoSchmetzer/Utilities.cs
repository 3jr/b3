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

namespace BallOnTiltablePlate.TimoSchmetzer.Utilities
{
    public static class Physics
    {
        /// <summary>
        /// Indicates wheter the Ball, if being on the plate, would hit (true) or roll (false).
        /// Gibt zurueck ob, wenn der Ball auf der Platte waere er auftreffen wuerde(true) oder Rollen wuerde(false)
        /// </summary>
        /// <param name="state">The state for which to calc</param>
        /// <returns>See summary.</returns>
        [Obsolete("Not Reliable", true)]
        private static bool IsHit(IPhysicsState state) { return !Mathematics.Orthagonal(Mathematics.CalcNormalisizedNormalVector(state.Tilt, true), state.Velocity); }

        /// <summary>
        /// Spiegelt den Geschwindigkeitsvektor mittels einer Householdertransformation.
        /// Geschwindikeitsvektor wird entsprechend von absorbitonsfaktor skaliert.
        /// AbsTimeReflected wird auf abstime gesetzt.
        /// </summary>
        /// <param name="state">The state for which to calc</param>
        /// <returns>Reflected State</returns>
        public static IPhysicsState Reflect(IPhysicsState state)
        {
            //Geschwindigkeit an Ebene der Platte Spiegln
            state.Velocity = Mathematics.Householdertransformation(state.Velocity, Mathematics.CalcNormalisizedNormalVector(state.Tilt, true));
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
        /// Does take care of whether the plate is moved. Use for moved Plates.
        /// double.positiveInfinity if 0 or never.
        /// </summary>
        /// <param name="state">The state for which to calc</param>
        /// <returns>naechsten Zeitpunkt des Auftreffens. findet sich keine, double.PositiveInfinity</returns>
        public static double CalcNextHit(IPhysicsState state)
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
        /// Berechnet den Zeitpunkt des naechsten auftreffens auf der Platte
        /// (ausgehend von abstime, d.h. wenn sich die Kugel bereits auf der Platte befindet 
        /// wird diese Lsg nicht berucksichtigt)
        /// Does not take care of whether the plate is moved. Use oly for unmoved Plates.
        /// double.positiveInfinity if 0 or never.
        /// </summary>
        /// <param name="state">The state for which to calc</param>
        /// <returns>naechsten Zeitpunkt des Auftreffens. findet sich keine, double.PositiveInfinity</returns>
        public static double CalcNextHit2(IPhysicsState state)
        {
            double[] solutions = Physics.CalcNextHitRawSolution2(state);
            //too high accuracy
            System.Diagnostics.Debug.Print(solutions[1].ToString());
            solutions[0] += 1111.1;
            solutions[0] -= 1111.1;
            solutions[1] += 1111.1;
            solutions[1] -= 1111.1;
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
        public static double[] CalcNextHitRawSolution(IPhysicsState state)
        {
            Vector3D Normal = Mathematics.CalcNormalisizedNormalVector(state.Tilt, true);
            double a = 0.5 * state.Acceleration.X * Normal.X + 0.5 * state.Acceleration.Y * Normal.Y + 0.5 * state.Acceleration.Z * Normal.Z;
            double b = state.Velocity.X * Normal.X + state.Velocity.Y * Normal.Y + state.Velocity.Z * Normal.Z;
            double c = state.Position.X * Normal.X + state.Position.Y * Normal.Y + state.Position.Z * Normal.Z;
            double[] solutions = Mathematics.MNF(a, b, c);
            return solutions;
        }

        /// <summary>
        /// Takes Care of PlateVelocity
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static double[] CalcNextHitRawSolution2(IPhysicsState state)
        {

            return new double[0];
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
            { Tilt.X = Mathematics.DegToRad(Tilt.X); Tilt.Y = Mathematics.DegToRad(Tilt.Y); }
            return HangabtriebskraftBerechnen(g, Tilt);
        }

        /// <summary>
        /// Hangabtriebskraft berechnen. Keine gute Uebersetzung fuer Hangabtribskraft gefunden.
        /// </summary>
        /// <param name="Tilt">Tilt is given in Rad.</param>
        /// <returns>Hangabtriebskraft</returns>
        public static Vector3D HangabtriebskraftBerechnen(double g, Vector Tilt)
        {
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
    public static class Mathematics
    {
        public struct LineEquation
        {
            //f(x) = m*x+c
            public double m;
            public double c;
        }

        public static LineEquation GetLineEquation(Point[] values)
        {
            //Do Linear Regression
            int n = values.Length;
            double SumX = 0;
            double SumY = 0;
            double SumSquaredX = 0;
            double XYProductSum = 0;
            foreach (Point P in values)
            {
                SumX += (int)P.X;
                SumY += (int)P.Y;
                SumSquaredX += (int)(P.X * P.X);
                XYProductSum += (int)(P.X * P.Y);
            }
            double AverageX = SumX / n;
            double AverageY = SumY / n;
            LineEquation eq = new LineEquation();
            eq.m = (n*XYProductSum-SumX*SumY)/(n*SumSquaredX-SumX*SumX);
            eq.c = AverageY - eq.m * AverageX;
            return eq;
        }

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
            { Tilt.X = DegToRad(Tilt.X); Tilt.Y = DegToRad(Tilt.Y); }
            return CalcNormalisizedNormalVector(Tilt);
        }

        /// <summary>
        /// Returns the Normal Vector of a plane with the specified turnations around the axis.
        /// </summary>
        /// <param name="Tilt">Rotation given in Rad</param>
        /// <returns>Normalisized Normal Vector</returns>
        public static Vector3D CalcNormalisizedNormalVector(Vector Tilt)
        {
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
