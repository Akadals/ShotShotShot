using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PopupManager : Shared<PopupManager>
{
    private static readonly string[] MixerParameters = { "Master", "BGM", "VFX", "UI" };
    private AudioMixer mixer;
    private GameObject popupRoot;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        mixer = Resources.Load<AudioMixer>("Mixer");
        GameObject prefab = Resources.Load<GameObject>("Prefabs/UI/SettingsPopup");
        if (prefab == null)
        {
            Debug.LogError("SettingsPopup prefab was not found in Resources/Prefabs/UI.");
            return;
        }

        GameObject popupCanvas = Instantiate(prefab, transform);
        popupRoot = popupCanvas.transform.GetChild(0).gameObject;
        popupRoot.SetActive(false);

        Transform panel = popupRoot.transform.Find("SettingsPanel");
        panel.GetComponentInChildren<Button>(true).onClick.AddListener(Close);
        for (int i = 0; i < MixerParameters.Length; i++)
        {
            string parameter = MixerParameters[i];
            Slider slider = panel.Find(parameter + "Slider").GetComponent<Slider>();
            TextMeshProUGUI valueText = panel.Find(parameter + "Value").GetComponent<TextMeshProUGUI>();
            SetupSlider(slider, valueText, parameter);
        }
    }

    public void OpenSettings()
    {
        if (popupRoot != null) popupRoot.SetActive(true);
    }

    public void Close()
    {
        if (popupRoot != null) popupRoot.SetActive(false);
    }

    private void SetupSlider(Slider slider, TextMeshProUGUI valueText, string parameter)
    {
        string key = "Audio_" + parameter;
        float value = PlayerPrefs.GetFloat(key, 1f);
        if (!PlayerPrefs.HasKey(key) && mixer != null && mixer.GetFloat(parameter, out float decibels))
            value = decibels <= -79f ? 0f : Mathf.Pow(10f, decibels / 20f);

        slider.SetValueWithoutNotify(Mathf.Clamp01(value));
        valueText.SetText("{0:0}%", Mathf.RoundToInt(slider.value * 100f));
        slider.onValueChanged.AddListener(volume => SetVolume(parameter, key, volume, valueText));
    }

    private void SetVolume(string parameter, string key, float value, TextMeshProUGUI valueText)
    {
        valueText.SetText("{0:0}%", Mathf.RoundToInt(value * 100f));
        if (mixer != null) mixer.SetFloat(parameter, value <= 0.001f ? -80f : Mathf.Lerp(-40f, 0f, value));
        PlayerPrefs.SetFloat(key, value);
    }
}
