using UnityEngine;

namespace GameOfLife.Simulation
{
    public class SimulationRenderer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        private Texture2D _texture;

        public void Initialize(int width, int height)
        {
            if (_texture != null) Destroy(_texture);
            _texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            _texture.filterMode = FilterMode.Point;
            _texture.wrapMode = TextureWrapMode.Clamp;

            if (spriteRenderer.sprite != null) Destroy(spriteRenderer.sprite);
            spriteRenderer.sprite = Sprite.Create(
                _texture,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        public void ApplyTexture()
        {
            if (_texture != null)
            {
                _texture.Apply();
            }
        }

        public Unity.Collections.NativeArray<Color32> GetRawTextureData()
        {
            return _texture.GetRawTextureData<Color32>();
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            Vector3 localPos = spriteRenderer.transform.InverseTransformPoint(worldPos);
            int x = Mathf.RoundToInt(localPos.x * 100f + _texture.width * 0.5f);
            int y = Mathf.RoundToInt(localPos.y * 100f + _texture.height * 0.5f);
            return new Vector2Int(x, y);
        }
    }
}
