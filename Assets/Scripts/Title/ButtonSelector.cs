using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ButtonSelector : MonoBehaviour
{
    [SerializeField] private RectTransform selector;
    [SerializeField] private float space;
    [SerializeField] private AudioClip selectClip;
    private IButton[] buttons;
    private AudioSource audioSource;
    private int selected;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        buttons = GetComponentsInChildren<IButton>();
        if (selector == null || buttons.Length == 0)
        {
            enabled = false;
            return;
        }

        selector.gameObject.SetActive(true);
        Vector2 position = buttons[0].SelectorPosition;
        selector.anchoredPosition = new Vector2(position.x - space, position.y);
    }

    private void Update()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null || !buttons[i].IsHighlighted) continue;
            if (selected != i)
            {
                selected = i;
                if (selectClip != null) audioSource.PlayOneShot(selectClip);
            }

            Vector2 position = buttons[i].SelectorPosition;
            selector.anchoredPosition = new Vector2(position.x - space, position.y);
        }
    }
}
