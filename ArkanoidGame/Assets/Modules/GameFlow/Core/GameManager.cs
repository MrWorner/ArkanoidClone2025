using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Настройки Игры")]
    [SerializeField] private int startLives = 3;

    [Header("Для Разработчика")]
    [SerializeField] private bool ignoreBottomWall = false;

    [Header("Ссылки")]
    [SerializeField] private BallPool ballPool;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PowerUpPool powerUpPool;

    [Header("Бонусы (Баланс)")]
    [Tooltip("Сколько кирпичей нужно для ПЕРВОГО бонуса на уровне")]
    [SerializeField] private int startBricksForPowerUp = 10;

    [Tooltip("На сколько увеличивать требование после каждого бонуса")]
    [SerializeField] private int powerUpStepIncrement = 5;

    // --- Состояние Игры ---
    public int CurrentLives { get; private set; }
    public int CurrentScore { get; private set; }

    private int _currentLevel = 1;
    private int _activeBrickCount;

    // Логика бонусов
    private int _bricksDestroyedCounter;
    private int _currentPowerUpThreshold; // Динамический порог

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (uiManager == null || levelManager == null || ballPool == null || powerUpPool == null)
        {
            Debug.LogError("GameManager: Не все ссылки назначены!");
            return;
        }

        Brick.OnAnyBrickDestroyed += HandleBrickDestroyed;

        StartNewGame();
    }

    void StartNewGame()
    {
        CurrentLives = startLives;
        CurrentScore = 0;
        _currentLevel = 1;

        // Инициализируем UI
        uiManager.UpdateLives(CurrentLives);
        uiManager.UpdateScore(CurrentScore);
        uiManager.UpdateLevel(_currentLevel);

        // Сбрасываем сложность бонусов перед первым уровнем
        ResetPowerUpLogic();

        StartCoroutine(LoadLevel(_currentLevel));
    }

    // --- PUBLIC METHODS ---

    public void SetBrickCount(int count)
    {
        _activeBrickCount = count;
    }

    public void AddScore(int amount)
    {
        CurrentScore += amount;
        if (uiManager != null) uiManager.UpdateScore(CurrentScore);
    }

    public void ActivateTripleBall()
    {
        List<BallController> activeBalls = ballPool.GetActiveBalls();
        if (activeBalls.Count == 0) return;

        var ballsToClone = new List<BallController>(activeBalls);
        foreach (var sourceBall in ballsToClone)
        {
            if (sourceBall == null || !sourceBall.gameObject.activeSelf) continue;

            Vector2 velocity = sourceBall.GetComponent<Rigidbody2D>().linearVelocity;
            Vector3 position = sourceBall.transform.position;

            Vector2 dir1 = Quaternion.Euler(0, 0, -20) * velocity;
            ballPool.GetBall().SpawnAsClone(position, dir1);

            Vector2 dir2 = Quaternion.Euler(0, 0, 20) * velocity;
            ballPool.GetBall().SpawnAsClone(position, dir2);
        }
    }

    public void HandleBallLost(BallController ball)
    {
        ballPool.ReturnBall(ball);
        int remainingBalls = ballPool.GetActiveBalls().Count;
        if (remainingBalls <= 0) LoseLife();
    }

    // --- INTERNAL LOGIC ---

    private void HandleBrickDestroyed(Vector3 brickPos)
    {
        // 1. Уменьшаем кол-во оставшихся кирпичей
        _activeBrickCount--;

        // --- ИСПРАВЛЕНИЕ #2: Сначала проверяем победу! ---
        if (_activeBrickCount <= 0)
        {
            _currentLevel++;
            if (uiManager != null) uiManager.UpdateLevel(_currentLevel);
            StartCoroutine(LoadLevel(_currentLevel));

            // ВАЖНО: Делаем return, чтобы бонус НЕ выпал на последнем кирпиче
            return;
        }
        // ------------------------------------------------

        // 2. Логика бонусов (если уровень еще не пройден)
        _bricksDestroyedCounter++;

        // --- ИСПРАВЛЕНИЕ #1: Динамический шаг ---
        if (_bricksDestroyedCounter >= _currentPowerUpThreshold)
        {
            SpawnPowerUp(brickPos);

            // Сбрасываем счетчик
            _bricksDestroyedCounter = 0;

            // Увеличиваем сложность (10 -> 15 -> 20...)
            _currentPowerUpThreshold += powerUpStepIncrement;
            Debug.Log($"GameManager: Бонус выпал! Следующий через {_currentPowerUpThreshold} кирпичей.");
        }
        // ----------------------------------------
    }

    private void SpawnPowerUp(Vector3 spawnPos)
    {
        powerUpPool.GetPowerUp(spawnPos);
    }

    private void ResetPowerUpLogic()
    {
        _bricksDestroyedCounter = 0;
        _currentPowerUpThreshold = startBricksForPowerUp; // Сброс к 10
    }

    private void LoseLife()
    {
        if (ignoreBottomWall) { RespawnMainBall(); return; }

        CurrentLives--;
        if (uiManager != null) uiManager.UpdateLives(CurrentLives);

        if (CurrentLives <= 0)
        {
            if (uiManager != null) uiManager.ShowGameOver(true);
            Time.timeScale = 0f;
        }
        else
        {
            RespawnMainBall();
        }
    }

    private void RespawnMainBall()
    {
        BallController newBall = ballPool.GetBall();
        newBall.ResetToPaddle();
    }

    private IEnumerator LoadLevel(int level)
    {
        Debug.Log($"Loading Level {level}...");

        // --- ИСПРАВЛЕНИЕ #3: Принудительно показываем UI здесь ---
        // Это гарантирует, что надпись появится перед любой другой логикой
        if (uiManager != null)
        {
            uiManager.ShowLevelTransition($"Level {level}");
        }
        // ---------------------------------------------------------

        // Чистим поле
        ballPool.ReturnAllBalls();
        powerUpPool.ReturnAllActive(); // Убираем старые бонусы

        // Сбрасываем логику спавна бонусов для нового уровня
        ResetPowerUpLogic();

        // Строим уровень
        if (levelManager != null)
        {
            // Для теста используем хаос, но в реальной игре можно чередовать
            levelManager.BuildChaosLevel();
        }

        RespawnMainBall();

        // Ждем 2 секунды (игрок видит надпись Level X)
        yield return new WaitForSeconds(2f);

        if (uiManager != null) uiManager.HideLevelTransition();
    }

    private void OnDestroy()
    {
        Brick.OnAnyBrickDestroyed -= HandleBrickDestroyed;
    }
}