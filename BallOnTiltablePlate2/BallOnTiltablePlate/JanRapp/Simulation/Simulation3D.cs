using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallOnTiltablePlate.JanRapp.Simulation
{
    [BallOnPlateItemInfo("Timo", "Schmetzer", "Simulation3D", "0.1")]
    class Simulation3D : SimulationBase
    {
        public override void Update(double deltaSeconds)
        {
            TimoSchmetzer.PhysicSimulation3D.RunSimulation(this, deltaSeconds);
        }
    }
}
