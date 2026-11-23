using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : Singleton<SceneManager>
{
    [SerializeField] private float m_fadeDuration = 0.35f;
    private CanvasGroup m_canvasGroup;
    private bool m_isTransitioning;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();
        m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
        m_canvasGroup.alpha = 0f;
        m_canvasGroup.blocksRaycasts = false;

        GameObject overlay = new GameObject("Fade", typeof(RectTransform));
        overlay.transform.SetParent(transform, false);
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        overlay.AddComponent<Image>().color = new Color(0.005f, 0.015f, 0.04f, 1f);
    }

    public void LoadScene(string _sceneName)
    {
        if (!m_isTransitioning) StartCoroutine(Transition(_sceneName));
    }

    private IEnumerator Transition(string _sceneName)
    {
        m_isTransitioning = true;
        m_canvasGroup.blocksRaycasts = true;
        yield return Fade(0f, 1f);

        AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_sceneName);
        if (operation == null)
        {
            yield return Fade(1f, 0f);
            m_canvasGroup.blocksRaycasts = false;
            m_isTransitioning = false;
            yield break;
        }

        while (!operation.isDone) yield return null;
        yield return null;
        yield return Fade(1f, 0f);
        m_canvasGroup.blocksRaycasts = false;
        m_isTransitioning = false;
    }

    private IEnumerator Fade(float _from, float _to)
    {
        float elapsed = 0f;
        while (elapsed < m_fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            m_canvasGroup.alpha = Mathf.Lerp(_from, _to, elapsed / m_fadeDuration);
            yield return null;
        }

        m_canvasGroup.alpha = _to;
    }
}
