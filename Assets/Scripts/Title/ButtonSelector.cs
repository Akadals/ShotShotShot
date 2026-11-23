using UnityEngine;

public class ButtonSelector : MonoBehaviour
{
    [SerializeField] private RectTransform m_selector;
    [SerializeField] private float m_space;
    private IButton[] m_buttons;
    private int m_selectedIndex;

    private void Start()
    {
        m_buttons = GetComponentsInChildren<IButton>();
        if (m_selector == null || m_buttons.Length == 0)
        {
            enabled = false;
            return;
        }

        m_selector.gameObject.SetActive(true);
        UpdateSelector(m_buttons[0]);
    }

    private void Update()
    {
        for (int i = 0; i < m_buttons.Length; i++)
        {
            if (m_buttons[i] == null || !m_buttons[i].IsHighlighted) continue;
            if (m_selectedIndex != i)
            {
                m_selectedIndex = i;
                AudioManager.Instance?.PlayUi("ButtonSelect");
            }

            break;
        }

        UpdateSelector(m_buttons[m_selectedIndex]);
    }

    private void UpdateSelector(IButton _button)
    {
        Vector2 position = _button.SelectorPosition;
        m_selector.anchoredPosition = new Vector2(position.x - m_space, position.y);
    }
}
