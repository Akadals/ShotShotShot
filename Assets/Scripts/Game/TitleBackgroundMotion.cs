using UnityEngine;

public class TitleBackgroundMotion : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.28f;
    [SerializeField] private float meteorInterval = 3.2f;
    [SerializeField] private float meteorSpeed = 0.55f;
    private const int MeteorPoolSize = 24;
    private Transform[] tiles;
    private float tileHeight = 15.36f;
    private float nextMeteorTime;
    private Camera mainCamera;
    private Sprite[] meteorSprites;
    private GameObjectPool meteorPool;

    private void Start()
    {
        mainCamera = Camera.main;
        tiles = new Transform[transform.childCount];
        for (int i = 0; i < tiles.Length; i++) tiles[i] = transform.GetChild(i);
        if (tiles.Length > 1)
        {
            float highest = float.MinValue;
            float second = float.MinValue;
            foreach (Transform tile in tiles)
            {
                float y = tile.localPosition.y;
                if (y > highest) { second = highest; highest = y; }
                else if (y > second && y < highest) second = y;
            }
            if (second > float.MinValue) tileHeight = Mathf.Abs(highest - second);
        }

        meteorSprites = Resources.LoadAll<Sprite>("kenney_space-shooter-extension/PNG/Sprites/Meteors");
        if (mainCamera != null && meteorSprites.Length > 0) CreateMeteorPool();
        nextMeteorTime = Time.time + Random.Range(0.5f, meteorInterval);
    }

    private void Update()
    {
        foreach (Transform tile in tiles)
        {
            tile.localPosition += Vector3.down * (scrollSpeed * Time.deltaTime);
            if (tile.localPosition.y < -tileHeight * 1.5f)
                tile.localPosition += Vector3.up * tileHeight * 3f;
        }

        if (meteorPool != null && Time.time >= nextMeteorTime)
        {
            SpawnThreeWayMeteorBurst();
            nextMeteorTime = Time.time + Random.Range(meteorInterval * 0.8f, meteorInterval * 1.4f);
        }
    }

    private void CreateMeteorPool()
    {
        GameObject template = new GameObject("AmbientMeteorTemplate");
        template.transform.SetParent(transform, false);
        SpriteRenderer renderer = template.AddComponent<SpriteRenderer>();
        renderer.color = new Color(0.58f, 0.76f, 0.92f, 0.3f);
        renderer.sortingOrder = 1;
        template.AddComponent<AmbientMeteor>();
        template.SetActive(false);
        meteorPool = new GameObjectPool(template, transform, MeteorPoolSize);
    }

    private void SpawnThreeWayMeteorBurst()
    {
        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;
        float baseAngle = Random.Range(0f, 360f);
        for (int i = 0; i < 3; i++)
        {
            float angle = (baseAngle + i * 120f) * Mathf.Deg2Rad;
            Vector2 start = new Vector2(Mathf.Cos(angle) * (halfWidth + 1f), Mathf.Sin(angle) * (halfHeight + 1f));
            Vector2 direction = (-start).normalized;
            direction = Quaternion.Euler(0f, 0f, Random.Range(-10f, 10f)) * direction;
            SpawnMeteor(start, direction, halfWidth, halfHeight);
        }
    }

    private void SpawnMeteor(Vector2 start, Vector2 direction, float halfWidth, float halfHeight)
    {
        Vector3 position = mainCamera.transform.position + new Vector3(start.x, start.y, 5f);
        GameObject meteor = meteorPool.Get(position, Quaternion.identity);
        meteor.transform.localScale = Vector3.one * Random.Range(0.2f, 0.42f);
        AmbientMeteor motion = meteor.GetComponent<AmbientMeteor>();
        Sprite sprite = meteorSprites[Random.Range(0, meteorSprites.Length)];
        motion.Initialize(meteorPool, mainCamera, sprite, direction,
            meteorSpeed * Random.Range(0.65f, 1.35f), halfWidth, halfHeight);
        meteor.SetActive(true);
    }
}

public class AmbientMeteor : MonoBehaviour
{
    private GameObjectPool pool;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private Vector2 direction;
    private float speed;
    private float halfWidth;
    private float halfHeight;
    private float rotationSpeed;
    private bool isActive;

    public void Initialize(GameObjectPool ownerPool, Camera camera, Sprite sprite, Vector2 moveDirection, float moveSpeed, float width, float height)
    {
        pool = ownerPool;
        mainCamera = camera;
        direction = moveDirection;
        speed = moveSpeed;
        halfWidth = width;
        halfHeight = height;
        rotationSpeed = Random.Range(-28f, 28f);
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        isActive = true;
    }

    private void Update()
    {
        if (!isActive) return;
        transform.position += (Vector3)(direction * (speed * Time.deltaTime));
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        Vector3 local = transform.position - mainCamera.transform.position;
        if (Mathf.Abs(local.x) > halfWidth + 2f || Mathf.Abs(local.y) > halfHeight + 2f)
        {
            isActive = false;
            pool.Release(gameObject);
        }
    }
}
