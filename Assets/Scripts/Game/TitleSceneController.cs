using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class TitleSceneController : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip clickClip;
    private AudioMixer mixer;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        mixer = Resources.Load<AudioMixer>("Mixer");
        if (mixer != null)
        {
            AudioMixerGroup[] groups = mixer.FindMatchingGroups("UI");
            if (groups.Length > 0) audioSource.outputAudioMixerGroup = groups[0];
        }
        clickClip = Resources.Load<AudioClip>("kenney_interface-sounds/Audio/click_001");
        Bind("Start", StartGame);
        Bind("Option", OpenSettings);
        Bind("Quit", QuitGame);
    }

    private void Bind(string objectName, Action action)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null) return;
        IButton button = target.GetComponent<IButton>();
        if (button == null) return;
        if (button.Events == null) button.Events = new UnityEvent();
        button.Events.AddListener(() => StartCoroutine(ClickFeedback(target.transform, action)));
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

    private void StartGame()
    {
        PopupManager.Instance?.Close();
        SceneTransition.Instance?.LoadScene("sGame");
    }

    private void OpenSettings()
    {
        PopupManager.Instance?.OpenSettings();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
