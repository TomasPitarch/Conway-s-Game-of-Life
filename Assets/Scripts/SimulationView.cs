using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SimulationView : MonoBehaviour
{
    [SerializeField] private CellsManager cellsManager;

    private SimulationViewModel _viewModel;
    private VisualElement _root;

    private Label _labelSpeedDisplay;
    private Label _labelCellCount;
    private Label _labelSimulationMethod;
    private DropdownField _dropdownSimulationMethod;
    private IntegerField _fieldTargetCells;
    private Slider _sliderSpeed;
    private Button _btnStart, _btnPause, _btnReset;

    private void Awake()
    {
        _viewModel = new SimulationViewModel(cellsManager);
        _root = GetComponent<UIDocument>().rootVisualElement;

        InitializeUIElements();
    }

    private void InitializeUIElements()
    {
        _labelSpeedDisplay = _root.Q<Label>("LabelCurrentSpeed");
        _labelCellCount = _root.Q<Label>("LabelAliveCount");
        _labelSimulationMethod = _root.Q<Label>("LabelSimulationMethod");
        _dropdownSimulationMethod = _root.Q<DropdownField>("DropdownSimulationMethod");
        _dropdownSimulationMethod.choices = new System.Collections.Generic.List<string> { "Classic", "Burst", "GPU" };

        _fieldTargetCells = _root.Q<IntegerField>("FieldTargetCells");
        _sliderSpeed = _root.Q<Slider>("SliderSpeed");

        _btnStart = _root.Q<Button>("BtnStart");
        _btnPause = _root.Q<Button>("BtnPause");
        _btnReset = _root.Q<Button>("BtnReset");
    }

    private void OnEnable()
    {
        BindViewModel();
        BindUIElements();

        // Set initial state
        _viewModel.SetSpeed(_sliderSpeed.value);
        _viewModel.SetTargetCells(_fieldTargetCells.value);
        HandleSimulationMethodChanged(_viewModel.CurrentSimulationMethod);
    }

    private void OnDisable()
    {
        UnbindViewModel();
        UnbindUIElements();

        // Perform ViewModel cleanup to avoid memory leaks
        _viewModel.Cleanup();
    }

    private void BindViewModel()
    {
        _viewModel.OnSpeedChanged += HandleSpeedChanged;
        _viewModel.OnSimulationMethodChanged += HandleSimulationMethodChanged;
        _viewModel.OnAliveCellsChanged += HandleAliveCellsChanged;
        _viewModel.OnTargetCellsChanged += HandleTargetCellsChanged;
        _viewModel.OnPauseStatusChanged += HandlePauseStatusChanged;
    }

    private void UnbindViewModel()
    {
        _viewModel.OnSpeedChanged -= HandleSpeedChanged;
        _viewModel.OnSimulationMethodChanged -= HandleSimulationMethodChanged;
        _viewModel.OnAliveCellsChanged -= HandleAliveCellsChanged;
        _viewModel.OnTargetCellsChanged -= HandleTargetCellsChanged;
        _viewModel.OnPauseStatusChanged -= HandlePauseStatusChanged;
    }

    private void BindUIElements()
    {
        _btnStart.clicked += _viewModel.StartSimulation;
        _btnPause.clicked += _viewModel.TogglePause;

        _dropdownSimulationMethod.RegisterValueChangedCallback(HandleSimulationMethodDropdownChanged);
        _sliderSpeed.RegisterValueChangedCallback(HandleSpeedSliderChanged);
        _fieldTargetCells.RegisterValueChangedCallback(HandleTargetCellsFieldChanged);
    }

    private void UnbindUIElements()
    {
        _btnStart.clicked -= _viewModel.StartSimulation;
        _btnPause.clicked -= _viewModel.TogglePause;

        _dropdownSimulationMethod.UnregisterValueChangedCallback(HandleSimulationMethodDropdownChanged);
        _sliderSpeed.UnregisterValueChangedCallback(HandleSpeedSliderChanged);
        _fieldTargetCells.UnregisterValueChangedCallback(HandleTargetCellsFieldChanged);
    }

    // ViewModel Event Handlers
    private void HandleSpeedChanged(string formattedSpeed) => _labelSpeedDisplay.text = formattedSpeed;
    private void HandleSimulationMethodChanged(string method)
    {
        _labelSimulationMethod.text = method;
        if (_dropdownSimulationMethod != null)
            _dropdownSimulationMethod.value = method;
    }
    private void HandleAliveCellsChanged(int count) => _labelCellCount.text = count.ToString();
    private void HandleTargetCellsChanged(int target) => _fieldTargetCells.value = target;
    private void HandlePauseStatusChanged(bool isPaused) => _btnPause.text = isPaused ? "Resume" : "Pause";

    // UI Event Handlers
    private void HandleSimulationMethodDropdownChanged(ChangeEvent<string> evt)
    {
        int index = _dropdownSimulationMethod.choices.IndexOf(evt.newValue);
        _viewModel.SetSimulationMethod(index);
    }
    private void HandleSpeedSliderChanged(ChangeEvent<float> evt) => _viewModel.SetSpeed(evt.newValue);
    private void HandleTargetCellsFieldChanged(ChangeEvent<int> evt) => _viewModel.SetTargetCells(evt.newValue);
}
