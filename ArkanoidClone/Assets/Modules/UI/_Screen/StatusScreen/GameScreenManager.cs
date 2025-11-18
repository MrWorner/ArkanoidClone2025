// GameScreenManager.cs (Переименованный UIManager)

using UnityEngine;
using TMPro;

/// <summary>
/// Управляет активацией/скрытием больших экранов состояния игры (Game Over, Victory, Level Transition).
/// </summary>
public class GameScreenManager : MonoBehaviour
{
    // Оставлены только ссылки на игровые экраны
    [Header("Views")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject levelTransitionScreen;
    [SerializeField] private TextMeshProUGUI levelTransitionText;

    // --- Методы для экранов ---

    public void ShowGameOver(bool show)
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(show);
        }
    }

    public void ShowVictory(bool show)
    {
        if (victoryScreen != null)
        {
            victoryScreen.SetActive(show);
        }
    }

    public void ShowLevelTransition(string text)
    {
        if (levelTransitionText != null)
        {
            levelTransitionText.text = text;
        }
        if (levelTransitionScreen != null)
        {
            levelTransitionScreen.SetActive(true);
        }
    }

    public void HideLevelTransition()
    {
        if (levelTransitionScreen != null)
        {
            levelTransitionScreen.SetActive(false);
        }
    }

    // NOTE: Методы UpdateScore, UpdateLives, UpdateLevel удалены!
}