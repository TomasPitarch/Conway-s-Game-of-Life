using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOfLife.Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera _camera;

        private InputSystem_Actions _input;

        [Header("Zoom Settings")]
        public float zoomSpeed = 5f;
        public float smoothness = 10f;
        public float minZoom = 2f;
        public float maxZoom = 20f;
        public float zoomThreshold = 0.1f;

        [Header("Pan Settings")]
        public float panSpeed = 1f;

        private bool _isPanning;
        private Vector3 _dragOrigin;

        private float _targetZoom;
        private Vector3 _targetPosition;

        private Coroutine _lerpCoroutine;

        private void Awake()
        {
            _input = new InputSystem_Actions();
        }

        private void Start()
        {
            _targetZoom = _camera.orthographicSize;
            _targetPosition = _camera.transform.position;
            _input.UI.ScrollWheel.performed += OnScrollZoom;

            _input.UI.RightClick.performed += _ => StartPan();
            _input.UI.MiddleClick.performed += _ => StartPan();
        }

        private void StartPan()
        {
            if (!_isPanning)
            {
                _isPanning = true;
                _dragOrigin = _camera.ScreenToWorldPoint(_input.UI.Point.ReadValue<Vector2>());
                StartCoroutine(PanRoutine());
            }
        }

        private IEnumerator PanRoutine()
        {
            while (_isPanning)
            {
                if (!_input.UI.RightClick.IsPressed() && !_input.UI.MiddleClick.IsPressed())
                {
                    _isPanning = false;
                    yield break;
                }

                Vector3 currentPos = _camera.ScreenToWorldPoint(_input.UI.Point.ReadValue<Vector2>());
                Vector3 difference = _dragOrigin - currentPos;

                difference.z = 0;

                _camera.transform.position += difference;
                _targetPosition += difference;

                yield return null;
            }
        }

        private void OnScrollZoom(InputAction.CallbackContext context)
        {
            Vector2 pointerPosition = _input.UI.Point.ReadValue<Vector2>();
            Vector2 scrollValue = context.ReadValue<Vector2>();

            SetZoom(-scrollValue.y * smoothness, pointerPosition);
        }

        public void SetZoom(float delta, Vector2 pointerPosition)
        {
            _targetZoom = Mathf.Clamp(_targetZoom + delta, minZoom, maxZoom);
            _targetPosition = _camera.ScreenToWorldPoint(pointerPosition);
            _targetPosition.z = -10;

            if (_lerpCoroutine is null)
            {
                _lerpCoroutine = StartCoroutine(nameof(SmoothZoom));
            }
        }

        private IEnumerator SmoothZoom()
        {
            while (Mathf.Abs(_camera.orthographicSize - _targetZoom) > zoomThreshold || Vector3.Distance(_camera.transform.position, _targetPosition) > zoomThreshold)
            {
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetZoom, Time.deltaTime * zoomSpeed);
                _camera.transform.position = Vector3.Lerp(_camera.transform.position, _targetPosition, Time.deltaTime * zoomSpeed);
                yield return null;
            }
            _lerpCoroutine = null;
        }

        private void OnEnable()
        {
            _input.Enable();
        }

        private void OnDisable()
        {
            _input.Disable();
        }
    }
}
