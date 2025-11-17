using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;

    // --- НОВОЕ ПОЛЕ ---
    [SerializeField] private TextMeshProUGUI levelText;
    // ------------------

    [Header("Экраны")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject levelTransitionScreen;
    [SerializeField] private TextMeshProUGUI levelTransitionText;

    void Start()
    {
        gameOverScreen.SetActive(false);
        levelTransitionScreen.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"SCORE: {score}";
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null) livesText.text = $"LIVES: {lives}";
    }

    // --- НОВЫЙ МЕТОД ---
    public void UpdateLevel(int level)
    {
        if (levelText != null) levelText.text = $"LEVEL: {level}";
    }
    // -------------------

    public void ShowGameOver(bool show)
    {
        gameOverScreen.SetActive(true);
    }

    public void ShowLevelTransition(string text)
    {
        levelTransitionText.text = text;
        levelTransitionScreen.SetActive(true);
    }

    public void HideLevelTransition()
    {
        levelTransitionScreen.SetActive(false);
    }
}