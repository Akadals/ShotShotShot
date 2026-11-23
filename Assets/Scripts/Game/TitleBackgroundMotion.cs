using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TitleBackgroundMotion : MonoBehaviour
{
    [SerializeField] private float m_scrollSpeed = 0.28f;
    [SerializeField] private float m_meteorInterval = 3.2f;
    [SerializeField] private float m_meteorSpeed = 0.55f;
    [SerializeField] private float m_tileHeight = 15.36f;
    [SerializeField] private int m_meteorPoolSize = 24;

    private Transform[] m_tiles;
    private float m_nextMeteorTime;
    private Camera m_mainCamera;
    private Sprite[] m_meteorSprites;
    private GameObjectPool m_meteorPool;
    private AsyncOperationHandle<System.Collections.Generic.IList<Sprite>> m_meteorHandle;

    private IEnumerator Start()
    {
        m_meteorSprites = System.Array.Empty<Sprite>();
        m_meteorHandle = Addressables.LoadAssetsAsync<Sprite>("TitleMeteor", null);
        yield return m_meteorHandle;
        if (m_meteorHandle.Status == AsyncOperationStatus.Succeeded)
            m_meteorSprites = new System.Collections.Generic.List<Sprite>(m_meteorHandle.Result).ToArray();
        else
            Debug.LogError("No sprites have the Addressables label 'TitleMeteor'.");

        m_mainCamera = Camera.main;
        m_tiles = new Transform[transform.childCount];
        for (int i = 0; i < m_tiles.Length; i++) m_tiles[i] = transform.GetChild(i);
        UpdateTileHeight();

        if (m_mainCamera != null && m_meteorSprites.Length > 0) CreateMeteorPool();
        m_nextMeteorTime = Time.time + Random.Range(0.5f, m_meteorInterval);
    }

    private void Update()
    {
        if (m_tiles == null) return;
        foreach (Transform tile in m_tiles)
        {
            tile.localPosition += Vector3.down * (m_scrollSpeed * Time.deltaTime);
            if (tile.localPosition.y < -m_tileHeight * 1.5f)
                tile.localPosition += Vector3.up * m_tileHeight * 3f;
        }

        if (m_meteorPool != null && Time.time >= m_nextMeteorTime)
        {
            SpawnMeteorBurst();
            m_nextMeteorTime = Time.time + Random.Range(m_meteorInterval * 0.8f, m_meteorInterval * 1.4f);
        }
    }

    private void UpdateTileHeight()
    {
        if (m_tiles.Length < 2) return;
        float highest = float.MinValue;
        float secondHighest = float.MinValue;
        foreach (Transform tile in m_tiles)
        {
            float y = tile.localPosition.y;
            if (y > highest)
            {
                secondHighest = highest;
                highest = y;
            }
            else if (y > secondHighest && y < highest)
            {
                secondHighest = y;
            }
        }

        if (secondHighest > float.MinValue) m_tileHeight = Mathf.Abs(highest - secondHighest);
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
        m_meteorPool = new GameObjectPool(template, transform, m_meteorPoolSize);
    }

    private void SpawnMeteorBurst()
    {
        float halfHeight = m_mainCamera.orthographicSize;
        float halfWidth = halfHeight * m_mainCamera.aspect;
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

    private void SpawnMeteor(Vector2 _start, Vector2 _direction, float _halfWidth, float _halfHeight)
    {
        Vector3 position = m_mainCamera.transform.position + new Vector3(_start.x, _start.y, 5f);
        GameObject meteor = m_meteorPool.Get(position, Quaternion.identity);
        meteor.transform.localScale = Vector3.one * Random.Range(0.2f, 0.42f);
        AmbientMeteor motion = meteor.GetComponent<AmbientMeteor>();
        Sprite sprite = m_meteorSprites[Random.Range(0, m_meteorSprites.Length)];
        motion.Initialize(m_meteorPool, m_mainCamera, sprite, _direction,
            m_meteorSpeed * Random.Range(0.65f, 1.35f), _halfWidth, _halfHeight);
        meteor.SetActive(true);
    }

    private void OnDestroy()
    {
        if (m_meteorHandle.IsValid()) Addressables.Release(m_meteorHandle);
    }
}

public class AmbientMeteor : MonoBehaviour
{
    private GameObjectPool m_pool;
    private Camera m_mainCamera;
    private SpriteRenderer m_spriteRenderer;
    private Vector2 m_direction;
    private float m_speed;
    private float m_halfWidth;
    private float m_halfHeight;
    private float m_rotationSpeed;
    private bool m_isActive;

    public void Initialize(GameObjectPool _ownerPool, Camera _camera, Sprite _sprite, Vector2 _moveDirection,
        float _moveSpeed, float _width, float _height)
    {
        m_pool = _ownerPool;
        m_mainCamera = _camera;
        m_direction = _moveDirection;
        m_speed = _moveSpeed;
        m_halfWidth = _width;
        m_halfHeight = _height;
        m_rotationSpeed = Random.Range(-28f, 28f);
        if (m_spriteRenderer == null) m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_spriteRenderer.sprite = _sprite;
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        m_isActive = true;
    }

    private void Update()
    {
        if (!m_isActive) return;
        transform.position += (Vector3)(m_direction * (m_speed * Time.deltaTime));
        transform.Rotate(0f, 0f, m_rotationSpeed * Time.deltaTime);
        Vector3 local = transform.position - m_mainCamera.transform.position;
        if (Mathf.Abs(local.x) > m_halfWidth + 2f || Mathf.Abs(local.y) > m_halfHeight + 2f)
        {
            m_isActive = false;
            m_pool.Release(gameObject);
        }
    }
}
