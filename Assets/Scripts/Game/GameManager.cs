using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : Shared<GameManager>
{
    private const int MaxHp = 3;
    private static readonly WaitForSeconds SpawnDelay = new WaitForSeconds(0.07f);
    private int hp;
    private int score;
    private int wave;
    private float elapsed;
    private int displayedElapsedSecond = -1;
    private bool isPlaying;
    private bool isSpawning;
    private float nextWaveTime;
    private float cameraHalfWidth;
    private float cameraHalfHeight;
    private Camera mainCamera;
    private PlayerController player;
    private GameHud hud;
    private GameBackgroundMotion background;
    private GameObjectPool enemyPool;
    private GameObjectPool playerProjectilePool;
    private GameObjectPool enemyProjectilePool;
    private Sprite playerSprite;
    private Sprite[] enemySprites;
    private Sprite playerMissileSprite;
    private Sprite enemyMissileSprite;
    private AudioSource audioSource;
    private AudioClip fireClip;
    private AudioClip explosionClip;
    private AudioMixer mixer;
    private GameObject hudPrefab;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        ReadCameraBounds();
        hudPrefab = Resources.Load<GameObject>("Prefabs/UI/GameHUD");
        Sprite[] shipSprites = Resources.LoadAll<Sprite>("kenney_space-shooter-extension/PNG/Sprites/Ships");
        playerSprite = FindSprite(shipSprites, "spaceShips_001");
        List<Sprite> enemies = new List<Sprite>();
        foreach (Sprite sprite in shipSprites)
        {
            if (sprite != null && sprite != playerSprite) enemies.Add(sprite);
        }
        enemySprites = enemies.Count > 0 ? enemies.ToArray() : shipSprites;
        Sprite[] missiles = Resources.LoadAll<Sprite>("kenney_space-shooter-extension/PNG/Sprites/Missiles");
        playerMissileSprite = FindSprite(missiles, "spaceMissiles_001");
        enemyMissileSprite = FindSprite(missiles, "spaceMissiles_003");
        SetupAudio();
    }

    private void Update()
    {
        if (!isPlaying) return;
        elapsed += Time.deltaTime;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) PauseOrResume();
        if (!isSpawning && Time.time >= nextWaveTime) StartCoroutine(SpawnFormation());
        if (Mathf.FloorToInt(elapsed) != displayedElapsedSecond) UpdateHud();
    }

    public void StartGame()
    {
        if (isPlaying) return;
        Time.timeScale = 1f;
        ReadCameraBounds();
        hp = MaxHp;
        score = 0;
        wave = 0;
        elapsed = 0f;
        displayedElapsedSecond = -1;
        isPlaying = true;
        SetupPools();
        CreatePlayer();
        CreateBackground();
        CreateHud();
        UpdateHud();
        nextWaveTime = Time.time + 0.8f;
    }

    private void ReadCameraBounds()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) return;
        cameraHalfHeight = mainCamera.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * mainCamera.aspect;
    }

    private void SetupPools()
    {
        if (enemyPool != null && playerProjectilePool != null && enemyProjectilePool != null)
        {
            enemyPool.ReleaseAll();
            playerProjectilePool.ReleaseAll();
            enemyProjectilePool.ReleaseAll();
            return;
        }
        Transform root = transform.Find("Pools");
        if (root == null)
        {
            GameObject poolRoot = new GameObject("Pools");
            poolRoot.transform.SetParent(transform, false);
            root = poolRoot.transform;
        }
        GameObject enemyTemplate = MakeTemplate("EnemyShipTemplate", root, 0.42f, true);
        GameObject playerShotTemplate = MakeTemplate("PlayerMissileTemplate", root, 0.12f, false);
        GameObject enemyShotTemplate = MakeTemplate("EnemyMissileTemplate", root, 0.12f, false);
        enemyPool = new GameObjectPool(enemyTemplate, root, 48);
        playerProjectilePool = new GameObjectPool(playerShotTemplate, root, 48);
        enemyProjectilePool = new GameObjectPool(enemyShotTemplate, root, 48);
    }

    private GameObject MakeTemplate(string objectName, Transform parent, float colliderSize, bool isEnemy)
    {
        GameObject item = new GameObject(objectName);
        item.transform.SetParent(parent, false);
        SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = isEnemy ? 1 : 2;
        Rigidbody2D body = item.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Dynamic;
        if (isEnemy) body.interpolation = RigidbodyInterpolation2D.Interpolate;
        CircleCollider2D collider = item.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = colliderSize;
        if (isEnemy) item.AddComponent<EnemyController>();
        else item.AddComponent<ProjectileController>();
        item.SetActive(false);
        return item;
    }

    private void CreatePlayer()
    {
        GameObject playerObject = new GameObject("PlayerShip");
        playerObject.transform.position = new Vector3(0f, -2.7f, 0f);
        SpriteRenderer renderer = playerObject.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 3;
        Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Dynamic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        CircleCollider2D collider = playerObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.38f;
        player = playerObject.AddComponent<PlayerController>();
        player.Initialize(playerSprite);
    }

    private void CreateBackground()
    {
        GameObject backgroundObject = new GameObject("GameBackground");
        backgroundObject.transform.SetParent(transform, false);
        background = backgroundObject.AddComponent<GameBackgroundMotion>();
        background.Initialize(player, mainCamera);
    }

    private void CreateHud()
    {
        if (hudPrefab == null)
        {
            Debug.LogError("GameHUD prefab was not found in Resources/Prefabs/UI.");
            return;
        }

        GameObject hudObject = Instantiate(hudPrefab);
        hud = hudObject.GetComponent<GameHud>();
        if (hud == null)
        {
            Debug.LogError("GameHUD prefab is missing the GameHud component.");
            Destroy(hudObject);
            return;
        }
        hud.Initialize(RestartGame, ReturnToTitle);
    }
    private void UpdateHud()
    {
        displayedElapsedSecond = Mathf.FloorToInt(elapsed);
        if (hud != null) hud.SetStats(score, wave, elapsed, hp, MaxHp);
    }

    public void FirePlayerShot(Vector3 position, Vector2 direction)
    {
        if (!isPlaying || playerProjectilePool == null) return;
        CreateProjectile(playerProjectilePool, playerMissileSprite, position, direction, 10f, false);
        PlaySound(fireClip, 0.38f);
    }

    public void FireEnemyShot(Vector3 position, Vector3 targetPosition)
    {
        if (!isPlaying || enemyProjectilePool == null) return;
        Vector2 direction = ((Vector2)targetPosition - (Vector2)position).normalized;
        CreateProjectile(enemyProjectilePool, enemyMissileSprite, position, direction, 4.5f, true);
    }

    private void CreateProjectile(GameObjectPool pool, Sprite sprite, Vector3 position, Vector2 direction, float speed, bool enemyShot)
    {
        GameObject projectileObject = pool.Get(position, Quaternion.identity);
        projectileObject.transform.localScale = Vector3.one * (enemyShot ? 0.65f : 0.8f);
        ProjectileController projectile = projectileObject.GetComponent<ProjectileController>();
        projectileObject.SetActive(true);
        projectile.Initialize(pool, sprite, direction, speed, enemyShot, mainCamera, cameraHalfWidth, cameraHalfHeight);
    }

    private IEnumerator SpawnFormation()
    {
        isSpawning = true;
        wave++;
        UpdateHud();
        int shipsPerSide = Mathf.Clamp(1 + wave / 2, 2, 5);
        for (int side = 0; side < 4; side++)
        {
            for (int i = 0; i < shipsPerSide; i++)
            {
                SpawnEnemy(side, i, shipsPerSide);
                yield return SpawnDelay;
            }
        }
        isSpawning = false;
        nextWaveTime = Time.time + Mathf.Max(2f, 5.5f - wave * 0.12f);
    }

    private void SpawnEnemy(int side, int index, int count)
    {
        float spacing = 1.35f;
        float offset = (index - (count - 1) * 0.5f) * spacing;
        Vector3 position;
        if (side == 0) position = new Vector3(offset, cameraHalfHeight + 1.2f, 0f);
        else if (side == 1) position = new Vector3(offset, -cameraHalfHeight - 1.2f, 0f);
        else if (side == 2) position = new Vector3(-cameraHalfWidth - 1.2f, offset, 0f);
        else position = new Vector3(cameraHalfWidth + 1.2f, offset, 0f);
        position.x += Random.Range(-0.25f, 0.25f);
        position.y += Random.Range(-0.25f, 0.25f);

        GameObject enemyObject = enemyPool.Get(position, Quaternion.identity);
        enemyObject.transform.localScale = Vector3.one * Random.Range(0.75f, 0.95f);
        Sprite sprite = enemySprites.Length > 0 ? enemySprites[Random.Range(0, enemySprites.Length)] : null;
        EnemyController enemy = enemyObject.GetComponent<EnemyController>();
        int enemyHp = wave > 3 && Random.value < 0.25f ? 2 : 1;
        enemy.Initialize(enemyPool, sprite, enemyHp, 1.0f + Mathf.Min(wave * 0.055f, 1.3f), player, cameraHalfWidth, cameraHalfHeight);
        enemyObject.SetActive(true);
    }

    public void EnemyDestroyed()
    {
        if (!isPlaying) return;
        score += 100;
        UpdateHud();
        PlaySound(explosionClip, 0.3f);
    }

    public void DamagePlayer()
    {
        if (!isPlaying) return;
        hp--;
        if (hp <= 0) EndGame();
        else UpdateHud();
    }

    private void EndGame()
    {
        StopAllCoroutines();
        isSpawning = false;
        isPlaying = false;
        int best = PlayerPrefs.GetInt("ShotShotShot_BestScore", 0);
        if (score > best)
        {
            best = score;
            PlayerPrefs.SetInt("ShotShotShot_BestScore", best);
            PlayerPrefs.Save();
        }
        if (player != null) player.SetVisible(false);
        UpdateHud();
        if (hud != null) hud.ShowGameOver(score, best);
    }

    private void RestartGame()
    {
        if (player != null) Destroy(player.gameObject);
        if (hud != null) Destroy(hud.gameObject);
        if (background != null) Destroy(background.gameObject);
        player = null;
        hud = null;
        background = null;
        isPlaying = false;
        StartGame();
    }

    private void ReturnToTitle()
    {
        Time.timeScale = 1f;
        StopAllCoroutines();
        isSpawning = false;
        isPlaying = false;
        if (player != null) Destroy(player.gameObject);
        if (hud != null) Destroy(hud.gameObject);
        if (background != null) Destroy(background.gameObject);
        enemyPool?.ReleaseAll();
        playerProjectilePool?.ReleaseAll();
        enemyProjectilePool?.ReleaseAll();
        SceneTransition transition = SceneTransition.Instance;
        if (transition != null) transition.LoadScene("sTitle");
        else SceneManager.LoadScene("sTitle");
        Destroy(gameObject);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PauseOrResume()
    {
        Time.timeScale = Time.timeScale > 0f ? 0f : 1f;
    }

    private void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        mixer = Resources.Load<AudioMixer>("Mixer");
        if (mixer != null)
        {
            AudioMixerGroup[] groups = mixer.FindMatchingGroups("VFX");
            if (groups.Length > 0) audioSource.outputAudioMixerGroup = groups[0];
        }
        fireClip = Resources.Load<AudioClip>("kenney_sci-fi-sounds/Audio/laserRetro_000");
        explosionClip = Resources.Load<AudioClip>("kenney_sci-fi-sounds/Audio/explosionCrunch_000");
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip, volume);
    }

    private static Sprite FindSprite(Sprite[] sprites, string spriteName)
    {
        if (sprites == null) return null;
        foreach (Sprite sprite in sprites)
        {
            if (sprite != null && sprite.name == spriteName) return sprite;
        }
        return sprites.Length > 0 ? sprites[0] : null;
    }
}






