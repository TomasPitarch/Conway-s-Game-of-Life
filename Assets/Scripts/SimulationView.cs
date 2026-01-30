using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SimulationView : MonoBehaviour
{
    
    public CellsManager cellsManager;
    

    private SimulationViewModel _viewModel;
    private VisualElement _root;

    
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
        cellsManager.OnAliveCellsChanged += UpdateCellCountDisplay;

        _fieldTargetCells.RegisterValueChangedCallback(HandleChangeStartCells);
        
        _btnStart.clicked += HandleStart;
        _btnPause.clicked += HandlePause;

        _sliderSpeed.RegisterValueChangedCallback(HandleChangeSpeed);
        
        _viewModel.SetSpeed(_sliderSpeed.value);
        
        HandleChangeStartCells(ChangeEvent<int>.GetPooled(_fieldTargetCells.value, _fieldTargetCells.value));
    }

    private void HandlePause()
    {
        cellsManager.PauseSimulation();
    }
    private void HandleChangeStartCells(ChangeEvent<int> evt)
    {
        _viewModel.SetTargetCells(evt.newValue);
        cellsManager.seedCount = evt.newValue;
    }

    private void HandleChangeSpeed(ChangeEvent<float> evt)
    {
        _viewModel.SetSpeed(evt.newValue);
        cellsManager.SetSpeed(evt.newValue);
    }
    

    private void HandleStart()
    { 
        cellsManager.StartSimulation();
    }
    
    public void UpdateCellCountDisplay(int currentCount)
    {
        _viewModel.UpdateAliveCells(currentCount);
        _labelCellCount.text = currentCount.ToString();
    }

    void OnDisable()
    {
        _btnStart.clicked -= HandleStart;
    }
}