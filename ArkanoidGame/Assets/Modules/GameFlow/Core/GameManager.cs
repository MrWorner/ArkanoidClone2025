using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // --- Singleton ---
    public static GameManager Instance { get; private set; }
    // -----------------

    [Header("Настройки Игры")]
    [SerializeField] private int startLives = 3;

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
        // 1. Даем очки (Запрос #3)
        // CurrentScore += pointsPerBrick; // <-- УДАЛИТЕ ЭТУ СТРОКУ
        // uiManager.UpdateScore(CurrentScore); // <-- УДАЛИТЕ ЭТУ СТРОКУ

        // 2. Проверяем, не последний ли это кирпич
        _activeBrickCount--;
        if (_activeBrickCount <= 0)
        {
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
        Debug.Log($"GameManager: LoadLevel({level}) - Начало...");

        // 1. Сначала показываем экран перехода (чтобы скрыть момент перестройки, если нужно)
        if (uiManager != null)
        {
            uiManager.ShowLevelTransition($"Level {level}");
        }

        // 2. МГНОВЕННО строим новый уровень (это удалит старые кирпичи)
        if (levelManager != null)
        {
            // Если хотите больше случайности при каждом уровне - вызывайте Random
            levelManager.BuildChaosLevel();
            // Или обычный BuildLevel(), если настройки меняются только по кнопке
            // levelManager.BuildLevel();
        }

        // 3. МГНОВЕННО сажаем мяч на ракетку
        if (ball != null)
        {
            ball.gameObject.SetActive(true); // Убедимся, что он виден
            ball.ResetMode(); // Приклеиваем к ракетке
        }

        // 4. Теперь, когда всё готово, ЖДЕМ.
        // Игрок видит надпись "Level 2", а за ней уже стоит новый уровень и мяч на месте.
        yield return new WaitForSeconds(2f);

        // 5. Убираем надпись и начинаем игру
        if (uiManager != null)
        {
            uiManager.HideLevelTransition();
        }

        Debug.Log($"GameManager: Уровень {level} начат.");
    }

    // Не забываем отписаться
    private void OnDestroy()
    {
        Brick.OnAnyBrickDestroyed -= HandleBrickDestroyed;
    }

    public void AddScore(int amount)
    {
        CurrentScore += amount;
        if (uiManager != null)
        {
            uiManager.UpdateScore(CurrentScore);
        }
    }
}