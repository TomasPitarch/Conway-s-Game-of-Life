using System;

public class SimulationViewModel
{
    private SimulationModel _model = new SimulationModel();

   
    public event Action<string> OnSpeedChanged;
    public event Action<string> OnAliveCellsChanged;

    public void SetSpeed(float value)
    {
        _model.CurrentSpeed = value;
        
        OnSpeedChanged?.Invoke($"{value:F0} FPS");
    }

    public void UpdateAliveCells(int count)
    {
        _model.AliveCellsCount = count;
        
        OnAliveCellsChanged?.Invoke(count.ToString());
    }

    public void SetTargetCells(int target)
    {
        _model.TargetCells = target;
    }
    
    public int GetTargetCells() => _model.TargetCells;
}