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
        _cellsManager.OnSimulationTypeChanged += HandleSimulationTypeChanged;
    }

    /// <summary>
    /// Performs cleanup to avoid memory leaks.
    /// </summary>
    public void Cleanup()
    {
        _cellsManager.OnAliveCellsChanged -= HandleAliveCellsChanged;
        _cellsManager.OnSimulationTypeChanged -= HandleSimulationTypeChanged;
    }

    private void HandleAliveCellsChanged(int count)
    {
        _model.AliveCellsCount = count;
        OnAliveCellsChanged?.Invoke(count);
    }

    private void HandleSimulationTypeChanged(SimulationType type)
    {
        _model.SimulationMethod = type.ToString();
        OnSimulationMethodChanged?.Invoke(_model.SimulationMethod);
    }

    public string CurrentSimulationMethod => _cellsManager.currentType.ToString();

    public event Action<string> OnSpeedChanged;
    public event Action<string> OnSimulationMethodChanged;
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

    public void SetSimulationMethod(int index)
    {
        _cellsManager.currentType = (SimulationType)index;
    }
}
