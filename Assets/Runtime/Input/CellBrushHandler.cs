using UnityEngine;
using UnityEngine.InputSystem;
using GameOfLife.Simulation;

namespace GameOfLife.Input
{
    public class CellBrushHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private CellsManager cellsManager;

        private InputSystem_Actions _input;
        private bool _isDrawing;

        private void Awake()
        {
            _input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _input.Enable();
            _input.UI.Click.performed += _ => StartDrawing();
            _input.UI.Click.canceled += _ => StopDrawing();
        }

        private void OnDisable()
        {
            _input.Disable();
        }

        private void StartDrawing()
        {
            _isDrawing = true;
            StartCoroutine(DrawingRoutine());
        }

        private void StopDrawing()
        {
            _isDrawing = false;
        }

        private System.Collections.IEnumerator DrawingRoutine()
        {
            while (_isDrawing)
            {
                ApplyBrush();
                yield return null;
            }
        }

        private void ApplyBrush()
        {
            if (cellsManager == null || mainCamera == null) return;

            // If the mouse is over the UI, don't draw
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) 
                return;

            Vector2 screenPos = _input.UI.Point.ReadValue<Vector2>();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z));
            
            // Minimal knowledge: Just "Paint at this World Position"
            cellsManager.PaintAtWorldPosition(worldPos);
        }
    }
}
