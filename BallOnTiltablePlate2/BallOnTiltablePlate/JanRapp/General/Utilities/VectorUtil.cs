using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace BallOnTiltablePlate.JanRapp.Utilities
{
    public static class VectorUtil
    {
        public static Vector NaNVector = new Vector(double.NaN, double.NaN);

        public static Vector3D NaNVector3D = new Vector3D(double.NaN, double.NaN, double.NaN);

        public static Vector ZeroVector = new Vector(0.0, 0.0);

        public static Vector3D ZeroVector3D = new Vector3D(0.0, 0.0, 0.0);        

        public static double ToNoNaN(this double d)
        {
            if (double.IsNaN(d))
                d = 0.0;
            return d;
        }
        
        public static Vector ToNoNaN(this Vector v)
        {
            if (double.IsNaN(v.X))
                v.X = 0.0;
            if (double.IsNaN(v.Y))
                v.Y = 0.0;
            return v;
        }

        public static Vector3D ToNoNaN(this Vector3D v)
        {
            if (double.IsNaN(v.X))
                v.X = 0.0;
            if (double.IsNaN(v.Y))
                v.Y = 0.0;
            if (double.IsNaN(v.Z))
                v.Z = 0.0;
            return v;
        }

        public static bool IsNaN(this double d)
        {
            return double.IsNaN(d);
        }

        public static bool HasNaN(this Vector v)
        {
            return double.IsNaN(v.X) && double.IsNaN(v.Y);
        }

        public static bool HasNaN(this Vector3D v)
        {
            return double.IsNaN(v.X) && double.IsNaN(v.Y) && double.IsNaN(v.Z);
        }

        public static Vector ToYAxisInverted(this Vector v)
        {
            return new Vector(v.X, -v.Y);
        }

        public static Vector MutliplyEachWith(this Vector v, Vector v1)
        {
            return new Vector(v.X * v1.X, v.Y * v1.Y);
        }

        public static Vector3D MutliplyEachWith(this Vector3D v, Vector3D v1)
        {
            return new Vector3D(v.X * v1.X, v.Y * v1.Y, v.Z * v1.Z);
        }

        public static Vector ToVector2D(this Vector3D vec)
        {
            return new Vector(vec.X, vec.Y);
        }

        public static Vector GetNormalized(this Vector v)
        {
            return v / v.Length;
        }
    }
}

