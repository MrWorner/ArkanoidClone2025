using UnityEngine;
using System.Collections;

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
        Debug.Log("GameManager: Awake() вызван.");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Debug.Log("GameManager: Start() вызван.");

        // --- НОВЫЕ ПРОВЕРКИ ---
        // Это самая важная часть. Мы проверяем ссылки ДО того, как их использовать.
        if (uiManager == null) { Debug.LogError("GameManager ОШИБКА: 'Ui Manager' НЕ НАЗНАЧЕН в инспекторе!"); }
        if (ball == null) { Debug.LogError("GameManager ОШИБКА: 'Ball' НЕ НАЗНАЧЕН в инспекторе!"); }
        if (levelManager == null) { Debug.LogError("GameManager ОШИБКА: 'Level Manager' НЕ НАЗНАЧЕН в инспекторе!"); }
        // -----------------------

        Brick.OnAnyBrickDestroyed += HandleBrickDestroyed;

        StartNewGame();
    }

    void StartNewGame()
    {
        Debug.Log("GameManager: StartNewGame() вызван.");
        CurrentLives = startLives;
        CurrentScore = 0;
        _currentLevel = 1;

        // Добавляем проверку перед использованием
        if (uiManager != null)
        {
            uiManager.UpdateLives(CurrentLives);
            uiManager.UpdateScore(CurrentScore);
        }
        else
        {
            Debug.LogError("GameManager: Не могу обновить UI, 'uiManager' = null.");
            return; // Прерываем, т.к. игра сломана
        }

        StartCoroutine(LoadLevel(_currentLevel));
    }

    /// <summary>
    /// Вызывается, когда LevelManager заканчивает строить
    /// </summary>
    public void SetBrickCount(int count)
    {
        _activeBrickCount = count;
        Debug.Log($"GameManager: Уровень построен, кирпичей: {count}");
    }

    /// <summary>
    /// Вызывается, когда кирпич посылает 'event'
    /// </summary>
    private void HandleBrickDestroyed()
    {
        // 1. Даем очки
        CurrentScore += pointsPerBrick;
        if (uiManager != null) uiManager.UpdateScore(CurrentScore);

        // 2. Проверяем победу
        _activeBrickCount--;
        if (_activeBrickCount <= 0)
        {
            Debug.Log("GameManager: ПОБЕДА! Загрузка следующего уровня...");
            _currentLevel++;
            StartCoroutine(LoadLevel(_currentLevel));
        }
    }

    /// <summary>
    /// Вызывается, когда мяч попадает в 'bottomWall'
    /// </summary>
    public void HandleBallLost()
    {
        if (ignoreBottomWall) return;

        CurrentLives--;
        Debug.Log($"GameManager: Жизнь потеряна. Осталось: {CurrentLives}");
        if (uiManager != null) uiManager.UpdateLives(CurrentLives);

        if (CurrentLives <= 0)
        {
            Debug.Log("GameManager: GAME OVER.");
            if (uiManager != null) uiManager.ShowGameOver(true);
            Time.timeScale = 0f;
        }
        else
        {
            if (ball != null) ball.ResetMode();
        }
    }

    /// <summary>
    /// Загружает новый уровень
    /// </summary>
    private IEnumerator LoadLevel(int level)
    {
        Debug.Log($"GameManager: LoadLevel({level}) - Начало загрузки...");

        // 1. Проверяем мяч
        if (ball == null)
        {
            Debug.LogError("GameManager: Не могу спрятать 'ball', 'ball' = null.");
            yield break; // Прерываем корутину
        }
        ball.ResetMode();
        ball.gameObject.SetActive(false);

        // 2. Проверяем UI
        if (uiManager == null)
        {
            Debug.LogError("GameManager: Не могу показать 'Level Transition', 'uiManager' = null.");
            yield break; // Прерываем корутину
        }
        uiManager.ShowLevelTransition($"Level {level}");
        Debug.Log("GameManager: Показываю экран 'Level 1'");

        yield return new WaitForSeconds(2f); // Ждем 2 сек

        // 3. Проверяем LevelManager
        if (levelManager == null)
        {
            Debug.LogError("GameManager: Не могу построить уровень, 'levelManager' = null.");
            yield break; // Прерываем корутину
        }
        levelManager.BuildLevel();

        uiManager.HideLevelTransition();

        ball.gameObject.SetActive(true);
        ball.ResetMode(); // ResetMode() ТЕПЕРЬ сам запускает таймер
        Debug.Log($"GameManager: LoadLevel({level}) - Загрузка завершена. Мяч на ракетке.");
    }

    // Не забываем отписаться
    private void OnDestroy()
    {
        Brick.OnAnyBrickDestroyed -= HandleBrickDestroyed;
    }
}