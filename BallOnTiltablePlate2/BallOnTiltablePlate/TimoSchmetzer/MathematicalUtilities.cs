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
    public static class Mathematics
    {
        
        #region LinearRegression

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
            eq.m = (n * XYProductSum - SumX * SumY) / (n * SumSquaredX - SumX * SumX);
            eq.c = AverageY - eq.m * AverageX;
            return eq;
        }

        #endregion

        #region Algebra
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
        #endregion
        
        #region Transformations
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
        #endregion

        #region PlaneCalcs

        /// <summary>
        /// Returns the Normal Vector of a plane with the specified turnations around the axis.
        /// Vector is NOT NORMALIZED.
        /// </summary>
        /// <param name="Tilt">Rotation given in Rad</param>
        /// <returns>Normal Vector</returns>
        public static Vector3D CalcNormalVector(Vector Tilt)
        {
            Vector3D NormalVector = new Vector3D();
            NormalVector.X = -Math.Tan(Tilt.X);
            NormalVector.Y = -Math.Tan(Tilt.Y);
            NormalVector.Z = 1;
            return NormalVector;
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
        /// Returns the Hight of the Plate at coorinates x,y
        /// </summary>
        /// <param name="Position">coordinates</param>
        /// <param name="NormalVector">NormalVector of the Plate</param>
        /// <returns>hight of Plate</returns>
        public static double HightOfPlate(Point Position, Vector3D NormalVector)
        {
            Vector3D n = NormalVector;
            double PlateZ = (-Position.X * n.X - Position.Y * n.Y) / (n.Z);
            return PlateZ;
        }

        [Obsolete("tmpredirect",false)]
        public static double HightofPlate(Point Position, Vector3D NormalVector)
        {
            return (HightOfPlate(Position, NormalVector));
        }

        /// <summary>
        /// Calculastes the Normalized Normal Vector of a 
        /// Points mustn'b be on a line
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Vector3D PointsOnLayerToNormalizizedNormalVector(Vector3D a, Vector3D b, Vector3D c)
        {
            Vector3D Vec = Vector3D.CrossProduct(b - a, c - a);
            Vec.Normalize();
            return Vec;
        }

        /// <summary>
        /// Convert Point given by platecoordinates and plate tilt to Point3D.
        /// </summary>
        /// <param name="PlateCoordinate">Plate Coordinates</param>
        /// <param name="Tilt">Tilt of the plate</param>
        /// <returns></returns>
        public static Point3D PlateCoordinatesToEucidean3DCoordinates(Vector PlateCoordinate, Vector Tilt)
        {
            throw new NotImplementedException("Not finished yet");
            Vector3D Euclidean = new Vector3D();
            Vector3D eX = new Vector3D(Math.Cos(Tilt.X), 0, Math.Sin(Tilt.X));
            Vector3D eY = new Vector3D(0, Math.Cos(Tilt.Y), Math.Sin(Tilt.Y));
            Euclidean = eX * PlateCoordinate.X + eY * PlateCoordinate.Y;
            return (Point3D)Euclidean;
        }

        /// <summary>
        /// Calcs Plate Edges of a 2x2 plate
        /// </summary>
        /// <param name="Tilt">Tilt of the plate</param>
        /// <returns></returns>
        public static Point3D[] CalcPlateEdges(Vector Tilt)
        {
            Point3D[] Edges = new Point3D[4];
            Edges[0] = PlateCoordinatesToEucidean3DCoordinates(new Vector(-1, 1), Tilt);
            Edges[1] = PlateCoordinatesToEucidean3DCoordinates(new Vector(-1, -1), Tilt);
            Edges[2] = PlateCoordinatesToEucidean3DCoordinates(new Vector(1, -1), Tilt);
            Edges[3] = PlateCoordinatesToEucidean3DCoordinates(new Vector(1, 1), Tilt);
            return Edges;
        }

        #endregion

        #region Vectorcalcus

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
        /// Returns the angle between two Vectors
        /// </summary>
        /// <param name="a">Vector3D</param>
        /// <param name="b">Vector3D</param>
        /// <returns>Angle [Rad]</returns>
        public static double AngleBetwennVectors(Vector3D a, Vector3D b)
        { 
            return Math.Acos((Vector3D.DotProduct(a,b))/(a.Length*b.Length));
        }

        #endregion

        #region Trigonometry

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

        #endregion
    }
}