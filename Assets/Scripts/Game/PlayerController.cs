using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Min(1)] private int m_maxHealth = 8;
    [SerializeField, Min(0f)] private float m_rotationSpeed = 420f;
    [SerializeField, Min(0f)] private float m_mouseFollowSpeed = 30f;

    private Camera m_mainCamera;
    private Rigidbody2D m_body;
    private Vector3 m_startPosition;
    private float m_startRotation;
    private Vector3 m_velocity;
    private Vector3 m_smoothPosition;
    private float m_targetRotation;
    private float m_fireTimer;
    private float m_boundaryX;
    private float m_boundaryY;
    private SpriteRenderer m_spriteRenderer;
    private Vector2 m_facingDirection = Vector2.up;
    private GameManager m_gameManager;

    public Vector2 MovementVelocity => m_velocity;
    public int CurrentHealth { get; private set; }
    public int MaxHealth => m_maxHealth;

    public event Action<int, int> HealthChanged;
    public event Action HealthDepleted;

    private void Awake()
    {
        m_body = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_startPosition = transform.position;
        m_startRotation = transform.eulerAngles.z;
    }

    public void Initialize(Sprite _shipSprite, GameManager _gameManager)
    {
        m_mainCamera = Camera.main;
        m_gameManager = _gameManager;
        if (_shipSprite != null) m_spriteRenderer.sprite = _shipSprite;
        if (m_mainCamera != null)
        {
            m_boundaryY = m_mainCamera.orthographicSize - 0.55f;
            m_boundaryX = m_boundaryY * m_mainCamera.aspect - 0.45f;
        }

        transform.SetPositionAndRotation(m_startPosition, Quaternion.Euler(0f, 0f, m_startRotation));
        m_body.position = m_startPosition;
        m_body.rotation = m_startRotation;
        m_body.linearVelocity = Vector2.zero;
        m_body.angularVelocity = 0f;
        m_smoothPosition = m_startPosition;
        m_velocity = Vector3.zero;
        m_facingDirection = transform.up;
        m_targetRotation = m_startRotation;
        m_fireTimer = 0f;

        CurrentHealth = m_maxHealth;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (m_gameManager == null || !m_gameManager.IsPlaying || m_mainCamera == null) return;
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 pointer = mouse.position.ReadValue();
            Vector3 point = m_mainCamera.ScreenToWorldPoint(new Vector3(pointer.x, pointer.y, -m_mainCamera.transform.position.z));
            point.z = 0f;
            point.x = Mathf.Clamp(point.x, -m_boundaryX, m_boundaryX);
            point.y = Mathf.Clamp(point.y, -m_boundaryY, m_boundaryY);
            m_smoothPosition = Vector3.SmoothDamp(m_smoothPosition, point, ref m_velocity, 0.12f, m_mouseFollowSpeed);

            Vector2 aimDirection = (Vector2)point - (Vector2)transform.position;
            if (aimDirection.sqrMagnitude > 0.01f)
            {
                m_facingDirection = aimDirection.normalized;
                m_targetRotation = Mathf.Atan2(m_facingDirection.y, m_facingDirection.x) * Mathf.Rad2Deg - 90f;
            }
        }

        m_fireTimer -= Time.deltaTime;
        bool firing = (mouse != null && mouse.leftButton.isPressed)
            || (Keyboard.current != null && Keyboard.current.spaceKey.isPressed);
        if (firing && m_fireTimer <= 0f)
        {
            m_fireTimer = 0.18f;
            Vector3 muzzlePosition = transform.position + (Vector3)(m_facingDirection * 0.48f);
            m_gameManager.FirePlayerShot(muzzlePosition, m_facingDirection);
        }
    }

    private void FixedUpdate()
    {
        if (m_gameManager == null || !m_gameManager.IsPlaying) return;
        m_body.MovePosition(m_smoothPosition);
        m_body.MoveRotation(Mathf.MoveTowardsAngle(m_body.rotation, m_targetRotation, m_rotationSpeed * Time.fixedDeltaTime));
    }

    public void TakeDamage(int _amount = 1)
    {
        if (CurrentHealth <= 0 || _amount <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - _amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        if (CurrentHealth == 0) HealthDepleted?.Invoke();
    }

    public void SetVisible(bool _visible)
    {
        gameObject.SetActive(_visible);
    }
}
