using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private GameObjectPool m_pool;
    private GameManager m_gameManager;
    private PlayerController m_player;
    private Rigidbody2D m_body;
    private SpriteRenderer m_spriteRenderer;
    private int m_hitPoints;
    private float m_speed;
    private float m_attackInterval;
    private float m_fireTimer;
    private bool m_isActive;

    public void Initialize(GameObjectPool _ownerPool, EnemyShipData _ship, int _hitPoints, float _speedBonus,
        PlayerController _target, GameManager _gameManager)
    {
        m_pool = _ownerPool;
        m_gameManager = _gameManager;
        m_player = _target;
        m_hitPoints = _hitPoints;
        m_speed = _ship.MoveSpeed + _speedBonus;
        m_attackInterval = _ship.AttackInterval;
        m_fireTimer = m_attackInterval * Random.Range(0.65f, 1f);

        if (m_body == null) m_body = GetComponent<Rigidbody2D>();
        if (m_spriteRenderer == null) m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_spriteRenderer.sprite = _ship.Sprite;
        m_spriteRenderer.color = Color.white;
        m_isActive = true;
    }

    private void FixedUpdate()
    {
        if (!m_isActive || !m_gameManager.IsPlaying) return;

        Vector2 position = m_body.position;
        Vector2 toPlayer = (Vector2)m_player.transform.position - position;
        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            Vector2 direction = toPlayer.normalized;
            m_body.MovePosition(position + direction * (m_speed * Time.fixedDeltaTime));
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            m_body.MoveRotation(Mathf.MoveTowardsAngle(m_body.rotation, angle, 220f * Time.fixedDeltaTime));
        }

        m_fireTimer -= Time.fixedDeltaTime;
        if (m_fireTimer <= 0f)
        {
            m_fireTimer = m_attackInterval;
            m_gameManager.FireEnemyShot(position - (Vector2)transform.up * 0.4f, m_player.transform.position);
        }
    }

    public void TakeHit(int _damage)
    {
        if (!m_isActive) return;
        m_hitPoints -= _damage;
        if (m_hitPoints <= 0)
        {
            m_isActive = false;
            m_gameManager.EnemyDestroyed();
            m_pool.Release(gameObject);
        }
        else if (m_spriteRenderer != null) m_spriteRenderer.color = new Color(1f, 0.55f, 0.45f);
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (!m_isActive || !_other.TryGetComponent<PlayerController>(out _)) return;
        m_gameManager.DamagePlayer();
        Release();
    }

    private void Release()
    {
        m_isActive = false;
        m_pool.Release(gameObject);
    }
}
