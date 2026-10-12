using UnityEngine;

public class GameBackgroundMotion : MonoBehaviour
{
    private static Sprite cachedBackground;
    private static bool backgroundLoadAttempted;
    [SerializeField] private float parallaxStrength = 0.22f;
    private Transform backgroundRoot;
    private Transform[] tiles;
    private PlayerController player;
    private Camera mainCamera;
    private Vector3 cameraCenter;
    private Vector2 smoothOffset;
    private Vector2 offsetVelocity;
    private Vector2 tileSize;

    public void Initialize(PlayerController targetPlayer, Camera camera)
    {
        player = targetPlayer;
        mainCamera = camera;
        cameraCenter = camera.transform.position + Vector3.forward * 10f;
        Sprite background = LoadBackgroundSprite();
        if (background == null) return;
        tileSize = background.bounds.size;
        if (tileSize.x <= 0f || tileSize.y <= 0f) return;
        backgroundRoot = new GameObject("ParallaxGrid").transform;
        backgroundRoot.SetParent(transform, false);
        tiles = new Transform[9];
        int index = 0;
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                GameObject tile = new GameObject("Grid_" + index);
                tile.transform.SetParent(backgroundRoot, false);
                tile.transform.localPosition = new Vector3(x * tileSize.x, y * tileSize.y, 0f);
                SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = background;
                renderer.sortingOrder = -10;
                tiles[index++] = tile.transform;
            }
        }
        backgroundRoot.position = cameraCenter;
    }

    private void Update()
    {
        if (backgroundRoot == null || mainCamera == null || player == null) return;
        Vector2 targetOffset = -player.MovementVelocity * parallaxStrength;
        targetOffset.x = Mathf.Clamp(targetOffset.x, -1.1f, 1.1f);
        targetOffset.y = Mathf.Clamp(targetOffset.y, -0.8f, 0.8f);
        smoothOffset = Vector2.SmoothDamp(smoothOffset, targetOffset, ref offsetVelocity, 0.28f);
        backgroundRoot.position = cameraCenter + new Vector3(smoothOffset.x, smoothOffset.y, 0f);
    }

    private Sprite LoadBackgroundSprite()
    {
        if (backgroundLoadAttempted) return cachedBackground;
        backgroundLoadAttempted = true;
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprite/Background");
        if (sprites.Length > 0)
        {
            cachedBackground = sprites[0];
            return cachedBackground;
        }
        Texture2D[] textures = Resources.LoadAll<Texture2D>("Sprite/Background");
        if (textures.Length == 0) return cachedBackground;
        Texture2D texture = textures[0];
        cachedBackground = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        return cachedBackground;
    }
}
