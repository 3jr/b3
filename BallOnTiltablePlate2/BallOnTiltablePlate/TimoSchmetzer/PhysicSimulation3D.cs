using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using BallOnTiltablePlate.JanRapp.Utilities.Vectors;
using BallOnTiltablePlate.JanRapp.Simulation;

namespace BallOnTiltablePlate.TimoSchmetzer
{
    class PhysicSimulation3D
    {
        #region info
        public String Author = "Timo Schmetzer";
        public Version Version = new Version(0, 6);
        public String Comment = "Numerical Simulation";
        #endregion

        public void RunSimulation(IPhysicsState state, double elapsedSeconds)
        { 
            
        }
    }
}
