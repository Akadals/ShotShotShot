using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody2D body;
    private Vector3 velocity;
    private Vector3 smoothPosition;
    private float targetRotation;
    private float fireTimer;
    private float boundaryX;
    private float boundaryY;
    private SpriteRenderer spriteRenderer;
    private Vector2 facingDirection = Vector2.up;

    public Vector2 MovementVelocity => velocity;

    public void Initialize(Sprite shipSprite)
    {
        mainCamera = Camera.main;
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (shipSprite != null) spriteRenderer.sprite = shipSprite;
        if (mainCamera != null)
        {
            boundaryY = mainCamera.orthographicSize - 0.55f;
            boundaryX = boundaryY * mainCamera.aspect - 0.45f;
        }

        smoothPosition = transform.position;
        targetRotation = transform.eulerAngles.z;
    }

    private void Update()
    {
        if (mainCamera == null) return;
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 pointer = mouse.position.ReadValue();
            Vector3 point = mainCamera.ScreenToWorldPoint(new Vector3(pointer.x, pointer.y, -mainCamera.transform.position.z));
            point.z = 0f;
            point.x = Mathf.Clamp(point.x, -boundaryX, boundaryX);
            point.y = Mathf.Clamp(point.y, -boundaryY, boundaryY);
            smoothPosition = Vector3.SmoothDamp(smoothPosition, point, ref velocity, 0.12f, 30f);

            Vector2 aimDirection = (Vector2)point - (Vector2)transform.position;
            if (aimDirection.sqrMagnitude > 0.01f)
            {
                facingDirection = aimDirection.normalized;
                targetRotation = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg - 90f;
            }
        }

        fireTimer -= Time.deltaTime;
        bool firing = (mouse != null && mouse.leftButton.isPressed) || (Keyboard.current != null && Keyboard.current.spaceKey.isPressed);
        if (firing && fireTimer <= 0f)
        {
            fireTimer = 0.18f;
            Vector3 muzzlePosition = transform.position + (Vector3)(facingDirection * 0.48f);
            GameManager.Instance?.FirePlayerShot(muzzlePosition, facingDirection);
        }
    }

    private void FixedUpdate()
    {
        body.MovePosition(smoothPosition);
        body.MoveRotation(Mathf.MoveTowardsAngle(body.rotation, targetRotation, 420f * Time.fixedDeltaTime));
    }

    public void SetVisible(bool visible) => gameObject.SetActive(visible);
}
