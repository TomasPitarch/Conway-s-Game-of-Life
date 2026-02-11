using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using GameOfLife.Simulation.Strategies;

namespace GameOfLife.Simulation
{
    public class CellsManager : MonoBehaviour
    {
        public event Action<int> OnAliveCellsChanged;

        [Header("Settings")]
        public SimulationType currentType;

        [SerializeField] private SimulationRenderer simulationRenderer;

        private Dictionary<SimulationType, ISimulationStrategy> _simulationStrategies;
        private ISimulationStrategy _currentSimulationStrategy;

        public int width = 512;
        public int height = 512;
        public Color32 liveColor = Color.white;
        public Color32 deadColor = Color.black;

        [HideInInspector]
        public int seedCount = 50000;
        [SerializeField] public float speed = 60f;

        private System.Random _random = new System.Random();
        private NativeArray<bool> _currentGeneration;
        private NativeArray<bool> _nextGeneration;
        private bool _isOnPause = false;
        private Coroutine _simulationCoroutine;

        private void Awake()
        {
            _currentGeneration = new NativeArray<bool>(width * height, Allocator.Persistent);
            _nextGeneration = new NativeArray<bool>(width * height, Allocator.Persistent);

            _simulationStrategies = SimulationStrategyFactory.CreateStrategies();

            if (simulationRenderer != null)
            {
                simulationRenderer.Initialize(width, height);
            }
        }

        private void GenerateCells()
        {
            for (int i = 0; i < _currentGeneration.Length; i++) _currentGeneration[i] = false;

            for (int i = 0; i < seedCount; i++)
            {
                int x = _random.Next(0, width);
                int y = _random.Next(0, height);
                _currentGeneration[y * width + x] = true;
            }
        }

        private void SimulationStep()
        {
            if (!_simulationStrategies.TryGetValue(currentType, out _currentSimulationStrategy)) return;
            if (simulationRenderer == null) return;

            NativeArray<Color32> textureData = simulationRenderer.GetRawTextureData();

             _currentSimulationStrategy.ExecuteStep(
                ref _currentGeneration,
                ref _nextGeneration,
                ref textureData,
                width, height,
                liveColor, deadColor
            );

            _currentGeneration.CopyFrom(_nextGeneration);
            simulationRenderer.ApplyTexture();

            int aliveCount = CountAliveCells();
            OnAliveCellsChanged?.Invoke(aliveCount);
        }

        private int CountAliveCells()
        {
            // Optimized counting using Burst-compiled Job
            NativeReference<int> countRef = new NativeReference<int>(Allocator.TempJob);
            CountAliveJob job = new CountAliveJob
            {
                cells = _currentGeneration,
                count = countRef
            };
            job.Schedule().Complete();
            int count = countRef.Value;
            countRef.Dispose();
            return count;
        }

        [BurstCompile]
        struct CountAliveJob : IJob
        {
            [ReadOnly] public NativeArray<bool> cells;
            public NativeReference<int> count;

            public void Execute()
            {
                int c = 0;
                for (int i = 0; i < cells.Length; i++)
                {
                    if (cells[i]) c++;
                }
                count.Value = c;
            }
        }

        private IEnumerator SimulationLoop()
        {
            while (true)
            {
                if (!_isOnPause)
                {
                    SimulationStep();
                }

                yield return new WaitForSeconds(1f / speed);
            }
        }

        public void StartSimulation()
        {
            GenerateCells();
            _isOnPause = false;
            if (_simulationCoroutine == null)
                _simulationCoroutine = StartCoroutine(SimulationLoop());
        }

        public void ResetSimulation()
        {
            if (_simulationCoroutine != null)
            {
                StopCoroutine(_simulationCoroutine);
                _simulationCoroutine = null;
            }
            GenerateCells();
            SimulationStep(); // Update texture immediately
        }

        public void PauseSimulation() => _isOnPause = !_isOnPause;

        public void SetSpeed(float newSpeed) => speed = newSpeed;

        public void SetDimensions(int newWidth, int newHeight)
        {
            width = Mathf.Max(1, newWidth);
            height = Mathf.Max(1, newHeight);
            Reinitialize();
        }

        public void SetLiveColor(Color32 color) => liveColor = color;
        public void SetDeadColor(Color32 color) => deadColor = color;
        public void SetSimulationType(SimulationType type) => currentType = type;

        public void Reinitialize()
        {
            if (_simulationCoroutine != null)
            {
                StopCoroutine(_simulationCoroutine);
                _simulationCoroutine = null;
            }

            if (_currentGeneration.IsCreated) _currentGeneration.Dispose();
            if (_nextGeneration.IsCreated) _nextGeneration.Dispose();

            _currentGeneration = new NativeArray<bool>(width * height, Allocator.Persistent);
            _nextGeneration = new NativeArray<bool>(width * height, Allocator.Persistent);

            if (simulationRenderer != null)
            {
                simulationRenderer.Initialize(width, height);
            }

            GenerateCells();
            _isOnPause = false;
            _simulationCoroutine = StartCoroutine(SimulationLoop());
        }

        private void OnDestroy()
        {
            if (_currentGeneration.IsCreated) _currentGeneration.Dispose();
            if (_nextGeneration.IsCreated) _nextGeneration.Dispose();
        }
    }
}
