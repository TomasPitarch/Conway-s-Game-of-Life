using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SimulationView : MonoBehaviour
{
    [Header("Referencias a la Lógica")]
    public CellsManager cellsManager;
    public CameraController cameraController;

    private SimulationViewModel _viewModel;
    private VisualElement _root;

    // Elementos de la UI
    private Label _labelSpeedDisplay;
    private Label _labelCellCount;
    private IntegerField _fieldTargetCells;
    private Slider _sliderSpeed;
    private Button _btnStart, _btnPause, _btnReset;

    private void Awake()
    {
        _viewModel = new SimulationViewModel();
        _root = GetComponent<UIDocument>().rootVisualElement;
    }

    void OnEnable()
    {
        _labelSpeedDisplay = _root.Q<Label>("LabelCurrentSpeed");
        _labelCellCount = _root.Q<Label>("LabelAliveCount");
        _fieldTargetCells = _root.Q<IntegerField>("FieldTargetCells");
        _sliderSpeed = _root.Q<Slider>("SliderSpeed");
        
        _btnStart = _root.Q<Button>("BtnStart");
        _btnPause = _root.Q<Button>("BtnPause");
        _btnReset = _root.Q<Button>("BtnReset");

       
        _viewModel.OnSpeedChanged += (formattedSpeed) => _labelSpeedDisplay.text = formattedSpeed;
        _viewModel.OnAliveCellsChanged += (countText) => _labelCellCount.text = countText;

        
        _btnStart.clicked += HandleStart;
        _btnPause.clicked += () => Debug.Log("Simulación Pausada"); // Aquí llamarías a cellsManager.Pause()
        _btnReset.clicked += () => UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Reinicio simple

        _sliderSpeed.RegisterValueChangedCallback(evt => {
            _viewModel.SetSpeed(evt.newValue);
            cellsManager.SetSpeed(evt.newValue);
        });
        
        _viewModel.SetSpeed(_sliderSpeed.value);
    }

    private void HandleStart()
    { // int target = _fieldTargetCells.value;
        // _viewModel.SetTargetCells(target);
        cellsManager.StartSimulation();
    }
    
    public void UpdateCellCountDisplay(int currentCount)
    {
        _viewModel.UpdateAliveCells(currentCount);
    }

    void OnDisable()
    {
        
        _btnStart.clicked -= HandleStart;
        
    }
}