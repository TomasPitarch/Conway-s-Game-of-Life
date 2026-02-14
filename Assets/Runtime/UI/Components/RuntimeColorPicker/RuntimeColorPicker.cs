using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace GameOfLife.UI
{
    [UxmlElement]
    public partial class RuntimeColorPicker : VisualElement
    {
        #pragma warning disable CS0618
        public new class UxmlFactory : UxmlFactory<RuntimeColorPicker, UxmlTraits> { }
        #pragma warning restore CS0618
        public event Action<Color> OnColorChanged;

        private VisualElement _svBox;
        private VisualElement _svCursor;
        private VisualElement _hueBar;
        private VisualElement _hueCursor;
        private VisualElement _preview;

        private Texture2D _hueTexture;
        private Texture2D _svTexture;

        private float _currentHue = 0f;
        private float _currentSaturation = 1f;
        private float _currentValue = 1f;
        
        private bool _isDraggingHue = false;
        private bool _isDraggingSV = false;

        public Color value
        {
            get => Color.HSVToRGB(_currentHue, _currentSaturation, _currentValue);
            set
            {
                Color.RGBToHSV(value, out float h, out float s, out float v);
                _currentHue = h;
                _currentSaturation = s;
                _currentValue = v;
                UpdateUI();
            }
        }

        public RuntimeColorPicker()
        {
            AddToClassList("runtime-color-picker");

            _svBox = new VisualElement { name = "sv-box" };
            _svCursor = new VisualElement { name = "sv-cursor" };
            _svBox.Add(_svCursor);
            Add(_svBox);

            var controlsContainer = new VisualElement { name = "controls-container" };
            
            _hueBar = new VisualElement { name = "hue-bar" };
            _hueCursor = new VisualElement { name = "hue-cursor" };
            _hueBar.Add(_hueCursor);
            controlsContainer.Add(_hueBar);

            _preview = new VisualElement { name = "color-preview" };
            controlsContainer.Add(_preview);

            Add(controlsContainer);

            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            GenerateTextures();

            _svBox.RegisterCallback<PointerDownEvent>(OnSVPointerDown);
            _svBox.RegisterCallback<PointerMoveEvent>(OnSVPointerMove);
            _svBox.RegisterCallback<PointerUpEvent>(OnSVPointerUp);
            // Capture pointer to handle dragging outside the element
            _svBox.RegisterCallback<PointerCaptureOutEvent>(evt => _isDraggingSV = false);


            _hueBar.RegisterCallback<PointerDownEvent>(OnHuePointerDown);
            _hueBar.RegisterCallback<PointerMoveEvent>(OnHuePointerMove);
            _hueBar.RegisterCallback<PointerUpEvent>(OnHuePointerUp);
            _hueBar.RegisterCallback<PointerCaptureOutEvent>(evt => _isDraggingHue = false);

            UpdateUI();
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
             // Cleanup if needed
             if (_hueTexture != null) UnityEngine.Object.DestroyImmediate(_hueTexture);
             if (_svTexture != null) UnityEngine.Object.DestroyImmediate(_svTexture);
             _hueTexture = null;
             _svTexture = null;
        }

        private void GenerateTextures()
        {
            if (_hueTexture == null)
            {
                GenerateHueTexture();
                _hueBar.style.backgroundImage = _hueTexture;
            }

            if (_svTexture == null)
            {
                _svTexture = new Texture2D(32, 32);
                _svTexture.wrapMode = TextureWrapMode.Clamp;
                _svBox.style.backgroundImage = _svTexture;
            }
        }

        private void UpdateUI()
        {
            UpdateSVTexture();
            UpdateCursors();
            _preview.style.backgroundColor = value;
        }

        private void UpdateCursors()
        {
            // Position Hue Cursor
            if (_hueBar.layout.width > 0)
            {
                float huePos = _currentHue * _hueBar.layout.width;
                _hueCursor.style.left = huePos - (_hueCursor.layout.width * 0.5f);
            }

            // Position SV Cursor
            if (_svBox.layout.width > 0 && _svBox.layout.height > 0)
            {
                float x = _currentSaturation * _svBox.layout.width;
                float y = (1f - _currentValue) * _svBox.layout.height;
                _svCursor.style.left = x - (_svCursor.layout.width * 0.5f);
                _svCursor.style.top = y - (_svCursor.layout.height * 0.5f);
            }
        }
        
        // --- Input Handling ---

        private void OnSVPointerDown(PointerDownEvent evt)
        {
            _isDraggingSV = true;
            _svBox.CapturePointer(evt.pointerId);
            UpdateSVFromPointer(evt.localPosition);
        }

        private void OnSVPointerMove(PointerMoveEvent evt)
        {
            if (_isDraggingSV) UpdateSVFromPointer(evt.localPosition);
        }

        private void OnSVPointerUp(PointerUpEvent evt)
        {
            _isDraggingSV = false;
            _svBox.ReleasePointer(evt.pointerId);
        }

        private void UpdateSVFromPointer(Vector2 localPos)
        {
            float w = _svBox.layout.width;
            float h = _svBox.layout.height;
            if (w == 0 || h == 0) return;

            _currentSaturation = Mathf.Clamp01(localPos.x / w);
            _currentValue = Mathf.Clamp01(1f - (localPos.y / h));

            UpdateUI();
            OnColorChanged?.Invoke(value);
        }

        private void OnHuePointerDown(PointerDownEvent evt)
        {
            _isDraggingHue = true;
            _hueBar.CapturePointer(evt.pointerId);
            UpdateHueFromPointer(evt.localPosition);
        }

        private void OnHuePointerMove(PointerMoveEvent evt)
        {
            if (_isDraggingHue) UpdateHueFromPointer(evt.localPosition);
        }

        private void OnHuePointerUp(PointerUpEvent evt)
        {
            _isDraggingHue = false;
            _hueBar.ReleasePointer(evt.pointerId);
        }

        private void UpdateHueFromPointer(Vector2 localPos)
        {
            float w = _hueBar.layout.width;
            if (w == 0) return;

            _currentHue = Mathf.Clamp01(localPos.x / w);
            UpdateUI();
            OnColorChanged?.Invoke(value);
        }


        // --- Texture Generation ---

        private void GenerateHueTexture()
        {
            int width = 128; // Resolution
            int height = 1;
            _hueTexture = new Texture2D(width, height);
            _hueTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] cols = new Color[width * height];
            for (int i = 0; i < width; i++)
            {
                cols[i] = Color.HSVToRGB((float)i / width, 1, 1);
            }
            _hueTexture.SetPixels(cols);
            _hueTexture.Apply();
        }

        private void UpdateSVTexture()
        {
             // Optimization: Regenerate only when hue changes significantly? 
             // For now, doing it every update for simplicity, it's small (32x32).
             int size = 32; 
             Color[] cols = new Color[size * size];
             for (int y = 0; y < size; y++)
             {
                 for (int x = 0; x < size; x++)
                 {
                     float s = (float)x / (size - 1);
                     float v = (float)y / (size - 1);
                     cols[y * size + x] = Color.HSVToRGB(_currentHue, s, v);
                 }
             }
             _svTexture.SetPixels(cols);
             _svTexture.Apply();
        }
    }
}
