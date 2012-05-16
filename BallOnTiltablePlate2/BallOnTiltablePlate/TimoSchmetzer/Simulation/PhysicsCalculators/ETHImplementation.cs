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
    class ETHImplementation : IPhysicsCalculator
    {
         public void CalcPhysics(IPhysicsState state, double elapsedSeconds)
        {
            if (elapsedSeconds == 0)
                return;
            Vector tiltnew = state.Tilt + state.PlateVelocity * elapsedSeconds;
           state.Tilt = Mathematics.ToSequentialTilt(state.Tilt);
           state.Position = Mathematics.TransformVectorToDifferentTilt(state.Position, state.Tilt, new Vector(0, 0));
           state.Velocity = (Vector3D)Mathematics.TransformVectorToDifferentTilt((Point3D)state.Velocity, state.Tilt, new Vector(0, 0));
           //CalcPhysicsTransformed(state, elapsedSeconds);
           state.Acceleration = new Vector3D(-(3e0 / 5e0) * state.Gravity * Math.Cos(state.Tilt.Y) * Math.Sin(state.Tilt.X),
  (3e0 / 5e0) * state.Gravity * Math.Sin(state.Tilt.Y), 0);
           state.Position += 0.5 * state.Acceleration * elapsedSeconds * elapsedSeconds+ state.Velocity*elapsedSeconds;
           state.Velocity += state.Acceleration * elapsedSeconds;
           state.Position = Mathematics.TransformVectorToDifferentTilt(state.Position, new Vector(0, 0), state.Tilt);
           state.Velocity = (Vector3D)Mathematics.TransformVectorToDifferentTilt((Point3D)state.Velocity, new Vector(0, 0), state.Tilt);
           state.Acceleration = (Vector3D)Mathematics.TransformVectorToDifferentTilt((Point3D)state.Acceleration, new Vector(0, 0), state.Tilt);
           state.Tilt = tiltnew;
           state.Position = new Point3D(state.Position.X, state.Position.Y, Mathematics.HightOfPlate(new Point(state.Position.X, state.Position.Y), Mathematics.CalcNormalVector(state.Tilt)));
         }
    }
}
