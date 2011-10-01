using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BallOnTiltablePlate.JanRapp.Simulation

namespace BallOnTiltablePlate.TimoSchmetzer
{
    /// <summary>
    /// Call only to prevent Comiler errors, using not existant Property Angle Velocity
    /// You may delete this and add Property AngleVelocity to IPhysicsState
    /// </summary>
    public static class tmp
    {
        public static Vector AngleVelocity(this IPhysicsState state)
        {
            return new Vector(0,0);
        }
    }
}
