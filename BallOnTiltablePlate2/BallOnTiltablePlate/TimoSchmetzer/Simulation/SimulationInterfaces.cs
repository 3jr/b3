using System.Windows;
using System.Windows.Media.Media3D;

namespace BallOnTiltablePlate.TimoSchmetzer.Simulation
{
    /// <summary>
    /// Interface used for Interaction with the User.
    /// Contains Desired Quantities an Constats rather than real quantities.
    /// </summary>
    public interface SimulationState
    {
        #region Constats
        /// <summary>
        /// Contains the local Gravity in (m)/(s^2) (with a negative algebraic sign)
        /// </summary>
        double Gravity { get; } //= -9.81;

        /// <summary>
        /// Contains which part of the original hight may be achieved.
        /// Enthaellt, welchen anteil der Ausgangshoehe bei Reflektion wieder erreicht werden soll.
        /// </summary>
        double HitAttenuationFactor { get; } // = 1;

        /// <summary>
        /// Plate Velocity the Plate is turned with if DesiredTilt is not equal to Tilt.
        /// </summary>
        Vector PlateVelocity { get; }

        /// <summary>
        /// Absolute Velocity Reduction on a hit.
        /// Absolute Geschwindigkeitsreduktion bei Aufprall
        /// </summary>
        double AbsoluteAbsorbtion { get; } //= 0.08;

        #endregion

        #region Status Properties
        /// <summary>
        /// Contains the rotaition of the plate against the axis in Rad.
        /// Enthaellt die Kippung der Platte als Vector in x/y Richtung in Radiant.
        /// </summary>
        Vector Tilt { get; set; }

        /// <summary>
        /// The Tilt desired by User.
        /// </summary>
        Vector DesiredTilt { get; }

        /// <summary>
        /// Contains the Position [of the Ball] as Vector3D.
        /// Enthaellt die Position [des Balls] als Vector3D.
        /// </summary>
        Point3D Position { get; set; }

        /// <summary>
        /// Contains the Velocity of the Ball as Vector3D.
        /// Enthaellt die Geschwindigkeit [des Balls] als Vector3D.
        /// </summary>
        Vector3D Velocity { get; set; }

        /// <summary>
        /// Contains the Acceleration [of the Ball] as Vector3D.
        /// Enthaellt die Beschleunigung [des Balls] als Vector3D.
        /// </summary>
        Vector3D Acceleration { get; set; }
        #endregion
    }

    /// <summary>
    /// Interface used for PhysicCalculations. (PhysicCalculators, Utilites)
    /// Represents a physical state of the plate.
    /// Contains real quantities, not maximum Values or Controll variables.
    /// </summary>
    public interface PhysicsState
    {
        #region Constats
        /// <summary>
        /// Contains the local Gravity in (m)/(s^2) (with a negative algebraic sign)
        /// </summary>
        double Gravity { get; } //= -9.81;

        /// <summary>
        /// Contains which part of the original hight may be achieved.
        /// Enthaellt, welchen anteil der Ausgangshoehe bei Reflektion wieder erreicht werden soll.
        /// </summary>
        double HitAttenuationFactor { get; } // = 1;

        /// <summary>
        /// Absolute Velocity Reduction on a hit.
        /// Absolute Geschwindigkeitsreduktion bei Aufprall
        /// </summary>
        double AbsoluteAbsorbtion { get; } //= 0.08;

        #endregion

        #region Status Properties
        /// <summary>
        /// Contains the rotaition of the plate against the axis in Rad.
        /// Angle is Angle against the axis.
        /// Enthaellt die Kippung der Platte als Vector in x/y Richtung in Radiant.
        /// </summary>
        Vector Tilt { get; set; }

        /// <summary>
        /// Contains the Position [of the Ball] as Vector3D.
        /// Enthaellt die Position [des Balls] als Vector3D.
        /// </summary>
        Point3D Position { get; set; }

        /// <summary>
        /// Contains the Velocity of the Ball as Vector3D.
        /// Enthaellt die Geschwindigkeit [des Balls] als Vector3D.
        /// </summary>
        Vector3D Velocity { get; set; }

        /// <summary>
        /// Contains the Acceleration [of the Ball] as Vector3D.
        /// Enthaellt die Beschleunigung [des Balls] als Vector3D.
        /// </summary>
        Vector3D Acceleration { get; set; }

        /// <summary>
        /// AngleVelocity the plate is turned with. Angle is Angle against the axis.
        /// </summary>
        Vector PlateVelocity { get; }

        #endregion
    }

    /// <summary>
    /// Implementions do calc Physical state after elapedSeconds.
    /// </summary>
    public interface PhysicsCalculator
    {
        /// <summary>
        /// Method, that does the Physical Calculations
        /// </summary>
        /// <param name="state">Physical State</param>
        /// <param name="elapsedSeconds">Time to Elapse for which to calc.</param>
        void CalcPhysics(PhysicsState state, double elapsedSeconds);
    }
}