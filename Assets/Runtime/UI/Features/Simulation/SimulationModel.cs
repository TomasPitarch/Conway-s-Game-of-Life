using UnityEngine;
using GameOfLife.Simulation;

namespace GameOfLife.UI
{
    public class SimulationModel
    {
        public float CurrentSpeed { get; set; }
        public int AliveCellsCount { get; set; }
        public int TargetCells { get; set; }
        public bool IsPaused { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Color32 LiveColor { get; set; }
        public Color32 DeadColor { get; set; }
        public SimulationType SimulationType { get; set; }
        public float FPS { get; set; }
    }
}
