using System;
using UnityEngine;
using GameOfLife.Simulation;

namespace GameOfLife.UI
{
    public class SimulationViewModel
    {
        private readonly SimulationModel _model;
        private readonly CellsManager _cellsManager;

        public SimulationViewModel(CellsManager cellsManager)
        {
            _model = new SimulationModel();
            _cellsManager = cellsManager;
        }

        /// <summary>
        /// Subscribes to model/service events.
        /// </summary>
        public void Initialize()
        {
            _cellsManager.OnAliveCellsChanged += HandleAliveCellsChanged;
        }

        /// <summary>
        /// Performs cleanup to avoid memory leaks.
        /// </summary>
        public void Cleanup()
        {
            _cellsManager.OnAliveCellsChanged -= HandleAliveCellsChanged;
        }

        private void HandleAliveCellsChanged(int count)
        {
            _model.AliveCellsCount = count;
            OnAliveCellsChanged?.Invoke(count);
        }

        public event Action<string> OnSpeedChanged;
        public event Action<int> OnAliveCellsChanged;
        public event Action<int> OnTargetCellsChanged;
        public event Action<bool> OnPauseStatusChanged;
        public event Action<int> OnWidthChanged;
        public event Action<int> OnHeightChanged;
        public event Action<Color32> OnLiveColorChanged;
        public event Action<Color32> OnDeadColorChanged;
        public event Action<SimulationType> OnSimulationTypeChanged;
        public event Action<float> OnFPSChanged;

        public void SetSpeed(float value)
        {
            _model.CurrentSpeed = value;
            _cellsManager.SetSpeed(value);
            OnSpeedChanged?.Invoke($"{value:F0} CPS");
        }

        public void SetTargetCells(int target)
        {
            _model.TargetCells = target;
            _cellsManager.seedCount = target;
            OnTargetCellsChanged?.Invoke(target);
        }

        public void StartSimulation()
        {
            _cellsManager.StartSimulation();
        }

        public void TogglePause()
        {
            _cellsManager.PauseSimulation();
            _model.IsPaused = !_model.IsPaused;
            OnPauseStatusChanged?.Invoke(_model.IsPaused);
        }

        public void SetWidth(int width)
        {
            _model.Width = width;
            OnWidthChanged?.Invoke(width);
        }

        public void SetHeight(int height)
        {
            _model.Height = height;
            OnHeightChanged?.Invoke(height);
        }

        public void ApplyDimensions()
        {
            _cellsManager.SetDimensions(_model.Width, _model.Height);
        }

        public void SetLiveColor(Color32 color)
        {
            _model.LiveColor = color;
            _cellsManager.SetLiveColor(color);
            OnLiveColorChanged?.Invoke(color);
        }

        public void SetDeadColor(Color32 color)
        {
            _model.DeadColor = color;
            _cellsManager.SetDeadColor(color);
            OnDeadColorChanged?.Invoke(color);
        }

        public void SetSimulationType(SimulationType type)
        {
            _model.SimulationType = type;
            _cellsManager.SetSimulationType(type);
            OnSimulationTypeChanged?.Invoke(type);
        }

        public void UpdateFPS(float fps)
        {
            _model.FPS = fps;
            OnFPSChanged?.Invoke(fps);
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
