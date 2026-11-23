using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class IButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private UnityEvent m_onClick = new UnityEvent();
    private RectTransform m_rect;
    private bool m_isHighlighted;

    public Vector2 SelectorPosition => m_rect.anchoredPosition;
    public bool IsHighlighted => m_isHighlighted;
    public UnityEvent OnClick => m_onClick;

    private void Awake()
    {
        m_rect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData _eventData)
    {
        m_isHighlighted = true;
    }

    public void OnPointerExit(PointerEventData _eventData)
    {
        m_isHighlighted = false;
    }

    public void OnPointerClick(PointerEventData _eventData)
    {
        AudioManager.Instance?.PlayUi("ButtonClick");
        m_onClick?.Invoke();
    }
}
