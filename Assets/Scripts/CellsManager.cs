using System;
using System.Collections;
using UnityEngine;

public class CellsManager : MonoBehaviour
{
    public event Action<int> OnAliveCellsChanged;
    
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Ajustes de la Rejilla")] public int width = 256;
    public int height = 256;


    public int seedCount = 250;

    private Texture2D _texture;
    private bool[] _currentGeneration;
    private bool[] _nextGeneration;
    private float _speed = 60f;

    System.Random _random = new System.Random();
    private Coroutine _simulationCoroutine;

    void Awake()
    {
        _random = new System.Random();
        _currentGeneration = new bool[width * height];
        _nextGeneration = new bool[width * height];
        _texture = new Texture2D(width, height);


        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;


        _spriteRenderer.sprite = Sprite.Create(
            _texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f), // Pivot en el centro
            100f // Pixels Per Unit (PPU)
        );
    }

    private void GenerateCells()
    {
        ClearCells();
        for (int i = 0; i < seedCount; i++)
        {
            int x = _random.Next(0, width);
            int y = _random.Next(0, height);

            _currentGeneration[(y * width) + x] = true;
        }
    }

    private void ClearCells()
    {
        System.Array.Clear(_currentGeneration, 0, _currentGeneration.Length);
    }
    private void PaintCells()
    {
        Color32[] colors = new Color32[_currentGeneration.Length];
        int x = 0;
        int y = 0;
        for (int i = 0; i < _currentGeneration.Length; i++)
        {
            colors[i] = _currentGeneration[i] ? Color.black : Color.white;
        }

        _texture.SetPixels32(colors);
        _texture.Apply();
    }
    private void Simulation()
    {
        int aliveCount = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
               
                int index = y * width + x;
                int neighbors = CountLiveNeighbors(x, y);

                if (_currentGeneration[index])
                {
                    _nextGeneration[index] = (neighbors == 2 || neighbors == 3);
                    if (_nextGeneration[index]) aliveCount++;
                }
                else
                {
                   
                    _nextGeneration[index] = (neighbors == 3);
                    if (_nextGeneration[index]) aliveCount++;
                }
            }
        }
        System.Array.Copy(_nextGeneration, _currentGeneration, _currentGeneration.Length);
        OnAliveCellsChanged?.Invoke(aliveCount);
    }
    public void StartSimulation()
    {
        GenerateCells();
        if (_simulationCoroutine is null)
        {
            _simulationCoroutine = StartCoroutine(nameof(SimulationLoop));
        }
    }
    private int CountLiveNeighbors(int x, int y)
    {
        int liveNeighbors = 0;

        for (int offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                if (offsetX == 0 && offsetY == 0) continue; 

                
                int nx = (x + offsetX + width) % width;
                int ny = (y + offsetY + height) % height;

                
                if (_currentGeneration[ny * width + nx])
                {
                    liveNeighbors++;
                }
            }
        }
        return liveNeighbors;
    }
    
    private IEnumerator SimulationLoop()
    {
        while (true) 
        {
            Simulation(); 
            PaintCells();
            yield return new WaitForSeconds(1/_speed);
        }
        
    }
    public void StopSimulation()
    {
        if (_simulationCoroutine is not null)
        {
            StopCoroutine(_simulationCoroutine);
            _simulationCoroutine = null;
            ClearCells();
        }
    }
    public void SetSpeed(float newSpeed)
    {
        _speed = newSpeed;
    }
}
   
    

