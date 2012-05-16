using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BallOnTiltablePlate.TimoSchmetzer.Utilities
{
    public static class Mathematics
    {

        #region LinearRegression

        /// <summary>
        /// Represents the equation of a line.
        /// f(x) = m*x+c
        /// </summary>
        public struct LineEquation
        {
            public double m;
            public double c;
        }

        /// <summary>
        /// Searches a line Equation fitting best for all Points using linear Regression.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static LineEquation GetLineEquation(IEnumerable<Point> values)
        {
            //Do Linear Regression
            int n = values.Count();
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

        public static Point3D RotateTransform(Vector3D axis, double rad, Point3D OriginalPoint)
        {
            Matrix3x3 R = RotateTransformationMatrix(axis, rad);
            return R * OriginalPoint;
        }

        public static Point3D ApplyTransformationMatrix(Matrix3x3 TransformationMatrix, Point3D OriginalPoint)
        {
            return TransformationMatrix * OriginalPoint;
        }

        public static Matrix3x3 RotateTransformationMatrix(Vector3D axis, double rad)
        {
            axis.Normalize();
            return new Matrix3x3(Math.Cos(rad) + (axis.X * axis.X) * (1 - Math.Cos(rad)), axis.X * axis.Y * (1 - Math.Cos(rad)) - axis.Z * Math.Sin(rad), axis.X * axis.Z * (1 - Math.Cos(rad)) + axis.Y * Math.Sin(rad),
                                    axis.X * axis.Y * (1 - Math.Cos(rad)) + axis.Z * Math.Sin(rad), Math.Cos(rad) + axis.Y * axis.Y * (1 - Math.Cos(rad)), axis.Z * axis.Y * (1 - Math.Cos(rad)) - axis.X * Math.Sin(rad),
                                    axis.X * axis.Z * (1 - Math.Cos(rad)) - axis.Y * Math.Sin(rad), axis.Z * axis.Y * (1 - Math.Cos(rad)) + axis.X * Math.Sin(rad), Math.Cos(rad) + (axis.Z * axis.Z) * (1 - Math.Cos(rad)));
        }
        #endregion

        #region TiltTransforms
        /// <summary>
        /// Returns a Quarternion that is able to transform a point on the xy-Layer to a Point on the tilted plate.
        /// Copied from JanRapp's TiltUtil
        /// </summary>
        /// <param name="sequentalTilt">The tilt for which to calc.</param>
        /// <returns>See summary.</returns>
        public static Quaternion RotationForTilt(Vector sequentalTilt) //c
        {
            Vector3D firstAxisForYRotation = new Vector3D(1.0, 0.0, 0.0); //X-Axis
            Vector3D secoundAxisForXRotation = new Vector3D(0.0, 1.0, 0.0);// Math.Tan(sequentalTilt.Y)); //Y-Axis Rotated by seqentialTilt.Y

            Quaternion firstRotation = new Quaternion(firstAxisForYRotation, RadToDeg(sequentalTilt.Y));
            Quaternion secoundRotaion = new Quaternion(secoundAxisForXRotation, RadToDeg(sequentalTilt.X));

            return firstRotation * secoundRotaion; //it just is that way with Quaternions
        }

        /// <summary>
        /// Returns a RotateTransform3D that is able to transform a point on the xy-Layer to a Point on the tilted plate.
        /// Copied from JanRapp's TiltUtil
        /// </summary>
        /// <param name="sequentalTilt">The tilt for which to calc.</param>
        /// <returns>See summary.</returns>
        public static RotateTransform3D RotateTransformForTilt(Vector sequentalTilt)
        {
            Quaternion rotation = Mathematics.RotationForTilt(sequentalTilt);
            RotateTransform3D rotationTransform = new RotateTransform3D(new QuaternionRotation3D(rotation));
            return rotationTransform;
        }

        /// <summary>
        /// Returns a Quarternion that is able to transform a Point on the tilted plate to a point on the xy-Layer.
        /// </summary>
        /// <param name="sequentalTilt">The tilt for which to calc.</param>
        /// <returns>See summary.</returns>
        public static Quaternion InverseRotationForTilt(Vector sequentalTilt) //c
        {
            Vector3D firstAxisForYRotation = new Vector3D(1.0, 0.0, 0.0); //X-Axis
            Vector3D secoundAxisForXRotation = new Vector3D(0.0, 1.0, 0.0);// Math.Tan(sequentalTilt.Y)); //Y-Axis Rotated by seqentialTilt.Y

            Quaternion firstRotation = new Quaternion(firstAxisForYRotation, -RadToDeg(sequentalTilt.Y));
            Quaternion secoundRotaion = new Quaternion(secoundAxisForXRotation, -RadToDeg(sequentalTilt.X));

            return secoundRotaion * firstRotation; 
        }

        /// <summary>
        /// Returns a Quarternion that is able to transform a Point on the tilted plate to a point on the xy-Layer.
        /// </summary>
        /// <param name="sequentalTilt">The tilt for which to calc.</param>
        /// <returns>See summary.</returns>
        public static RotateTransform3D InverseRotateTransformForTilt(Vector sequentalTilt)
        {
            Quaternion rotation = Mathematics.InverseRotationForTilt(sequentalTilt);
            RotateTransform3D rotationTransform = new RotateTransform3D(new QuaternionRotation3D(rotation));
            return rotationTransform;
        }

        /// <summary>
        /// Transforms a point on a tilted Plate to the Point with an other Tilt
        /// </summary>
        /// <param name="Point">The Point to Transform</param>
        /// <param name="oldsequentialTilt">The Tilt that has the plate the point is on</param>
        /// <param name="newsequentialTilt">The Tilt which the transformed platepoint will have</param>
        public static Point3D TransformVectorToDifferentTilt(Point3D Point, Vector oldsequentialTilt, Vector newsequentialTilt)
        {
            Point = Mathematics.InverseRotateTransformForTilt(oldsequentialTilt).Transform(Point);
            Point = Mathematics.RotateTransformForTilt(newsequentialTilt).Transform(Point);
            return Point;
        }
        #endregion

        #region PlaneCalcs

        /// <summary>
        /// Returns the Normal Vector of a plane with the specified turnations around the axis.
        /// Vector is NOT NORMALIZED.
        /// </summary>
        /// <param name="Tilt">Rotation given in Rad</param>
        /// <returns>Normal Vector</returns>
        public static Vector3D CalcNormalVectorSequentialTilt(Vector SequentialTilt)
        {
            Vector3D NormalVector = new Vector3D();
            Vector3D a = (Vector3D)TransformPlatePoint(new Vector(0, 1), SequentialTilt);
            Vector3D b = (Vector3D)TransformPlatePoint(new Vector(1, 0), SequentialTilt);
            NormalVector = Vector3D.CrossProduct(a, b);
            return NormalVector;
        }

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
        /// <param name="SequentialTilt">Tilt of the plate</param>
        /// <returns></returns>
        public static Point3D TransformPlatePoint(Vector PlateCoordinate, Vector SequentialTilt)
        {
            Point3D p = new Point3D(PlateCoordinate.X, PlateCoordinate.Y, 0);
            p = RotateTransform(new Vector3D(0.0, 1.0, 0.0), SequentialTilt.X, p);
            p = RotateTransform(new Vector3D(1.0, 0.0, 0.0), SequentialTilt.Y, p);
            return p;
        }

        /// <summary>
        /// Fusspunkt berechnen, fuer Ebene der Form x*n = 0.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="NormalVector"></param>
        /// <returns></returns>
        public static Point3D CalcFootOfPerpendicular(Point3D p, Vector3D NormalVector) 
        {
            double t = -(NormalVector.X * p.X + NormalVector.Y * p.Y + NormalVector.Z * p.Z)
                / (NormalVector.X * NormalVector.X + NormalVector.Y * NormalVector.Y + NormalVector.Z * NormalVector.Z);
            return p + t * NormalVector;
        }

        public static Vector ToSequentialTilt(Vector Tilt)
        {
            return new Vector(
                -Math.Sign(Tilt.X)*Math.Acos(1/(Math.Cos(Tilt.Y)*Math.Sqrt(1+Math.Tan(Tilt.X)*Math.Tan(Tilt.X)+Math.Tan(Tilt.Y)*Math.Tan(Tilt.Y))))
                ,Tilt.Y);
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
            return Math.Acos((Vector3D.DotProduct(a, b)) / (a.Length * b.Length));
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

        #region Matrix
        /// <summary>
        /// Represents a 3x3 Matrix.
        /// Indexes starting at 0.
        /// Does some standard math operators.
        /// </summary>
        public class Matrix3x3
        {
            double[,] data = new double[3,3];

            public Matrix3x3(double[,] MatrixData)
            {
                Array.Copy(MatrixData, data, data.Length);
            }

            public Matrix3x3(Matrix3x3 matrix)
            {
                Array.Copy(matrix.data, this.data, this.data.Length);
            }

            public Matrix3x3(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22)
            {
                data[0, 0] = m00;
                data[0, 1] = m01;
                data[0, 2] = m02;
                data[1, 0] = m10;
                data[1, 1] = m11;
                data[1, 2] = m12;
                data[2, 0] = m20;
                data[2, 1] = m21;
                data[2, 2] = m22;
            }

            public double this[int indexi, int indexj]
            {
                get { return data[indexi, indexj]; }
                set { data[indexi, indexj] = value; }
            }

            public static Matrix3x3 operator +(Matrix3x3 arg0, Matrix3x3 arg1)
            {
                return new Matrix3x3(arg0[0, 0] + arg1[0, 0], arg0[0, 1] + arg1[0, 1], arg0[0, 2] + arg1[0, 2],
                    arg0[1, 0] + arg1[1, 0], arg0[1, 1] + arg1[1, 1], arg0[1, 2] + arg1[1, 2],
                    arg0[2, 0] + arg1[2, 0], arg0[2, 1] + arg1[2, 1], arg0[2, 2] + arg1[2, 2]);
            }

            public static Matrix3x3 operator *(double arg0,Matrix3x3 arg1)
            {
                return new Matrix3x3(arg0* arg1[0, 0], arg0 * arg1[0, 1], arg0 * arg1[0, 2],
                    arg0* arg1[1, 0], arg0* arg1[1, 1], arg0* arg1[1, 2],
                    arg0* arg1[2, 0], arg0* arg1[2, 1], arg0* arg1[2, 2]);
            }

            public static Matrix3x3 operator *(Matrix3x3 arg0, Matrix3x3 arg1)
            {
                return new Matrix3x3(
                    arg0[0, 0] * arg1[0, 0] + arg0[0, 1] * arg1[1, 0] + arg0[0, 2] * arg1[2, 0], arg0[0, 0] * arg1[0, 1] + arg0[0, 1] * arg1[1, 1] + arg0[0, 2] * arg1[2, 1], arg0[0, 0] * arg1[0, 2] + arg0[0, 1] * arg1[1, 2] + arg0[0, 2] * arg1[2, 2],
                    arg0[1, 0] * arg1[0, 0] + arg0[1, 1] * arg1[1, 0] + arg0[1, 2] * arg1[2, 0], arg0[1, 0] * arg1[0, 1] + arg0[1, 1] * arg1[1, 1] + arg0[1, 2] * arg1[2, 1], arg0[1, 0] * arg1[0, 2] + arg0[1, 1] * arg1[1, 2] + arg0[1, 2] * arg1[2, 2],
                    arg0[2, 0] * arg1[0, 0] + arg0[2, 1] * arg1[1, 0] + arg0[2, 2] * arg1[2, 0], arg0[2, 0] * arg1[0, 1] + arg0[2, 1] * arg1[1, 1] + arg0[2, 2] * arg1[2, 1], arg0[2, 0] * arg1[0, 2] + arg0[2, 1] * arg1[1, 2] + arg0[2, 2] * arg1[2, 2]);
            }

            public static Vector3D operator *(Matrix3x3 arg0, Vector3D arg1)
            {
                return new Vector3D(arg0[0, 0] * arg1.X + arg0[0, 1] * arg1.Y + arg0[0, 2] * arg1.Z,
                    arg0[1, 0] * arg1.X + arg0[1, 1] * arg1.Y + arg0[1, 2] * arg1.Z,
                    arg0[2, 0] * arg1.X + arg0[2, 1] * arg1.Y + arg0[2, 2] * arg1.Z);
            }

            public static Point3D operator *(Matrix3x3 arg0, Point3D arg1)
            {
                return new Point3D(arg0[0, 0] * arg1.X + arg0[0, 1] * arg1.Y + arg0[0, 2] * arg1.Z,
                    arg0[1, 0] * arg1.X + arg0[1, 1] * arg1.Y + arg0[1, 2] * arg1.Z,
                    arg0[2, 0] * arg1.X + arg0[2, 1] * arg1.Y + arg0[2, 2] * arg1.Z);
            }
        }
        #endregion

        #region Random
        /// <summary>
        /// Returns a Random number in the specified Range
        /// </summary>
        /// <param name="min">Minimum value of random number</param>
        /// <param name="max">Maximum value of random number</param>
        /// <returns>Random number in the specified Range</returns>
        public static double Random(double min, double max)
        {
            Random r = new Random();
            return min + (max - min) * r.NextDouble();
        }
        #endregion

        #region Numerics
        /// <summary>
        /// Does Numeric Differential Equation solving for an equation of Type:
        /// dx/dt = f(t,x)
        /// </summary>
        /// <param name="f">Funktion of t,x for dx/dt</param>
        /// <param name="t">Time of Initial Value</param>
        /// <param name="h">Stepwide</param>
        /// <param name="x0">Value for Initial Time</param>
        /// <returns>Value at time t+h</returns>
        public static double RungeKutta44(Func<double, double, double> f, double t, double h, double x0)
        {
            double k1 = f(t, x0);
            double k2 = f(t + 0.5 * h, x0 + 0.5 * h * k1);
            double k3 = f(t + 0.5 * h, x0 + 0.5 * h * k2);
            double k4 = f(t + h, x0 + h * k3);
            return x0 + (h / 6) * (k1 + 2 * k2 + 2 * k3 + k4);
        }
        #endregion

    }
}
