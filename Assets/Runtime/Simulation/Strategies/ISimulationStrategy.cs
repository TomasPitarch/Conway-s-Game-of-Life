using Unity.Collections;
using UnityEngine;

namespace GameOfLife.Simulation.Strategies
{
    public interface ISimulationStrategy
    {
        void ExecuteStep(
            ref NativeArray<bool> currentGen,
            ref NativeArray<bool> nextGen,
            ref NativeArray<Color32> colors,
            int width, int height,
            Color32 liveCol, Color32 deadCol);
    }
}
