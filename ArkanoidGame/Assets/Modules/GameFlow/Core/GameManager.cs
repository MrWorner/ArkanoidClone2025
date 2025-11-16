using UnityEngine;
using System.Collections; // Для корутин (задержек)

public class GameManager : MonoBehaviour
{
    // --- Singleton ---
    public static GameManager Instance { get; private set; }
    // -----------------

    [Header("Настройки Игры")]
    [SerializeField] private int startLives = 3;
    [SerializeField] private int pointsPerBrick = 10;

    [Header("Для Разработчика")]
    [Tooltip("Если true, мяч не будет 'умирать' при падении")]
    [SerializeField] private bool ignoreBottomWall = false;

    [Header("Ссылки (Назначьте вручную)")]
    [SerializeField] private BallController ball;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private UIManager uiManager;

    // --- Состояние Игры ---
    public int CurrentLives { get; private set; }
    public int CurrentScore { get; private set; }
    private int _currentLevel = 1;
    private int _activeBrickCount;

    void Awake()
    {
        // Настройка Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 1. Подписываемся на "сигнал" от кирпичей
        // (Мы уже создали этот event в Brick.cs)
        Brick.OnAnyBrickDestroyed += HandleBrickDestroyed;

        StartNewGame();
    }

    void StartNewGame()
    {
        CurrentLives = startLives;
        CurrentScore = 0;
        _currentLevel = 1;

        // Обновляем UI
        uiManager.UpdateLives(CurrentLives);
        uiManager.UpdateScore(CurrentScore);

        // Строим первый уровень
        StartCoroutine(LoadLevel(_currentLevel));
    }

    /// <summary>
    /// Вызывается, когда LevelManager заканчивает строить
    /// </summary>
    public void SetBrickCount(int count)
    {
        _activeBrickCount = count;
    }

    /// <summary>
    /// Вызывается, когда кирпич посылает 'event'
    /// </summary>
    private void HandleBrickDestroyed()
    {
        // 1. Даем очки (Запрос #3)
        CurrentScore += pointsPerBrick;
        uiManager.UpdateScore(CurrentScore);

        // 2. Проверяем, не последний ли это кирпич
        _activeBrickCount--;
        if (_activeBrickCount <= 0)
        {
            // 3. Победа! (Запрос #2)
            _currentLevel++;
            StartCoroutine(LoadLevel(_currentLevel));
        }
    }

    /// <summary>
    /// Вызывается, когда мяч попадает в 'bottomWall'
    /// </summary>
    public void HandleBallLost()
    {
        // 1. Проверяем опцию разработчика (Запрос #1)
        if (ignoreBottomWall) return;

        CurrentLives--;
        uiManager.UpdateLives(CurrentLives);

        if (CurrentLives <= 0)
        {
            // 2. Game Over (Запрос #1)
            uiManager.ShowGameOver(true);
            Time.timeScale = 0f; // "Замораживаем" игру
        }
        else
        {
            // 3. Возрождаем мяч (Запрос #1)
            ball.ResetMode();
            // (BallController сам запустит корутину задержки)
        }
    }

    /// <summary>
    /// Загружает новый уровень
    /// </summary>
    private IEnumerator LoadLevel(int level)
    {
        // "Прячем" мяч, чтобы он не летал во время стройки
        ball.ResetMode();
        ball.gameObject.SetActive(false);

        // 1. Показываем черный экран (Запрос #2)
        uiManager.ShowLevelTransition($"Level {level}");
        yield return new WaitForSeconds(2f); // Ждем 2 сек

        // 2. Строим
        levelManager.BuildLevel(); // Он сам посчитает кирпичи и вызовет SetBrickCount

        // 3. Убираем черный экран
        uiManager.HideLevelTransition();

        // 4. "Включаем" мяч
        ball.gameObject.SetActive(true);
        ball.ResetMode(); // Снова, чтобы он прилип к ракетке
    }

    // Не забываем отписаться
    private void OnDestroy()
    {
        Brick.OnAnyBrickDestroyed -= HandleBrickDestroyed;
    }
}