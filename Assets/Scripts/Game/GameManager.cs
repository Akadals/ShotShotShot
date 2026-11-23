using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController m_player;

    private readonly List<AsyncOperationHandle> m_assetHandles = new List<AsyncOperationHandle>();
    private int m_score;
    private int m_wave;
    private float m_elapsed;
    private int m_displayedElapsedSecond = -1;
    private bool m_isPlaying;
    private bool m_isPaused;
    private float m_nextWaveTime;
    private float m_cameraHalfWidth;
    private float m_cameraHalfHeight;
    private Camera m_mainCamera;
    private GameHud m_hud;
    private GameBackgroundMotion m_background;
    private GameObjectPool m_enemyPool;
    private GameObjectPool m_playerProjectilePool;
    private GameObjectPool m_enemyProjectilePool;
    private GameObject m_hudPrefab;
    private Sprite m_playerSprite;
    private Sprite m_playerMissileSprite;
    private Sprite m_enemyMissileSprite;
    private Sprite m_backgroundSprite;
    private readonly List<EnemyShipData> m_enemyShips = new();

    public bool IsPlaying => m_isPlaying && !m_isPaused;

    private IEnumerator Start()
    {
        if (m_player == null)
        {
            Debug.LogError("GameManager needs a PlayerController assigned in the Inspector.", this);
            yield break;
        }

        m_player.HealthChanged += OnHealthChanged;
        m_player.HealthDepleted += EndGame;
        yield return LoadGameAssets();
        StartGame();
    }

    private IEnumerator LoadGameAssets()
    {
        AsyncOperationHandle<GameObject> hudHandle = Addressables.LoadAssetAsync<GameObject>("GameHUD");
        m_assetHandles.Add(hudHandle);
        yield return hudHandle;
        if (hudHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("GameHUD address was not found in Addressables.");
            yield break;
        }
        m_hudPrefab = hudHandle.Result;

        AsyncOperationHandle<Sprite> playerHandle = Addressables.LoadAssetAsync<Sprite>("PlayerShip");
        m_assetHandles.Add(playerHandle);
        yield return playerHandle;
        m_playerSprite = playerHandle.Status == AsyncOperationStatus.Succeeded ? playerHandle.Result : null;

        AsyncOperationHandle<Sprite> playerMissileHandle = Addressables.LoadAssetAsync<Sprite>("PlayerMissile");
        m_assetHandles.Add(playerMissileHandle);
        yield return playerMissileHandle;
        m_playerMissileSprite = playerMissileHandle.Status == AsyncOperationStatus.Succeeded ? playerMissileHandle.Result : null;

        AsyncOperationHandle<Sprite> enemyMissileHandle = Addressables.LoadAssetAsync<Sprite>("EnemyMissile");
        m_assetHandles.Add(enemyMissileHandle);
        yield return enemyMissileHandle;
        m_enemyMissileSprite = enemyMissileHandle.Status == AsyncOperationStatus.Succeeded ? enemyMissileHandle.Result : null;

        AsyncOperationHandle<Sprite> backgroundHandle = Addressables.LoadAssetAsync<Sprite>("Background");
        m_assetHandles.Add(backgroundHandle);
        yield return backgroundHandle;
        m_backgroundSprite = backgroundHandle.Status == AsyncOperationStatus.Succeeded ? backgroundHandle.Result : null;

        AsyncOperationHandle<IList<EnemyShipData>> enemyHandle = Addressables.LoadAssetsAsync<EnemyShipData>("EnemyShipData", null);
        m_assetHandles.Add(enemyHandle);
        yield return enemyHandle;
        if (enemyHandle.Status != AsyncOperationStatus.Succeeded) yield break;

        foreach (EnemyShipData ship in enemyHandle.Result)
        {
            if (ship != null && ship.Sprite != null) m_enemyShips.Add(ship);
        }
    }

    private void StartGame()
    {
        if (m_hudPrefab == null || m_playerSprite == null)
        {
            Debug.LogError("Game assets could not be loaded. Check Addressables addresses and build the content.");
            return;
        }

        if (m_enemyShips.Count == 0)
        {
            Debug.LogError("Assign a sprite to an enemy ship asset and add the EnemyShipData Addressables label.", this);
            return;
        }

        if (m_isPlaying) return;
        Time.timeScale = 1f;
        ReadCameraBounds();
        m_score = 0;
        m_wave = 0;
        m_elapsed = 0f;
        m_displayedElapsedSecond = -1;
        m_isPlaying = true;
        m_isPaused = false;
        SetupPools();
        m_player.Initialize(m_playerSprite, this);
        CreateBackground();
        CreateHud();
        UpdateHud();
        m_nextWaveTime = Time.time + 0.8f;
    }

    private void Update()
    {
        if (!m_isPlaying) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) PauseOrResume();
        if (m_isPaused) return;

        m_elapsed += Time.deltaTime;
        if (Time.time >= m_nextWaveTime) SpawnWave();
        if (Mathf.FloorToInt(m_elapsed) != m_displayedElapsedSecond) UpdateHud();
    }

    private void ReadCameraBounds()
    {
        m_mainCamera = Camera.main;
        if (m_mainCamera == null) return;
        m_cameraHalfHeight = m_mainCamera.orthographicSize;
        m_cameraHalfWidth = m_cameraHalfHeight * m_mainCamera.aspect;
    }

    private void SetupPools()
    {
        if (m_enemyPool != null)
        {
            m_enemyPool.ReleaseAll();
            m_playerProjectilePool.ReleaseAll();
            m_enemyProjectilePool.ReleaseAll();
            return;
        }

        Transform root = transform.Find("Pools");
        if (root == null)
        {
            GameObject poolRoot = new GameObject("Pools");
            poolRoot.transform.SetParent(transform, false);
            root = poolRoot.transform;
        }

        m_enemyPool = new GameObjectPool(MakeTemplate("EnemyShipTemplate", root, 0.42f, true), root, 48);
        m_playerProjectilePool = new GameObjectPool(MakeTemplate("PlayerMissileTemplate", root, 0.12f, false), root, 48);
        m_enemyProjectilePool = new GameObjectPool(MakeTemplate("EnemyMissileTemplate", root, 0.12f, false), root, 48);
    }

    private GameObject MakeTemplate(string _name, Transform _parent, float _colliderSize, bool _isEnemy)
    {
        GameObject item = new GameObject(_name);
        item.transform.SetParent(_parent, false);
        SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = _isEnemy ? 1 : 2;
        Rigidbody2D body = item.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Dynamic;
        if (_isEnemy) body.interpolation = RigidbodyInterpolation2D.Interpolate;
        CircleCollider2D collider = item.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = _colliderSize;
        if (_isEnemy) item.AddComponent<EnemyController>();
        else item.AddComponent<ProjectileController>();
        item.SetActive(false);
        return item;
    }

    private void CreateBackground()
    {
        GameObject backgroundObject = new GameObject("GameBackground");
        backgroundObject.transform.SetParent(transform, false);
        m_background = backgroundObject.AddComponent<GameBackgroundMotion>();
        m_background.Initialize(m_player, m_mainCamera, m_backgroundSprite);
    }

    private void CreateHud()
    {
        GameObject hudObject = Instantiate(m_hudPrefab);
        m_hud = hudObject.GetComponent<GameHud>();
        if (m_hud == null)
        {
            Debug.LogError("GameHUD prefab is missing the GameHud component.");
            Destroy(hudObject);
            return;
        }

        m_hud.Initialize(RestartGame, ReturnToTitle);
        m_hud.SetHealth(m_player.CurrentHealth, m_player.MaxHealth);
    }

    private void UpdateHud()
    {
        m_displayedElapsedSecond = Mathf.FloorToInt(m_elapsed);
        if (m_hud != null) m_hud.SetStats(m_score, m_wave, m_elapsed);
    }

    public void FirePlayerShot(Vector3 _position, Vector2 _direction)
    {
        if (!IsPlaying || m_playerProjectilePool == null) return;
        CreateProjectile(m_playerProjectilePool, m_playerMissileSprite, _position, _direction, 10f, false);
        AudioManager.Instance?.PlayEffect("Fire", 0.38f);
    }

    public void FireEnemyShot(Vector3 _position, Vector3 _targetPosition)
    {
        if (!IsPlaying || m_enemyProjectilePool == null) return;
        Vector2 direction = ((Vector2)_targetPosition - (Vector2)_position).normalized;
        CreateProjectile(m_enemyProjectilePool, m_enemyMissileSprite, _position, direction, 4.5f, true);
    }

    private void CreateProjectile(GameObjectPool _pool, Sprite _sprite, Vector3 _position, Vector2 _direction, float _speed, bool _enemyShot)
    {
        GameObject projectileObject = _pool.Get(_position, Quaternion.identity);
        projectileObject.transform.localScale = Vector3.one * (_enemyShot ? 0.65f : 0.8f);
        ProjectileController projectile = projectileObject.GetComponent<ProjectileController>();
        projectileObject.SetActive(true);
        projectile.Initialize(_pool, _sprite, _direction, _speed, _enemyShot, m_mainCamera, m_cameraHalfWidth, m_cameraHalfHeight, this);
    }

    private void SpawnWave()
    {
        m_wave++;
        UpdateHud();

        int enemyCount = Random.Range(8, 13);
        float angleStep = 2f * Mathf.PI / enemyCount;
        float startAngle = Random.Range(0f, 2f * Mathf.PI);
        float radiusX = (m_cameraHalfWidth + 0.6f) * Mathf.Sqrt(2f);
        float radiusY = (m_cameraHalfHeight + 0.6f) * Mathf.Sqrt(2f);
        Vector3 center = m_mainCamera.transform.position;
        for (int i = 0; i < enemyCount; i++)
        {
            float angle = startAngle + i * angleStep + Random.Range(-0.2f, 0.2f) * angleStep;
            Vector3 position = new Vector3(center.x + Mathf.Cos(angle) * radiusX, center.y + Mathf.Sin(angle) * radiusY, 0f);
            SpawnEnemy(position);
        }

        m_nextWaveTime = Time.time + Mathf.Max(2f, 5.5f - m_wave * 0.12f);
    }

    private void SpawnEnemy(Vector3 _position)
    {
        GameObject enemyObject = m_enemyPool.Get(_position, Quaternion.identity);
        enemyObject.transform.localScale = Vector3.one * Random.Range(0.75f, 0.95f);
        EnemyShipData ship = m_enemyShips[Random.Range(0, m_enemyShips.Count)];
        EnemyController enemy = enemyObject.GetComponent<EnemyController>();
        int enemyHp = m_wave > 3 && Random.value < 0.25f ? 2 : 1;
        float speedBonus = Mathf.Min(m_wave * 0.055f, 1.3f);
        enemy.Initialize(m_enemyPool, ship, enemyHp, speedBonus, m_player, this);
        enemyObject.SetActive(true);
    }

    public void EnemyDestroyed()
    {
        if (!IsPlaying) return;
        m_score += 100;
        UpdateHud();
        AudioManager.Instance?.PlayEffect("Explosion", 0.3f);
    }

    public void DamagePlayer()
    {
        if (IsPlaying) m_player.TakeDamage();
    }

    private void OnHealthChanged(int _current, int _maximum)
    {
        if (m_hud != null) m_hud.SetHealth(_current, _maximum);
    }

    private void EndGame()
    {
        m_isPlaying = false;
        int best = PlayerPrefs.GetInt("ShotShotShot_BestScore", 0);
        if (m_score > best)
        {
            best = m_score;
            PlayerPrefs.SetInt("ShotShotShot_BestScore", best);
            PlayerPrefs.Save();
        }

        if (m_player != null) m_player.SetVisible(false);
        UpdateHud();
        if (m_hud != null) m_hud.ShowGameOver(m_score, best);
    }

    private void RestartGame()
    {
        if (m_hud != null) Destroy(m_hud.gameObject);
        if (m_background != null) Destroy(m_background.gameObject);
        m_hud = null;
        m_background = null;
        m_isPlaying = false;
        StartGame();
    }

    private void ReturnToTitle()
    {
        m_isPlaying = false;
        ResumeGame();
        SceneManager.Instance.LoadScene("sTitle");
    }

    private void PauseOrResume()
    {
        PopupManager popup = PopupManager.Instance;
        if (popup == null) return;
        if (m_isPaused)
        {
            popup.Close();
            return;
        }

        if (!popup.TryOpenSettings(ResumeGame, ReturnToTitle)) return;
        m_isPaused = true;
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        m_isPaused = false;
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (m_isPaused)
        {
            PopupManager.Instance?.Close();
            ResumeGame();
        }

        if (m_player != null)
        {
            m_player.HealthChanged -= OnHealthChanged;
            m_player.HealthDepleted -= EndGame;
        }

        foreach (AsyncOperationHandle handle in m_assetHandles)
            if (handle.IsValid()) Addressables.Release(handle);
    }
}
