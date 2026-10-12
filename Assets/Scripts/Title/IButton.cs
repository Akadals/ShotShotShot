using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class IButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    private RectTransform rect = null;
    private Vector2 selectorPosition = Vector2.zero;
    private bool isHighlighted = false;
    public Vector2 SelectorPosition => selectorPosition;
    public bool IsHighlighted => isHighlighted;
    public UnityEvent Events;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (rect != null) selectorPosition = rect.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
        => isHighlighted = true;

    public void OnPointerExit(PointerEventData eventData)
        => isHighlighted = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        Events?.Invoke();
    }
}
