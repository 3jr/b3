using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BallOnTiltablePlate.JanRapp.Simulation
{
    [BallOnPlateItemInfo("Timo", "Schmetzer", "SimulationOnPlate", "0.1")]
    class SimulationOnPlate : SimulationBase
    {
        public override void Update(double deltaSeconds)
        {
            TimoSchmetzer.PhysicSimulationOnPlate.RunSimulation(this, deltaSeconds);
        }
    }
}
