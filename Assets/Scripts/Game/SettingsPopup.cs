using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [Serializable]
    private struct VolumeControl
    {
        [SerializeField] private Slider m_slider;
        [SerializeField] private TMP_Text m_valueText;

        public void Bind(VolumeChannel _channel)
        {
            Slider slider = m_slider;
            TMP_Text valueText = m_valueText;
            slider.SetValueWithoutNotify(AudioManager.Instance.GetVolume(_channel));
            valueText.SetText("{0:0}%", Mathf.RoundToInt(slider.value * 100f));
            slider.onValueChanged.AddListener(_value =>
            {
                AudioManager.Instance.SetVolume(_channel, _value);
                valueText.SetText("{0:0}%", Mathf.RoundToInt(_value * 100f));
            });
        }
    }

    [SerializeField] private GameObject m_overlay;
    [SerializeField] private GameObject m_titleButton;
    [SerializeField] private TMP_Text m_closeLabel;
    [SerializeField] private RectTransform m_closeButton;
    [SerializeField] private VolumeControl m_master;
    [SerializeField] private VolumeControl m_bgm;
    [SerializeField] private VolumeControl m_vfx;
    [SerializeField] private VolumeControl m_ui;

    private Action m_onResume;
    private Action m_onTitle;

    private void Awake()
    {
        m_overlay.SetActive(false);
        m_master.Bind(VolumeChannel.MASTER);
        m_bgm.Bind(VolumeChannel.BGM);
        m_vfx.Bind(VolumeChannel.VFX);
        m_ui.Bind(VolumeChannel.UI);
    }

    public void Show(Action _onResume, Action _onTitle)
    {
        m_onResume = _onResume;
        m_onTitle = _onTitle;
        bool isPauseMenu = _onTitle != null;
        m_closeLabel.text = isPauseMenu ? "RESUME" : "CLOSE";
        m_closeButton.anchoredPosition = new Vector2(isPauseMenu ? -135f : 0f, m_closeButton.anchoredPosition.y);
        m_titleButton.SetActive(isPauseMenu);
        m_overlay.SetActive(true);
    }

    public void Close()
    {
        if (!m_overlay.activeSelf) return;
        Action resume = m_onResume;
        Hide();
        resume?.Invoke();
    }

    public void ReturnToTitle()
    {
        Action returnToTitle = m_onTitle;
        Hide();
        returnToTitle?.Invoke();
    }

    private void Hide()
    {
        m_overlay.SetActive(false);
        m_onResume = null;
        m_onTitle = null;
        AudioManager.Instance?.SaveVolumeSettings();
    }
}
