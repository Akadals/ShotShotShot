using UnityEngine;

public class GameBackgroundMotion : MonoBehaviour
{
    [SerializeField] private Sprite m_backgroundSprite;
    [SerializeField] private float m_parallaxStrength = 0.22f;
    private Transform m_backgroundRoot;
    private Transform[] m_tiles;
    private PlayerController m_player;
    private Camera m_mainCamera;
    private Vector3 m_cameraCenter;
    private Vector2 m_smoothOffset;
    private Vector2 m_offsetVelocity;
    private Vector2 m_tileSize;

    public void Initialize(PlayerController _targetPlayer, Camera _camera, Sprite _background)
    {
        m_player = _targetPlayer;
        m_mainCamera = _camera;
        m_cameraCenter = _camera.transform.position + Vector3.forward * 10f;
        if (_background == null) _background = m_backgroundSprite;
        if (_background == null) return;
        m_tileSize = _background.bounds.size;
        if (m_tileSize.x <= 0f || m_tileSize.y <= 0f) return;
        m_backgroundRoot = new GameObject("ParallaxGrid").transform;
        m_backgroundRoot.SetParent(transform, false);
        m_tiles = new Transform[9];
        int index = 0;
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                GameObject tile = new GameObject("Grid_" + index);
                tile.transform.SetParent(m_backgroundRoot, false);
                tile.transform.localPosition = new Vector3(x * m_tileSize.x, y * m_tileSize.y, 0f);
                SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = _background;
                renderer.sortingOrder = -10;
                m_tiles[index++] = tile.transform;
            }
        }
        m_backgroundRoot.position = m_cameraCenter;
    }

    private void Update()
    {
        if (m_backgroundRoot == null || m_mainCamera == null || m_player == null) return;
        Vector2 targetOffset = -m_player.MovementVelocity * m_parallaxStrength;
        targetOffset.x = Mathf.Clamp(targetOffset.x, -1.1f, 1.1f);
        targetOffset.y = Mathf.Clamp(targetOffset.y, -0.8f, 0.8f);
        m_smoothOffset = Vector2.SmoothDamp(m_smoothOffset, targetOffset, ref m_offsetVelocity, 0.28f);
        m_backgroundRoot.position = m_cameraCenter + new Vector3(m_smoothOffset.x, m_smoothOffset.y, 0f);
    }

}
