using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CellsManager : MonoBehaviour
{
    public event Action<int> OnAliveCellsChanged;
    
    [Header("Settings")]
    public SimulationType currentType; // Cambia esto en el Inspector en tiempo real
    
    private Dictionary<SimulationType,ISimulationStrategy> simulationStrategies;
    
    private ISimulationStrategy _currentSimulationStrategy;
   

    
    public int width = 512;
    public int height = 512;
    [SerializeField] private Color32 liveColor = Color.white;
    [SerializeField] private Color32 deadColor = Color.black;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [HideInInspector]
    public int seedCount = 50000;
    [SerializeField] private float _speed = 60f;

    private System.Random random=new System.Random();
    private Texture2D _texture;
    private NativeArray<bool> _currentGeneration;
    private NativeArray<bool> _nextGeneration;
    private bool _isOnPause = false;
    private Coroutine _simulationCoroutine;

    void Awake()
    {
        _currentGeneration = new NativeArray<bool>(width * height, Allocator.Persistent);
        _nextGeneration = new NativeArray<bool>(width * height, Allocator.Persistent);

        simulationStrategies = new Dictionary<SimulationType, ISimulationStrategy>();
        simulationStrategies.Add(SimulationType.Classic,new ClassicSimulation());
        simulationStrategies.Add(SimulationType.Burst, new BurstSimulation());
        
        
        _texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;

        
        _spriteRenderer.sprite = Sprite.Create(
            _texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f), 
            100f 
        );
    }

    private void GenerateCells()
    {
        
        for (int i = 0; i < _currentGeneration.Length; i++) _currentGeneration[i] = false;

        
        for (int i = 0; i < seedCount; i++)
        {
            int x = random.Next(0, width);
            int y = random.Next(0, height);
            _currentGeneration[y * width + x] = true;
        }
    }

    private void SimulationStep()
    {
       
        _currentSimulationStrategy = simulationStrategies[currentType];

        if (_currentSimulationStrategy == null) return;

        
        NativeArray<Color32> textureData = _texture.GetRawTextureData<Color32>();

         _currentSimulationStrategy.ExecuteStep(
            ref _currentGeneration, 
            ref _nextGeneration, 
            ref textureData, 
            width, height, 
            liveColor, deadColor
        );

        
        _currentGeneration.CopyFrom(_nextGeneration);

        
        _texture.Apply();

        
        //OnAliveCellsChanged?.Invoke(aliveCount);
    }

    private IEnumerator SimulationLoop()
    {
        while (true) 
        {
            if (!_isOnPause) 
            {
                SimulationStep();
            }
            
            yield return new WaitForSeconds(1f / _speed);
        }
    }

    
    public void StartSimulation()
    {
        GenerateCells();
        _isOnPause = false;
        if (_simulationCoroutine == null)
            _simulationCoroutine = StartCoroutine(SimulationLoop());
    }

    public void PauseSimulation() => _isOnPause = !_isOnPause;

    public void SetSpeed(float newSpeed) => _speed = newSpeed;

    private void OnDestroy()
    {
        if (_currentGeneration.IsCreated) _currentGeneration.Dispose();
        if (_nextGeneration.IsCreated) _nextGeneration.Dispose();
    }
}
public enum SimulationType { Classic, Burst }