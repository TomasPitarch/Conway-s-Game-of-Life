using Unity.Collections;
using UnityEngine;

public class ClassicSimulation : ISimulationStrategy
{
    public void ExecuteStep(ref NativeArray<bool> currentGen, ref NativeArray<bool> nextGen, ref NativeArray<Color32> colors, int width, int height, Color32 liveCol, Color32 deadCol)
    {
        for (int i = 0; i < currentGen.Length; i++)
        {
            int x = i % width;
            int y = i / width;
            int neighbors = 0;

            for (int offsetY = -1; offsetY <= 1; offsetY++) {
                for (int offsetX = -1; offsetX <= 1; offsetX++) {
                    if (offsetX == 0 && offsetY == 0) continue;
                    int nx = (x + offsetX + width) % width;
                    int ny = (y + offsetY + height) % height;
                    if (currentGen[ny * width + nx]) neighbors++;
                }
            }

            bool willBeAlive = currentGen[i] ? (neighbors == 2 || neighbors == 3) : (neighbors == 3);
            nextGen[i] = willBeAlive;
            colors[i] = willBeAlive ? liveCol : deadCol;
        }
    }

    public void Cleanup() { }
}