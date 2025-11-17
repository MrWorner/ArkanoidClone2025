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

    // --- ЗАМЕНА: Ссылка на пул вместо префаба ---
    [SerializeField] private PowerUpPool powerUpPool;
    // [SerializeField] private PowerUp tripleBallPrefab; // <-- Удалено, теперь это в пуле
    // --------------------------------------------

    [Header("Бонусы")]
    [SerializeField] private int minBricksForPowerUp = 5;
    [SerializeField] private int maxBricksForPowerUp = 10;

    public int CurrentLives { get; private set; }
    public int CurrentScore { get; private set; }

    private int _currentLevel = 1;
    private int _activeBrickCount;
    private int _bricksDestroyedCounter;
    private int _nextPowerUpTarget;

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
        // Проверки (добавили powerUpPool)
        if (uiManager == null || levelManager == null || ballPool == null || powerUpPool == null)
        {
            Debug.LogError("GameManager: Не все ссылки назначены!");
            return;
        }

        Brick.OnAnyBrickDestroyed += HandleBrickDestroyed;
        _nextPowerUpTarget = Random.Range(minBricksForPowerUp, maxBricksForPowerUp);
        StartNewGame();
    }

    void StartNewGame()
    {
        CurrentLives = startLives;
        CurrentScore = 0;
        _currentLevel = 1;

        uiManager.UpdateLives(CurrentLives);
        uiManager.UpdateScore(CurrentScore);
        uiManager.UpdateLevel(_currentLevel);

        StartCoroutine(LoadLevel(_currentLevel));
    }

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
        if (remainingBalls <= 0)
        {
            LoseLife();
        }
    }

    private void HandleBrickDestroyed()
    {
        _activeBrickCount--;
        if (_activeBrickCount <= 0)
        {
            _currentLevel++;
            if (uiManager != null) uiManager.UpdateLevel(_currentLevel);
            StartCoroutine(LoadLevel(_currentLevel));
        }

        _bricksDestroyedCounter++;
        if (_bricksDestroyedCounter >= _nextPowerUpTarget)
        {
            SpawnPowerUp();
            _bricksDestroyedCounter = 0;
            _nextPowerUpTarget = Random.Range(minBricksForPowerUp, maxBricksForPowerUp);
        }
    }

    private void SpawnPowerUp()
    {
        // Теперь вызываем через ПУЛ
        float randomX = Random.Range(-3f, 3f);
        Vector3 spawnPos = new Vector3(randomX, 6f, 0);

        powerUpPool.GetPowerUp(spawnPos); // <-- Вот так просто!
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
        if (uiManager != null) uiManager.ShowLevelTransition($"Level {level}");

        ballPool.ReturnAllBalls();

        // --- ИСПРАВЛЕНИЕ: Быстрая очистка через ПУЛ ---
        powerUpPool.ReturnAllActive(); // Больше никаких FindObjects!
        // ----------------------------------------------

        if (levelManager != null)
        {
            levelManager.BuildChaosLevel();
        }

        RespawnMainBall();

        yield return new WaitForSeconds(2f);

        if (uiManager != null) uiManager.HideLevelTransition();
    }

    private void OnDestroy()
    {
        Brick.OnAnyBrickDestroyed -= HandleBrickDestroyed;
    }
}