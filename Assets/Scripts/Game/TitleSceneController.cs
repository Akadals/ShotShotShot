using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] private IButton m_startButton;
    [SerializeField] private IButton m_optionButton;
    [SerializeField] private IButton m_quitButton;
    [SerializeField] private AudioClip m_clickClip;
    [SerializeField] private AudioMixer m_audioMixer;
    [SerializeField] private bool m_useInspectorButtonEvents;
    private AudioSource audioSource;
    private AudioClip clickClip;
    private AudioMixer mixer;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        mixer = m_audioMixer != null ? m_audioMixer : Resources.Load<AudioMixer>("Mixer");
        if (mixer != null)
        {
            AudioMixerGroup[] groups = mixer.FindMatchingGroups("UI");
            if (groups.Length > 0) audioSource.outputAudioMixerGroup = groups[0];
        }
        clickClip = m_clickClip != null ? m_clickClip : Resources.Load<AudioClip>("kenney_interface-sounds/Audio/click_001");
        if (!m_useInspectorButtonEvents)
        {
            Bind(m_startButton != null ? m_startButton : FindButton("Start"), StartGame);
            Bind(m_optionButton != null ? m_optionButton : FindButton("Option"), OpenSettings);
            Bind(m_quitButton != null ? m_quitButton : FindButton("Quit"), QuitGame);
        }
    }

    private IButton FindButton(string _objectName)
    {
        GameObject target = GameObject.Find(_objectName);
        return target != null ? target.GetComponent<IButton>() : null;
    }

    private void Bind(IButton _button, Action _action)
    {
        if (_button == null) return;
        Transform target = _button.transform;
        if (_button.Events == null) _button.Events = new UnityEvent();
        _button.Events.AddListener(() => StartCoroutine(ClickFeedback(target, _action)));
    }

    private IEnumerator ClickFeedback(Transform target, Action action)
    {
        if (audioSource != null && clickClip != null) audioSource.PlayOneShot(clickClip, 0.8f);
        Vector3 originalScale = target.localScale;
        target.localScale = originalScale * 0.92f;
        yield return new WaitForSecondsRealtime(0.06f);
        target.localScale = originalScale;
        yield return new WaitForSecondsRealtime(0.04f);
        action?.Invoke();
    }

    public void StartGame()
    {
        PopupManager.Instance?.Close();
        SceneTransition.Instance?.LoadScene("sGame");
    }

    public void OpenSettings()
    {
        PopupManager.Instance?.OpenSettings();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
