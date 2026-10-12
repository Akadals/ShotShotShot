using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI hullText;
    [SerializeField] private Image[] healthSegments;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    private Action retryAction;
    private Action titleAction;
    private int displayedScore = int.MinValue;
    private int displayedWave = int.MinValue;
    private int displayedElapsedSecond = int.MinValue;
    private int displayedHp = int.MinValue;
    private int displayedMaxHp = int.MinValue;

    public void Initialize(Action onRetry, Action onTitle)
    {
        retryAction = onRetry;
        titleAction = onTitle;
        displayedScore = int.MinValue;
        displayedWave = int.MinValue;
        displayedElapsedSecond = int.MinValue;
        displayedHp = int.MinValue;
        displayedMaxHp = int.MinValue;

        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(Retry);
            retryButton.onClick.AddListener(Retry);
        }
        if (titleButton != null)
        {
            titleButton.onClick.RemoveListener(ReturnToTitle);
            titleButton.onClick.AddListener(ReturnToTitle);
        }
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void SetStats(int score, int wave, float elapsed, int hp, int maxHp)
    {
        if (score != displayedScore)
        {
            if (scoreText != null) scoreText.SetText("SCORE  {0:000000}", score);
            displayedScore = score;
        }
        if (wave != displayedWave)
        {
            if (waveText != null) waveText.SetText("WAVE  {0:00}", wave);
            displayedWave = wave;
        }

        int elapsedSecond = Mathf.FloorToInt(elapsed);
        if (elapsedSecond != displayedElapsedSecond)
        {
            if (timeText != null) timeText.SetText("{0:00}:{1:00}", elapsedSecond / 60, elapsedSecond % 60);
            displayedElapsedSecond = elapsedSecond;
        }

        if (hp == displayedHp && maxHp == displayedMaxHp) return;
        if (hullText != null) hullText.SetText("HULL  {0:0}/{1:0}", hp, maxHp);
        displayedHp = hp;
        displayedMaxHp = maxHp;

        if (healthSegments == null) return;
        Color readyColor = new Color(0.25f, 0.9f, 1f, 1f);
        Color criticalColor = new Color(1f, 0.24f, 0.32f, 1f);
        Color emptyColor = new Color(0.09f, 0.17f, 0.24f, 1f);
        for (int i = 0; i < healthSegments.Length; i++)
        {
            if (healthSegments[i] == null) continue;
            healthSegments[i].color = i < hp ? (hp == 1 ? criticalColor : readyColor) : emptyColor;
        }
    }

    public void ShowGameOver(int score, int bestScore)
    {
        if (finalText != null) finalText.SetText("SCORE  {0:000000}      BEST  {1:000000}", score, bestScore);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private void Retry()
    {
        retryAction?.Invoke();
    }

    private void ReturnToTitle()
    {
        titleAction?.Invoke();
    }
}
