
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private InputSystem_Actions _input;

    [Header("Ajustes de Zoom")]
    public float zoomSpeed = 5f;
    public float smoothness = 10f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

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
        _targetPosition= _camera.transform.position;
        _input.UI.ScrollWheel.performed += OnScrollZoom;

    }
    private void OnScrollZoom(InputAction.CallbackContext context)
    {
        Vector2 pointerPosition = _input.UI.Point.ReadValue<Vector2>();
        Vector2 scrollValue = context.ReadValue<Vector2>();

        SetZoom(-scrollValue.y*smoothness, pointerPosition);
    }

    public void SetZoom(float delta, Vector2 pointerPosition)
    {
        _targetZoom = Mathf.Clamp(_targetZoom+delta, minZoom, maxZoom);
        _targetPosition = _camera.ScreenToWorldPoint(pointerPosition);
        _targetPosition.z = -10;

        if (_lerpCoroutine is null)
        {
            _lerpCoroutine = StartCoroutine(nameof(SmoothZoom));
        }
    }

    private IEnumerator SmoothZoom()
    {
        while (Mathf.Abs(_camera.orthographicSize - _targetZoom) > 0.01f)
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
