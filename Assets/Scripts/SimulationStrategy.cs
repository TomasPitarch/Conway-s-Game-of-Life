using Unity.Collections;
using UnityEngine;

public interface ISimulationStrategy
{
    public abstract void ExecuteStep(
        ref NativeArray<bool> currentGen,
        ref NativeArray<bool> nextGen,
        ref NativeArray<Color32> colors,
        int width, int height,
        Color32 liveCol, Color32 deadCol);

    public void Cleanup();
}