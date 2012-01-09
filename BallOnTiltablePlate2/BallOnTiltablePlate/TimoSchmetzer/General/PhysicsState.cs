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

namespace BallOnTiltablePlate.TimoSchmetzer.Physics
{
    /// <summary>
    /// Represents a physical state of the plate.
    /// Used in Physics3D/Physical Utilities
    /// </summary>
    public class PhysicsState
    {
        #region Constats
        /// <summary>
        /// Contains the local Gravity in (m)/(s^2) (with a negative algebraic sign)
        /// </summary>
        public double Gravity; //= -9.81;

        /// <summary>
        /// Contains which part of the original hight may be achieved.
        /// Enthaellt, welchen anteil der Ausgangshoehe bei Reflektion wieder erreicht werden soll.
        /// </summary>
        public double HitAttenuationFactor; // = 1;

        /// <summary>
        /// Absolute Velocity Reduction on a hit.
        /// Absolute Geschwindigkeitsreduktion bei Aufprall
        /// </summary>
        public double AbsoluteAbsorbtion; //= 0.08;

        #endregion

        #region Status Properties
        /// <summary>
        /// Contains the rotaition of the plate against the axis in Rad.
        /// Enthaellt die Kippung der Platte als Vector in x/y Richtung in Radiant.
        /// </summary>
        public Vector Tilt;

        /// <summary>
        /// Contains the Position [of the Ball] as Vector3D.
        /// Enthaellt die Position [des Balls] als Vector3D.
        /// </summary>
        public Point3D Position;

        /// <summary>
        /// Contains the Velocity of the Ball as Vector3D.
        /// Enthaellt die Geschwindigkeit [des Balls] als Vector3D.
        /// </summary>
        public Vector3D Velocity;

        /// <summary>
        /// Contains the Acceleration [of the Ball] as Vector3D.
        /// Enthaellt die Beschleunigung [des Balls] als Vector3D.
        /// </summary>
        public Vector3D Acceleration;

        /// <summary>
        /// AngleVelocity the plate is turned with
        /// </summary>
        public Vector PlateVelocity;

        #endregion
    }
}