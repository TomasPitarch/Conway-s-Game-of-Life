using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using GameOfLife.Simulation.Strategies;

namespace GameOfLife.Tests
{
    public class SimulationTests
    {
        [Test]
        public void ClassicSimulation_BlinkerPattern_ChangesState()
        {
            // Setup a simple 3x3 grid
            int width = 3;
            int height = 3;
            NativeArray<bool> currentGen = new NativeArray<bool>(width * height, Allocator.Temp);
            NativeArray<bool> nextGen = new NativeArray<bool>(width * height, Allocator.Temp);
            NativeArray<Color32> colors = new NativeArray<Color32>(width * height, Allocator.Temp);

            // Set a blinker pattern (middle column)
            // 0 1 0
            // 0 1 0
            // 0 1 0
            currentGen[1] = true;
            currentGen[4] = true;
            currentGen[7] = true;

            ClassicSimulation simulation = new ClassicSimulation();
            simulation.ExecuteStep(ref currentGen, ref nextGen, ref colors, width, height, Color.white, Color.black);

            // Verify that something changed
            bool changed = false;
            for(int i = 0; i < nextGen.Length; i++)
            {
                if (nextGen[i] != currentGen[i])
                {
                    changed = true;
                    break;
                }
            }

            Assert.IsTrue(changed, "Simulation state should change for a blinker pattern");

            currentGen.Dispose();
            nextGen.Dispose();
            colors.Dispose();
        }
    }
}
