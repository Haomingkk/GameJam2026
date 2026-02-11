using GameJam2026.GamePlay;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Gamejam2026.UI
{

    public class MinimapFogOfWar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _minimapRect;
        [SerializeField] private RawImage _fogImage;
        [SerializeField] private Camera _minimapCamera; 

        [Header("World mapping")]
        [SerializeField] private Tilemap _tileMapGrid;
        private Vector2 _worldMin; // bottom-left of map in world coords
        private Vector2 _worldMax; // top-right of map in world coords

        [Header("Fog settings")]
        [SerializeField] private int _textureSize = 512;         
        [SerializeField] private int _revealRadiusPixels = 12;  
        [SerializeField] private int _updateEveryNFrames = 2;
        [SerializeField, Range(0f, 1f)] private float _edgeSoftness = 0.5f;

        Texture2D _fogTex;
        Color32[] _pixels;
        int _frame;

        void Start()
        {
            _fogTex = new Texture2D(_textureSize, _textureSize, TextureFormat.RGBA32, false);
            _fogTex.filterMode = FilterMode.Bilinear;
            _fogTex.wrapMode = TextureWrapMode.Clamp;

            _pixels = new Color32[_textureSize * _textureSize];
            for (int i = 0; i < _pixels.Length; i++)
                _pixels[i] = new Color32(0, 0, 0, 255); 

            _fogTex.SetPixels32(_pixels);
            _fogTex.Apply(false);

            _fogImage.texture = _fogTex;

            _CalculateBounds();

            _fogImage.uvRect = new Rect(0, 0, 1, 1);
        }

        void LateUpdate()
        {
            _frame++;
            if (_frame % _updateEveryNFrames != 0) return;

            if (_minimapCamera == null) return;
            if (PlayerController.instance == null) return;

            Vector2 p = PlayerController.instance.transform.position;

            // world -> map UV -> texture pixels
            float u = Mathf.InverseLerp(_worldMin.x, _worldMax.x, p.x);
            float v = Mathf.InverseLerp(_worldMin.y, _worldMax.y, p.y);

            int px = Mathf.RoundToInt(u * (_textureSize - 1));
            int py = Mathf.RoundToInt(v * (_textureSize - 1));

            _RevealCircle(px, py, _revealRadiusPixels);

            _fogTex.SetPixels32(_pixels);// The core of the Code
            _fogTex.Apply(false);

            // ALIGN THE GLOBAL FOG TEXTURE TO THE MOVING MINIMAP VIEW
            _UpdateFogUVRectToMatchMinimapCamera();
        }

        void _UpdateFogUVRectToMatchMinimapCamera()
        {
            // Compute minimap camera world-view rectangle
            float camHalfH = _minimapCamera.orthographicSize;
            float camHalfW = camHalfH * _minimapCamera.aspect;

            Vector2 camCenter = _minimapCamera.transform.position;

            float viewMinX = camCenter.x - camHalfW;
            float viewMaxX = camCenter.x + camHalfW;
            float viewMinY = camCenter.y - camHalfH;
            float viewMaxY = camCenter.y + camHalfH;

            // Convert camera view bounds to map UV (0..1) relative to whole map
            float uMin = Mathf.InverseLerp(_worldMin.x, _worldMax.x, viewMinX);
            float uMax = Mathf.InverseLerp(_worldMin.x, _worldMax.x, viewMaxX);
            float vMin = Mathf.InverseLerp(_worldMin.y, _worldMax.y, viewMinY);
            float vMax = Mathf.InverseLerp(_worldMin.y, _worldMax.y, viewMaxY);

            // Clamp so UVRect never goes outside texture
            uMin = Mathf.Clamp01(uMin);
            uMax = Mathf.Clamp01(uMax);
            vMin = Mathf.Clamp01(vMin);
            vMax = Mathf.Clamp01(vMax);

            float uSize = Mathf.Max(0.0001f, uMax - uMin);
            float vSize = Mathf.Max(0.0001f, vMax - vMin);

            // This makes the fog overlay show the correct portion of the global fog texture
            _fogImage.uvRect = new Rect(uMin, vMin, uSize, vSize);
        }

        void _RevealCircle(int cx, int cy, int r)
        {
            int xMin = Mathf.Max(0, cx - r);
            int xMax = Mathf.Min(_textureSize - 1, cx + r);
            int yMin = Mathf.Max(0, cy - r);
            int yMax = Mathf.Min(_textureSize - 1, cy + r);

            float innerRadius = r * (1f - _edgeSoftness);
            float outerRadius = r;

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > outerRadius)
                        continue;

                    int idx = y * _textureSize + x;

                    // Fully revealed inside inner radius
                    if (dist <= innerRadius)
                    {
                        _pixels[idx].a = 0;
                    }
                    else
                    {
                        // Soft fade between inner and outer radius
                        float t = Mathf.InverseLerp(outerRadius, innerRadius, dist);
                        byte targetAlpha = (byte)Mathf.Lerp(255, 0, t);

                        // never re-darken revealed areas
                        if (targetAlpha < _pixels[idx].a)
                            _pixels[idx].a = targetAlpha;
                    }
                }
            }
        }


        void _CalculateBounds()
        {
            if (_tileMapGrid == null) return;

            BoundsInt cellBounds = _tileMapGrid.cellBounds;

            Vector3 minWorld = _tileMapGrid.CellToWorld(cellBounds.min);
            Vector3 maxWorld = _tileMapGrid.CellToWorld(cellBounds.max);

            _worldMin = new Vector2(minWorld.x, minWorld.y);
            _worldMax = new Vector2(maxWorld.x, maxWorld.y);
        }
    }


}
