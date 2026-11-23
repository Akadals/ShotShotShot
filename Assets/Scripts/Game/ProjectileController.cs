using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private GameObjectPool m_pool;
    private GameManager m_gameManager;
    private Rigidbody2D m_body;
    private SpriteRenderer m_spriteRenderer;
    private Vector2 m_direction;
    private float m_speed;
    private bool m_isEnemyShot;
    private bool m_isActive;
    private Camera m_mainCamera;
    private float m_halfWidth;
    private float m_halfHeight;

    public void Initialize(GameObjectPool _ownerPool, Sprite _sprite, Vector2 _direction, float _speed,
        bool _isEnemyShot, Camera _camera, float _width, float _height, GameManager _gameManager)
    {
        m_pool = _ownerPool;
        m_gameManager = _gameManager;
        m_direction = _direction.normalized;
        m_speed = _speed;
        m_isEnemyShot = _isEnemyShot;
        m_mainCamera = _camera;
        m_halfWidth = _width;
        m_halfHeight = _height;
        if (m_body == null) m_body = GetComponent<Rigidbody2D>();
        if (m_spriteRenderer == null) m_spriteRenderer = GetComponent<SpriteRenderer>();
        if (_sprite != null) m_spriteRenderer.sprite = _sprite;
        m_spriteRenderer.color = _isEnemyShot ? new Color(1f, 0.38f, 0.45f) : Color.white;
        float angle = Mathf.Atan2(m_direction.y, m_direction.x) * Mathf.Rad2Deg - 90f;
        m_body.rotation = angle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        m_isActive = true;
    }

    private void FixedUpdate()
    {
        if (!m_isActive) return;
        Vector2 position = m_body.position + m_direction * (m_speed * Time.fixedDeltaTime);
        m_body.MovePosition(position);
        if (m_mainCamera == null) return;

        Vector2 cameraPosition = m_mainCamera.transform.position;
        Vector2 localPosition = position - cameraPosition;
        if (Mathf.Abs(localPosition.x) > m_halfWidth + 2f || Mathf.Abs(localPosition.y) > m_halfHeight + 2f)
            Release();
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (!m_isActive) return;
        if (m_isEnemyShot)
        {
            if (!_other.TryGetComponent<PlayerController>(out _)) return;
            m_gameManager.DamagePlayer();
            Release();
            return;
        }

        if (!_other.TryGetComponent<EnemyController>(out EnemyController enemy)) return;
        enemy.TakeHit(1);
        Release();
    }

    private void Release()
    {
        m_isActive = false;
        m_pool.Release(gameObject);
    }
}
