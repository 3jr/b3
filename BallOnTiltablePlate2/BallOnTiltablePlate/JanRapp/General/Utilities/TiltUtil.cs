using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace BallOnTiltablePlate.JanRapp.Utilities
{
    static class TiltUtil
    {
        public static double DegToRad(double degrees)
        {
            return (Math.PI * degrees) / 180.0;
        }

        public static double RadToDeg(double radian)
        {
            return (radian / Math.PI) * 180.0;
        }

        public static Quaternion RotationForTilt(Vector sequentalTilt) //c
        {
            Vector3D firstAxisForYRotation = new Vector3D(1.0, 0.0, 0.0); //X-Axis
            Vector3D secoundAxisForXRotation = new Vector3D(0.0, 1.0, Math.Tan(sequentalTilt.Y)); //Y-Axis Rotated by seqentialTilt.Y

            Quaternion firstRotation = new Quaternion(firstAxisForYRotation, RadToDeg(sequentalTilt.Y));
            Quaternion secoundRotaion = new Quaternion(secoundAxisForXRotation, RadToDeg(sequentalTilt.X));

            return secoundRotaion * firstRotation; //it just is that way with Quaternions
        }

        public static Vector3D NormalVectorFromSequentialTilt(Vector sequentalTilt)
        {
            Quaternion rotation = TiltUtil.RotationForTilt(sequentalTilt);
            RotateTransform3D rotationTransform = new RotateTransform3D(new QuaternionRotation3D(rotation));

            return rotationTransform.Transform(new Vector3D(0.0, 0.0, 1.0));
        }

        public static Vector3D NormalVectorFromTiltToAxes(Vector tiltToAxis)
        {
            return new Vector3D(-Math.Tan(tiltToAxis.X), -Math.Tan(tiltToAxis.Y), 1);
        }

        public static Vector TiltToAxis(Vector3D normalVector)
        {
            normalVector = normalVector / normalVector.Z;

            return new Vector(-Math.Atan(normalVector.X), -Math.Atan(normalVector.Y));
        }

        public static Vector ToTiltToAxis(this Vector sequentalTilt)
        {
            Vector3D normal = TiltUtil.NormalVectorFromSequentialTilt(sequentalTilt);

            return TiltToAxis(normal);
        }

        public static Vector ToSequentailTilt(this Vector tiltToAxis) //c
        {
            Vector seqentialTilt = new Vector();

            seqentialTilt.Y = tiltToAxis.Y;

            Vector3D normal = NormalVectorFromTiltToAxes(tiltToAxis);
            Quaternion negativYRotation = new Quaternion(new Vector3D(1, 0, 0), RadToDeg(-seqentialTilt.Y));
            RotateTransform3D negativYRotationTransform = new RotateTransform3D(new QuaternionRotation3D(negativYRotation));

            Vector3D normalForXAxis = negativYRotationTransform.Transform(normal);

            seqentialTilt.X = Math.Atan(normalForXAxis.X / normalForXAxis.Z);

            return seqentialTilt;
        }
    }
}
