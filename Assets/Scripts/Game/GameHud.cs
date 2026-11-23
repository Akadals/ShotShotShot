using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_scoreText;
    [SerializeField] private TextMeshProUGUI m_waveText;
    [SerializeField] private TextMeshProUGUI m_timeText;
    [SerializeField] private TextMeshProUGUI m_hullText;
    [SerializeField] private Image[] m_healthSegments;
    [SerializeField] private GameObject m_gameOverPanel;
    [SerializeField] private TextMeshProUGUI m_finalText;
    [SerializeField] private Button m_retryButton;
    [SerializeField] private Button m_titleButton;

    private Action m_retryAction;
    private Action m_titleAction;
    private int m_displayedScore = int.MinValue;
    private int m_displayedWave = int.MinValue;
    private int m_displayedElapsedSecond = int.MinValue;
    private int m_displayedHealth = int.MinValue;
    private int m_displayedMaximumHealth = int.MinValue;

    public void Initialize(Action _onRetry, Action _onTitle)
    {
        m_retryAction = _onRetry;
        m_titleAction = _onTitle;
        m_displayedScore = int.MinValue;
        m_displayedWave = int.MinValue;
        m_displayedElapsedSecond = int.MinValue;
        m_displayedHealth = int.MinValue;
        m_displayedMaximumHealth = int.MinValue;
        if (m_retryButton != null) m_retryButton.onClick.AddListener(Retry);
        if (m_titleButton != null) m_titleButton.onClick.AddListener(ReturnToTitle);
        if (m_gameOverPanel != null) m_gameOverPanel.SetActive(false);
    }

    public void SetStats(int _score, int _wave, float _elapsed)
    {
        if (_score != m_displayedScore)
        {
            if (m_scoreText != null) m_scoreText.SetText("SCORE  {0:000000}", _score);
            m_displayedScore = _score;
        }

        if (_wave != m_displayedWave)
        {
            if (m_waveText != null) m_waveText.SetText("WAVE  {0:00}", _wave);
            m_displayedWave = _wave;
        }

        int elapsedSecond = Mathf.FloorToInt(_elapsed);
        if (elapsedSecond == m_displayedElapsedSecond) return;
        if (m_timeText != null) m_timeText.SetText("{0:00}:{1:00}", elapsedSecond / 60, elapsedSecond % 60);
        m_displayedElapsedSecond = elapsedSecond;
    }

    public void SetHealth(int _health, int _maximumHealth)
    {
        if (_health == m_displayedHealth && _maximumHealth == m_displayedMaximumHealth) return;
        if (m_hullText != null) m_hullText.SetText("HULL  {0:0}/{1:0}", _health, _maximumHealth);
        m_displayedHealth = _health;
        m_displayedMaximumHealth = _maximumHealth;

        if (m_healthSegments == null) return;
        Color readyColor = new Color(0.25f, 0.9f, 1f, 1f);
        Color criticalColor = new Color(1f, 0.24f, 0.32f, 1f);
        Color emptyColor = new Color(0.09f, 0.17f, 0.24f, 1f);
        int activeSegmentCount = _maximumHealth > 0
            ? Mathf.CeilToInt((float)_health * m_healthSegments.Length / _maximumHealth)
            : 0;
        for (int i = 0; i < m_healthSegments.Length; i++)
        {
            if (m_healthSegments[i] == null) continue;
            m_healthSegments[i].color = i < activeSegmentCount
                ? (_health == 1 ? criticalColor : readyColor)
                : emptyColor;
        }
    }

    public void ShowGameOver(int _score, int _bestScore)
    {
        if (m_finalText != null) m_finalText.SetText("SCORE  {0:000000}      BEST  {1:000000}", _score, _bestScore);
        if (m_gameOverPanel != null) m_gameOverPanel.SetActive(true);
    }

    public void Retry()
    {
        m_retryAction?.Invoke();
    }

    public void ReturnToTitle()
    {
        m_titleAction?.Invoke();
    }
}
