using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class GPUSimulation : ISimulationStrategy
{
    private ComputeShader _computeShader;
    private ComputeBuffer _currentGenBuffer;
    private ComputeBuffer _nextGenBuffer;
    private ComputeBuffer _colorsBuffer;

    private NativeArray<int> _inputData;
    private NativeArray<int> _outputData;
    private NativeArray<uint> _colorData;

    private int _width, _height;

    public GPUSimulation(ComputeShader shader)
    {
        _computeShader = shader;
    }

    public void ExecuteStep(ref NativeArray<bool> currentGen, ref NativeArray<bool> nextGen, ref NativeArray<Color32> colors, int width, int height, Color32 liveCol, Color32 deadCol)
    {
        if (_computeShader == null) return;

        int count = width * height;
        if (_currentGenBuffer == null || _currentGenBuffer.count != count)
        {
            ReleaseBuffers();
            _currentGenBuffer = new ComputeBuffer(count, sizeof(int));
            _nextGenBuffer = new ComputeBuffer(count, sizeof(int));
            _colorsBuffer = new ComputeBuffer(count, sizeof(uint));

            _inputData = new NativeArray<int>(count, Allocator.Persistent);
            _outputData = new NativeArray<int>(count, Allocator.Persistent);
            _colorData = new NativeArray<uint>(count, Allocator.Persistent);

            _width = width;
            _height = height;
        }

        // Parallel conversion using Burst
        var toIntJob = new BoolToIntJob { input = currentGen, output = _inputData };
        toIntJob.Schedule(count, 64).Complete();

        _currentGenBuffer.SetData(_inputData);

        int kernel = _computeShader.FindKernel("CSMain");
        _computeShader.SetBuffer(kernel, "currentGen", _currentGenBuffer);
        _computeShader.SetBuffer(kernel, "nextGen", _nextGenBuffer);
        _computeShader.SetBuffer(kernel, "colors", _colorsBuffer);
        _computeShader.SetInt("width", width);
        _computeShader.SetInt("height", height);

        uint liveColUint = (uint)(liveCol.r | (liveCol.g << 8) | (liveCol.b << 16) | (liveCol.a << 24));
        uint deadColUint = (uint)(deadCol.r | (deadCol.g << 8) | (deadCol.b << 16) | (deadCol.a << 24));

        _computeShader.SetInt("liveColor", (int)liveColUint);
        _computeShader.SetInt("deadColor", (int)deadColUint);

        _computeShader.Dispatch(kernel, Mathf.CeilToInt(width / 8f), Mathf.CeilToInt(height / 8f), 1);

        _nextGenBuffer.GetData(_outputData);
        _colorsBuffer.GetData(_colorData);

        // Parallel conversion back using Burst
        var fromIntJob = new IntToBoolAndColorJob {
            nextGenInt = _outputData,
            colorsUint = _colorData,
            nextGenBool = nextGen,
            colorsRes = colors
        };
        fromIntJob.Schedule(count, 64).Complete();
    }

    public void Cleanup()
    {
        ReleaseBuffers();
    }

    private void ReleaseBuffers()
    {
        if (_currentGenBuffer != null) { _currentGenBuffer.Release(); _currentGenBuffer = null; }
        if (_nextGenBuffer != null) { _nextGenBuffer.Release(); _nextGenBuffer = null; }
        if (_colorsBuffer != null) { _colorsBuffer.Release(); _colorsBuffer = null; }

        if (_inputData.IsCreated) _inputData.Dispose();
        if (_outputData.IsCreated) _outputData.Dispose();
        if (_colorData.IsCreated) _colorData.Dispose();
    }

    [BurstCompile]
    struct BoolToIntJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<bool> input;
        public NativeArray<int> output;
        public void Execute(int i) => output[i] = input[i] ? 1 : 0;
    }

    [BurstCompile]
    struct IntToBoolAndColorJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> nextGenInt;
        [ReadOnly] public NativeArray<uint> colorsUint;
        public NativeArray<bool> nextGenBool;
        public NativeArray<Color32> colorsRes;

        public void Execute(int i)
        {
            nextGenBool[i] = nextGenInt[i] > 0;
            uint c = colorsUint[i];
            colorsRes[i] = new Color32((byte)(c & 0xFF), (byte)((c >> 8) & 0xFF), (byte)((c >> 16) & 0xFF), (byte)((c >> 24) & 0xFF));
        }
    }
}
