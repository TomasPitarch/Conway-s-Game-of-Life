using System;

public class SimulationViewModel
{
    private SimulationModel _model = new SimulationModel();

   
    public event Action<string> OnSpeedChanged;
    public event Action<int> OnAliveCellsChanged;

    public void SetSpeed(float value)
    {
        _model.CurrentSpeed = value;
        
        OnSpeedChanged?.Invoke($"{value:F0} CPS");
    }

    public void UpdateAliveCells(int count)
    {
        _model.AliveCellsCount = count;
        
        OnAliveCellsChanged?.Invoke(count);
    }

    public void SetTargetCells(float target)
    {
        _model.TargetCells = (int)target;
    }
    
    public int GetTargetCells() => _model.TargetCells;
}