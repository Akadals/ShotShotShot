using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }
    private CanvasGroup canvasGroup;
    private bool isTransitioning;

    public static void EnsureInstance()
    {
        if (Instance != null) return;
        GameObject transition = new GameObject("SceneTransition");
        transition.AddComponent<SceneTransition>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        GameObject overlay = new GameObject("Fade", typeof(RectTransform));
        overlay.transform.SetParent(transform, false);
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = overlay.AddComponent<Image>();
        image.color = new Color(0.005f, 0.015f, 0.04f, 1f);
    }

    public void LoadScene(string sceneName)
    {
        if (!isTransitioning) StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
    {
        isTransitioning = true;
        canvasGroup.blocksRaycasts = true;
        yield return Fade(0f, 1f);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            yield return Fade(1f, 0f);
            canvasGroup.blocksRaycasts = false;
            isTransitioning = false;
            yield break;
        }
        while (!operation.isDone) yield return null;
        yield return null;
        yield return Fade(1f, 0f);
        canvasGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        const float duration = 0.35f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
