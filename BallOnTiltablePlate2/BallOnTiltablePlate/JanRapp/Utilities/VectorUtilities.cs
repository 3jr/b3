using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace BallOnTiltablePlate.JanRapp.Utilities.Vectors
{
    //MathUtilities by http://www.vcskicks.com/3d_gdiplus_drawing.php
    //All gegrees are taken in Radian and Metods with Deg suffix take it in Degree.
    //Added calculateNormalVector
    //Used Extension Methods   Changed by Jan Rapp at 10:17 9/10/2011
    public static class MathUtil
    {
        public static Point3D RotateXRad(this Point3D point3D, double degrees)
        {
            //Here we use Euler's matrix formula for rotating a 3D point x degrees around the x-axis

            //[ a  b  c ] [ x ]   [ x*a + y*b + z*c ]
            //[ d  e  f ] [ y ] = [ x*d + y*e + z*f ]
            //[ g  h  i ] [ z ]   [ x*g + y*h + z*i ]

            //[ 1    0        0   ]
            //[ 0   cos(x)  sin(x)]
            //[ 0   -sin(x) cos(x)]

            double cosDegrees = Math.Cos(degrees);
            double sinDegrees = Math.Sin(degrees);

            double y = (point3D.Y * cosDegrees) + (point3D.Z * sinDegrees);
            double z = (point3D.Y * -sinDegrees) + (point3D.Z * cosDegrees);

            return new Point3D(point3D.X, y, z);
        }

        public static Point3D RotateXDeg(this Point3D point3D, double degrees)
        {
            double cDegrees = (Math.PI * degrees) / 180.0; //Convert degrees to radian for .Net Cos/Sin functions
            return RotateXRad(point3D, cDegrees);
        }

        public static Point3D RotateYRad(this Point3D point3D, double degrees)
        {
            //Y-axis

            //[ cos(x)   0    sin(x)]
            //[   0      1      0   ]
            //[-sin(x)   0    cos(x)]

            double cosDegrees = Math.Cos(degrees);
            double sinDegrees = Math.Sin(degrees);

            double x = (point3D.X * cosDegrees) + (point3D.Z * sinDegrees);
            double z = (point3D.X * -sinDegrees) + (point3D.Z * cosDegrees);

            return new Point3D(x, point3D.Y, z);
        }

        public static Point3D RotateYDeg(this Point3D point3D, double degrees)
        {
            double cDegrees = (Math.PI * degrees) / 180.0; //Radians
            return RotateYRad(point3D, cDegrees);
        }

        public static Point3D RotateZRad(this Point3D point3D, double degrees)
        {
            //Z-axis

            //[ cos(x)  sin(x) 0]
            //[ -sin(x) cos(x) 0]
            //[    0     0     1]

            double cosDegrees = Math.Cos(degrees);
            double sinDegrees = Math.Sin(degrees);

            double x = (point3D.X * cosDegrees) + (point3D.Y * sinDegrees);
            double y = (point3D.X * -sinDegrees) + (point3D.Y * cosDegrees);

            return new Point3D(x, y, point3D.Z);
        }

        public static Point3D RotateZDeg(this Point3D point3D, double degrees)
        {
            double cDegrees = (Math.PI * degrees) / 180.0; //Radians
            return RotateZRad(point3D, cDegrees);
        }

        public static Point3D Translate(this Point3D points3D, Point3D oldOrigin, Point3D newOrigin)
        {
            //Moves a 3D point based on a moved reference point
            Point3D difference = new Point3D(newOrigin.X - oldOrigin.X, newOrigin.Y - oldOrigin.Y, newOrigin.Z - oldOrigin.Z);
            points3D.X += difference.X;
            points3D.Y += difference.Y;
            points3D.Z += difference.Z;
            return points3D;
        }

        //These are to make the above functions workable with arrays of 3D points
        public static Point3D[] RotateXRad(this Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateXRad(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] RotateXDeg(this Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateXDeg(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] RotateYRad(this Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateYRad(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] RotateYDeg(this Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateYDeg(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] RotateZRad(this Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateZRad(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] RotateZDeg(this Point3D[] points3D, double degrees)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateZDeg(points3D[i], degrees);
            }
            return points3D;
        }

        public static Point3D[] Translate(this Point3D[] points3D, Point3D oldOrigin, Point3D newOrigin)
        {
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = Translate(points3D[i], oldOrigin, newOrigin);
            }
            return points3D;
        }

        /// <summary>
        /// Berechnet den Normalenvektor der Platte
        /// </summary>
        /// <returns>Normalenvektror der Platte als Vector3D</returns>
        public static Vector3D Get3DNormalizedVector(this Vector vec)
        {
            Vector3D a = (Vector3D)RotateYRad(
                RotateXRad(new Point3D(-1, -1, 0), -(vec.Y)), -(vec.X));
            Vector3D b = (Vector3D)RotateYRad(
                RotateXRad(new Point3D(1, -1, 0), -(vec.Y)), -(vec.X));
            Vector3D c = (Vector3D)RotateYRad(
                RotateXRad(new Point3D(-1, 1, 0), -(-1)*(vec.Y)), -(vec.X));//g removed of: MathUtilities.RotateXRad(new Point3D(-1, 1, 0), -g(vec.Y)), -(vec.X));
            Vector3D n = Vector3D.CrossProduct(b - a, c - a);
            n.Normalize();
            return n;
        }

        public static double DegToRad(double degrees)
        {
            return (Math.PI * degrees) / 180.0;
        }

        public static double RadToDeg(double radian)
        {
            return (radian / Math.PI) * 180.0;
        }

        public static QuaternionRotation3D CombineSimutaniousRotationsAroundXAndYAxis(Vector rotationAroundXAndYAxis)
        {
            var result = new QuaternionRotation3D();
            CombineSimutaniousRotationsAroundXAndYAxis(rotationAroundXAndYAxis, ref result);
            return result;
        }

        public static void CombineSimutaniousRotationsAroundXAndYAxis(Vector rotationAroundXAndYAxis, ref QuaternionRotation3D result)
        {
            
            if (rotationAroundXAndYAxis == null) throw new ArgumentNullException("rotationAroundXAndYAxis");
            if (result == null) throw new ArgumentNullException("result");

            Vector3D firstAxisForXRotation = new Vector3D(1.0,0.0,0.0);
            Vector3D secoundAxisForYRotation = new Vector3D(0, Math.Cos(rotationAroundXAndYAxis.X), Math.Sin(rotationAroundXAndYAxis.X));
            double xAngle = (rotationAroundXAndYAxis.X);
            double yAngle = (rotationAroundXAndYAxis.Y);

            Quaternion fistRotation = new Quaternion(firstAxisForXRotation, RadToDeg(xAngle));
            Quaternion secoundRotaion = new Quaternion(secoundAxisForYRotation, RadToDeg(yAngle));

            Quaternion resultQuaternion = secoundRotaion * fistRotation;

            if (resultQuaternion != result.Quaternion)
            {
                if (result.IsFrozen) throw new ArgumentException("result may not be Frozen and has to be changed to fulfill the requirements for this Function");

                result.Quaternion = resultQuaternion;
            }
        }
    }
}

