using System.Collections.Generic;
using GameOfLife.Simulation;

namespace GameOfLife.Simulation.Strategies
{
    public static class SimulationStrategyFactory
    {
        public static Dictionary<SimulationType, ISimulationStrategy> CreateStrategies()
        {
            return new Dictionary<SimulationType, ISimulationStrategy>
            {
                { SimulationType.Classic, new ClassicSimulation() },
                { SimulationType.Burst, new BurstSimulation() }
            };
        }
    }
}
