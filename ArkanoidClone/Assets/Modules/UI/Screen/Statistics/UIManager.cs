using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Экраны и Сообщения")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject levelTransitionScreen;
    [SerializeField] private TextMeshProUGUI levelTransitionText;

    // --- НОВОЕ ПОЛЕ ---
    [SerializeField] private GameObject victoryMessage; // Текст или Панель "VICTORY"
    // ------------------

    void Start()
    {
        gameOverScreen.SetActive(false);
        levelTransitionScreen.SetActive(false);

        // Прячем победу при старте
        if (victoryMessage != null) victoryMessage.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"SCORE: {score}";
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null) livesText.text = $"LIVES: {lives}";
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null) levelText.text = $"LEVEL: {level}";
    }

    public void ShowGameOver(bool show)
    {
        gameOverScreen.SetActive(true);
    }

    // --- НОВЫЙ МЕТОД ---
    public void ShowVictory(bool show)
    {
        if (victoryMessage != null) victoryMessage.SetActive(show);
    }
    // -------------------

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