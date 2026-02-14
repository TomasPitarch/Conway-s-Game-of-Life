using UnityEngine;
using UnityEngine.UIElements;
using GameOfLife.Simulation;
using System.Collections.Generic;
using System;

namespace GameOfLife.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SimulationView : MonoBehaviour
    {
        [SerializeField] private CellsManager cellsManager;

        private SimulationViewModel _viewModel;
        private VisualElement _root;

        private Label _labelSpeedDisplay;
        private Label _labelCellCount;
        private Label _labelFPS;
        private IntegerField _fieldTargetCells;
        private IntegerField _fieldWidth;
        private IntegerField _fieldHeight;

        private RuntimeColorPicker _pickerLive;
        private RuntimeColorPicker _pickerDead;

        private DropdownField _dropdownStrategy;
        private Slider _sliderSpeed;
        private Button _btnStart, _btnPause, _btnApplyDimensions, _btnQuit;

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
            _labelFPS = _root.Q<Label>("LabelFPS");

            _fieldTargetCells = _root.Q<IntegerField>("FieldTargetCells");
            _fieldWidth = _root.Q<IntegerField>("FieldWidth");
            _fieldHeight = _root.Q<IntegerField>("FieldHeight");

            _pickerLive = _root.Q<RuntimeColorPicker>("PickerLive");
            _pickerDead = _root.Q<RuntimeColorPicker>("PickerDead");

            _dropdownStrategy = _root.Q<DropdownField>("DropdownStrategy");
            _dropdownStrategy.choices = new List<string> { SimulationType.Classic.ToString(), SimulationType.Burst.ToString() };

            _sliderSpeed = _root.Q<Slider>("SliderSpeed");

            _btnStart = _root.Q<Button>("BtnStart");
            _btnPause = _root.Q<Button>("BtnPause");
            _btnApplyDimensions = _root.Q<Button>("BtnApplyDimensions");
            _btnQuit = _root.Q<Button>("BtnQuit");
        }

        private void Update()
        {
            float fps = 1.0f / Time.unscaledDeltaTime;
            _viewModel.UpdateFPS(fps);
        }

        private void OnEnable()
        {
            _viewModel.Initialize();
            BindViewModel();
            BindUIElements();

            // Set initial state from cellsManager
            _viewModel.SetSpeed(cellsManager.speed);
            _viewModel.SetTargetCells(cellsManager.seedCount);
            _viewModel.SetWidth(cellsManager.width);
            _viewModel.SetHeight(cellsManager.height);
            _viewModel.SetSimulationType(cellsManager.currentType);

            _fieldWidth.value = cellsManager.width;
            _fieldHeight.value = cellsManager.height;
            _dropdownStrategy.value = cellsManager.currentType.ToString();

            _pickerLive.value = cellsManager.liveColor;
            _pickerDead.value = cellsManager.deadColor;
        }

        private void OnDisable()
        {
            UnbindViewModel();
            UnbindUIElements();
            _viewModel.Cleanup();
        }

        private void BindViewModel()
        {
            _viewModel.OnSpeedChanged += HandleSpeedChanged;
            _viewModel.OnAliveCellsChanged += HandleAliveCellsChanged;
            _viewModel.OnTargetCellsChanged += HandleTargetCellsChanged;
            _viewModel.OnPauseStatusChanged += HandlePauseStatusChanged;
            _viewModel.OnFPSChanged += HandleFPSChanged;
        }

        private void UnbindViewModel()
        {
            _viewModel.OnSpeedChanged -= HandleSpeedChanged;
            _viewModel.OnAliveCellsChanged -= HandleAliveCellsChanged;
            _viewModel.OnTargetCellsChanged -= HandleTargetCellsChanged;
            _viewModel.OnPauseStatusChanged -= HandlePauseStatusChanged;
            _viewModel.OnFPSChanged -= HandleFPSChanged;
        }

        private void BindUIElements()
        {
            _btnStart.clicked += _viewModel.StartSimulation;
            _btnPause.clicked += _viewModel.TogglePause;
            _btnApplyDimensions.clicked += _viewModel.ApplyDimensions;
            _btnQuit.clicked += _viewModel.QuitApplication;

            _sliderSpeed.RegisterValueChangedCallback(HandleSpeedSliderChanged);
            _fieldTargetCells.RegisterValueChangedCallback(HandleTargetCellsFieldChanged);
            _fieldWidth.RegisterValueChangedCallback(HandleWidthChanged);
            _fieldHeight.RegisterValueChangedCallback(HandleHeightChanged);

            _pickerLive.OnColorChanged += HandleLiveColorChanged;
            _pickerDead.OnColorChanged += HandleDeadColorChanged;

            _dropdownStrategy.RegisterValueChangedCallback(HandleStrategyChanged);
        }

        private void UnbindUIElements()
        {
            _btnStart.clicked -= _viewModel.StartSimulation;
            _btnPause.clicked -= _viewModel.TogglePause;
            _btnApplyDimensions.clicked -= _viewModel.ApplyDimensions;
            _btnQuit.clicked -= _viewModel.QuitApplication;

            _sliderSpeed.UnregisterValueChangedCallback(HandleSpeedSliderChanged);
            _fieldTargetCells.UnregisterValueChangedCallback(HandleTargetCellsFieldChanged);
            _fieldWidth.UnregisterValueChangedCallback(HandleWidthChanged);
            _fieldHeight.UnregisterValueChangedCallback(HandleHeightChanged);

            _pickerLive.OnColorChanged -= HandleLiveColorChanged;
            _pickerDead.OnColorChanged -= HandleDeadColorChanged;

            _dropdownStrategy.UnregisterValueChangedCallback(HandleStrategyChanged);
        }

        // ViewModel Event Handlers
        private void HandleSpeedChanged(string formattedSpeed) => _labelSpeedDisplay.text = formattedSpeed;
        private void HandleAliveCellsChanged(int count) => _labelCellCount.text = count.ToString();
        private void HandleTargetCellsChanged(int target) => _fieldTargetCells.value = target;
        private void HandlePauseStatusChanged(bool isPaused) => _btnPause.text = isPaused ? "Resume" : "Pause";
        private void HandleFPSChanged(float fps) => _labelFPS.text = fps.ToString("F0");

        // UI Event Handlers
        private void HandleSpeedSliderChanged(ChangeEvent<float> evt) => _viewModel.SetSpeed(evt.newValue);
        private void HandleTargetCellsFieldChanged(ChangeEvent<int> evt) => _viewModel.SetTargetCells(evt.newValue);
        private void HandleWidthChanged(ChangeEvent<int> evt) => _viewModel.SetWidth(evt.newValue);
        private void HandleHeightChanged(ChangeEvent<int> evt) => _viewModel.SetHeight(evt.newValue);

        private void HandleLiveColorChanged(Color color)
        {
            _viewModel.SetLiveColor(color);
        }

        private void HandleDeadColorChanged(Color color)
        {
            _viewModel.SetDeadColor(color);
        }

        private void HandleStrategyChanged(ChangeEvent<string> evt)
        {
            if (Enum.TryParse(evt.newValue, out SimulationType type))
                _viewModel.SetSimulationType(type);
        }
    }
}
