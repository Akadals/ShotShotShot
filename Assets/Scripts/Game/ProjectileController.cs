using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private GameObjectPool pool;
    private GameManager gameManager;
    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Vector2 direction;
    private float speed;
    private bool isEnemyShot;
    private bool isActive;
    private Camera mainCamera;
    private float halfWidth;
    private float halfHeight;

    public void Initialize(GameObjectPool ownerPool, Sprite sprite, Vector2 travelDirection, float moveSpeed, bool enemyShot, Camera camera, float width, float height)
    {
        pool = ownerPool;
        gameManager = GameManager.Instance;
        direction = travelDirection.normalized;
        speed = moveSpeed;
        isEnemyShot = enemyShot;
        mainCamera = camera;
        halfWidth = width;
        halfHeight = height;
        if (body == null) body = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (sprite != null) spriteRenderer.sprite = sprite;
        spriteRenderer.color = enemyShot ? new Color(1f, 0.38f, 0.45f) : Color.white;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        body.rotation = angle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        isActive = true;
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        Vector2 position = body.position + direction * (speed * Time.fixedDeltaTime);
        body.MovePosition(position);
        if (mainCamera == null) return;

        Vector2 cameraPosition = mainCamera.transform.position;
        Vector2 localPosition = position - cameraPosition;
        if (Mathf.Abs(localPosition.x) > halfWidth + 2f || Mathf.Abs(localPosition.y) > halfHeight + 2f)
            Release();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (isEnemyShot)
        {
            if (!other.TryGetComponent<PlayerController>(out _)) return;
            gameManager?.DamagePlayer();
            Release();
            return;
        }

        if (!other.TryGetComponent<EnemyController>(out EnemyController enemy)) return;
        enemy.TakeHit(1);
        Release();
    }

    private void Release()
    {
        isActive = false;
        pool?.Release(gameObject);
    }
}
