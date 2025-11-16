using UnityEngine;
using TMPro; // Важно для TextMeshPro

public class UIManager : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;

    [Header("Экраны")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject levelTransitionScreen;
    [SerializeField] private TextMeshProUGUI levelTransitionText;

    void Awake()
    {
        // Прячем все экраны при старте
        gameOverScreen.SetActive(false);
        levelTransitionScreen.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        scoreText.text = $"SCORE: {score}";
    }

    public void UpdateLives(int lives)
    {
        livesText.text = $"LIVES: {lives}";
    }

    public void ShowGameOver(bool show)
    {
        gameOverScreen.SetActive(true);
    }

    public void ShowLevelTransition(string text)
    {

        Debug.Log("<color=orange>IT WORKS!</color>", this);
        levelTransitionText.text = text;
        levelTransitionScreen.SetActive(true);
    }

    public void HideLevelTransition()
    {
        Debug.Log("<color=red>IT WORKS!</color>", this);
        levelTransitionScreen.SetActive(false);
    }
}