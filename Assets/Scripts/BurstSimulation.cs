using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class BurstSimulation : ISimulationStrategy
{
    public void ExecuteStep(ref NativeArray<bool> currentGen, ref NativeArray<bool> nextGen, ref NativeArray<Color32> colors, int width, int height, Color32 liveCol, Color32 deadCol)
    {
        UpdateCellsJob job = new UpdateCellsJob {
            currentGen = currentGen,
            nextGen = nextGen,
            colors = colors,
            width = width,
            height = height,
            liveCol = liveCol,
            deadCol = deadCol
        };

        JobHandle handle = job.Schedule(currentGen.Length, 64);
        handle.Complete();
    }

    [BurstCompile]
    struct UpdateCellsJob : IJobParallelFor
    {
        [ReadOnly]public NativeArray<bool> currentGen;
        public NativeArray<bool> nextGen;
        public NativeArray<Color32> colors;
        public int width, height;
        public Color32 liveCol, deadCol;

        public void Execute(int index) {

            int x = index % width;
            int y = index / width;
            int neighbors = 0;
            for (int offsetY = -1; offsetY <= 1; offsetY++) {
                for (int offsetX = -1; offsetX <= 1; offsetX++) {
                    if (offsetX == 0 && offsetY == 0) continue;
                    int nx = (x + offsetX + width) % width;
                    int ny = (y + offsetY + height) % height;
                    if (currentGen[ny * width + nx]) neighbors++;
                }
            }
            bool willBeAlive = currentGen[index] ? (neighbors == 2 || neighbors == 3) : (neighbors == 3);
            nextGen[index] = willBeAlive;
            colors[index] = willBeAlive ? liveCol : deadCol;
        }
    }

    public void Cleanup() { }
}