namespace GameOfLife.UI
{
    public class SimulationModel
    {
        public float CurrentSpeed { get; set; }
        public int AliveCellsCount { get; set; }
        public int TargetCells { get; set; }
        public bool IsPaused { get; set; }
    }
}
