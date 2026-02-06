using System;

public class SimulationViewModel
{
    private readonly SimulationModel _model;
    private readonly CellsManager _cellsManager;

    public SimulationViewModel(CellsManager cellsManager)
    {
        _model = new SimulationModel();
        _cellsManager = cellsManager;

        // Connect to Model/Service events
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
}
