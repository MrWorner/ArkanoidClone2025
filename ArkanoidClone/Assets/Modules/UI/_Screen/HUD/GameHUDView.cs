// GameHUDView.cs

using UnityEngine;
using TMPro; // Убедитесь, что используете TextMeshPro

/// <summary>
/// Представление (View) для постоянного отображения игровой статистики (Score, Lives, Level).
/// Наследует MonoBehaviour, может быть прикреплен к общему Canvas.
/// </summary>
public class GameHUDView : MonoBehaviour
{
    [Header("Элементы HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        // Проверка ссылок на текст
        if (scoreText == null || livesText == null || levelText == null)
        {
            Debug.LogError("GameHUDView: Не назначены все ссылки на TextMeshProUGUI в инспекторе!", this);
        }
    }

    /// <summary>
    /// Обновляет отображение текущего счета.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:N0}"; // Форматирование для красивого отображения больших чисел
        }
    }

    /// <summary>
    /// Обновляет отображение текущего количества жизней.
    /// </summary>
    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }

    /// <summary>
    /// Обновляет отображение текущего уровня.
    /// </summary>
    public void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
        }
    }
}