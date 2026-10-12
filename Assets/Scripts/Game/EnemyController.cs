using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private GameObjectPool pool;
    private GameManager gameManager;
    private PlayerController player;
    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private int hitPoints;
    private float speed;
    private float fireTimer;
    private float halfWidth;
    private float halfHeight;
    private bool isActive;

    public void Initialize(GameObjectPool ownerPool, Sprite sprite, int hp, float moveSpeed, PlayerController target, float width, float height)
    {
        pool = ownerPool;
        gameManager = GameManager.Instance;
        player = target;
        hitPoints = hp;
        speed = moveSpeed;
        halfWidth = width;
        halfHeight = height;
        fireTimer = Random.Range(1.05f, 1.75f);

        if (body == null) body = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (sprite != null) spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.white;
        isActive = true;
    }

    private void FixedUpdate()
    {
        if (!isActive || player == null) return;

        Vector2 position = body.position;
        Vector2 toPlayer = (Vector2)player.transform.position - position;
        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            Vector2 direction = toPlayer.normalized;
            body.MovePosition(position + direction * (speed * Time.fixedDeltaTime));
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            body.MoveRotation(Mathf.MoveTowardsAngle(body.rotation, angle, 220f * Time.fixedDeltaTime));
        }

        fireTimer -= Time.fixedDeltaTime;
        if (fireTimer <= 0f)
        {
            fireTimer = Random.Range(1.8f, 2.9f);
            gameManager?.FireEnemyShot(position - (Vector2)transform.up * 0.4f, player.transform.position);
        }

        if (Mathf.Abs(position.x) > halfWidth + 3f || Mathf.Abs(position.y) > halfHeight + 3f)
            Release();
    }

    public void TakeHit(int damage)
    {
        if (!isActive) return;
        hitPoints -= damage;
        if (hitPoints <= 0)
        {
            isActive = false;
            gameManager?.EnemyDestroyed();
            pool?.Release(gameObject);
        }
        else if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.55f, 0.45f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || !other.TryGetComponent<PlayerController>(out _)) return;
        gameManager?.DamagePlayer();
        Release();
    }

    private void Release()
    {
        isActive = false;
        pool?.Release(gameObject);
    }
}
