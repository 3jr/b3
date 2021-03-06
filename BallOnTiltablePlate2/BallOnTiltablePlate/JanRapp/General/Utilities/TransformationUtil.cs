﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace BallOnTiltablePlate.JanRapp.Utilities
{
    //MathUtilities by http://www.vcskicks.com/3d_gdiplus_drawing.php
    //All gegrees are taken in Radian and Metods with Deg suffix take it in Degree.
    //Added calculateNormalVector
    //Used Extension Methods   Changed by Jan Rapp at 10:17 9/10/2011
    public static class TransformationUtil
    {
        #region Rotation and Transformation of Points

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

        #endregion Rotation and Transformation of Points

        public static Vector PerspectiveProjection(Vector3D point, double cameraConstant)
        {
            return new Vector(
               cameraConstant * point.X / point.Z,
               cameraConstant * point.Y / point.Z);
        }

        public static Vector[] PerspectiveProjection(Vector3D[] points, double cameraConstant)
        {
            Vector[] result = new Vector[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                result[i] = PerspectiveProjection(points[i], cameraConstant);
            }

            return result;
        }
    }
}