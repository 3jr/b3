using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using BallOnTiltablePlate.TimoSchmetzer.Utilities;

namespace BallOnTiltablePlate.TimoSchmetzer.Simulation.PhysicsCalculators
{
    class LagrangePhysics : IPhysicsCalculator
    {
        public void CalcPhysics(IPhysicsState state, double elapsedSeconds)
        {
            double phiX = state.Tilt.X;
            double phiY = state.Tilt.Y;
            double tanPhiX = Math.Tan(phiX);
            double tanPhiY = Math.Tan(phiY);
            double g = state.Gravity;
            state.Acceleration = new Vector3D(
                (0.6e0 * g* tanPhiX)/(1+tanPhiX*tanPhiX+tanPhiY*tanPhiY),
                (0.6e0 * g * tanPhiY) / (1 + tanPhiX * tanPhiX + tanPhiY * tanPhiY)
                , 0);

            Physics.CalcMovement(state, elapsedSeconds);
            state.Position = new Point3D(state.Position.X, state.Position.Y, state.Position.X*tanPhiX+state.Position.Y*tanPhiY);
            state.Velocity = new Vector3D(state.Velocity.X, state.Velocity.Y, state.Velocity.X * tanPhiX + state.Velocity.Y * tanPhiY);
            state.Acceleration = new Vector3D(state.Acceleration.X, state.Acceleration.Y, state.Acceleration.X * tanPhiX + state.Acceleration.Y * tanPhiY);
            state.Tilt += state.PlateVelocity * elapsedSeconds;
        }


    }
}
