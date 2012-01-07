using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallOnTiltablePlate.JanRapp.Simulation
{
    [BallOnPlateItemInfo("Timo", "Schmetzer", "SimulationInAir", "0.1")]
    class SimulationInAir : SimulationBase
    {
        public override void Update(double deltaSeconds)
        {
            TimoSchmetzer.PhysicSimulationInAir.RunSimulation(this, deltaSeconds);
        }
    }
}
