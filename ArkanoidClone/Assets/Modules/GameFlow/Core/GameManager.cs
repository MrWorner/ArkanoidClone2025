using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Настройки Игры")]
    [SerializeField] private int startLives = 3;
    [SerializeField] private bool ignoreBottomWall = false;

    [Header("Ссылки")]
    [SerializeField] private BallPool ballPool;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PowerUpPool powerUpPool;

    [Header("Бонусы")]
    [SerializeField] private int startBricksForPowerUp = 10;
    [SerializeField] private int powerUpStepIncrement = 5;

    public int CurrentLives { get; private set; }
    public int CurrentScore { get; private set; }

    private int _currentLevel = 1;
    private int _activeBrickCount;
    private int _bricksDestroyedCounter;
    private int _currentPowerUpThreshold;

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

        uiManager.UpdateLives(CurrentLives);
        uiManager.UpdateScore(CurrentScore);
        uiManager.UpdateLevel(_currentLevel);

        ResetPowerUpLogic();

        // Первый запуск - сразу грузим уровень (без победы)
        StartCoroutine(LoadLevelRoutine(_currentLevel, false));
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

    // --- ИСПРАВЛЕНИЕ #1: Гарантия скорости ---
    public void ActivateTripleBall()
    {
        List<BallController> activeBalls = ballPool.GetActiveBalls();
        if (activeBalls.Count == 0) return;

        var ballsToClone = new List<BallController>(activeBalls);
        foreach (var sourceBall in ballsToClone)
        {
            if (sourceBall == null || !sourceBall.gameObject.activeSelf) continue;

            // 1. Берем текущую скорость
            Vector2 currentVelocity = sourceBall.GetComponent<Rigidbody2D>().velocity;

            // 2. Если мяч вдруг замедлился, берем "стандартную" скорость (7f)
            // Это гарантирует, что клоны вылетят бодро.
            float speed = currentVelocity.magnitude;
            if (speed < 5f) speed = 7f; // Минимальный порог

            Vector2 direction = currentVelocity.normalized;
            Vector3 position = sourceBall.transform.position;

            // 3. Создаем векторы, умножая НАПРАВЛЕНИЕ на СКОРОСТЬ
            Vector2 dir1 = (Quaternion.Euler(0, 0, -20) * direction) * speed;
            Vector2 dir2 = (Quaternion.Euler(0, 0, 20) * direction) * speed;

            ballPool.GetBall().SpawnAsClone(position, dir1);
            ballPool.GetBall().SpawnAsClone(position, dir2);
        }
    }
    // ----------------------------------------

    public void HandleBallLost(BallController ball)
    {
        SoundManager.Instance.PlayOneShot(SoundType.BallLost);
        ballPool.ReturnBall(ball);
        int remainingBalls = ballPool.GetActiveBalls().Count;
        if (remainingBalls <= 0) LoseLife();
    }

    // --- INTERNAL LOGIC ---

    private void HandleBrickDestroyed(Vector3 brickPos)
    {
        _activeBrickCount--;

        // --- ИСПРАВЛЕНИЕ #2: Победная последовательность ---
        if (_activeBrickCount <= 0)
        {
            // Запускаем победную корутину вместо мгновенной загрузки
            StartCoroutine(VictorySequence());
            return;
        }
        // ------------------------------------------------

        _bricksDestroyedCounter++;
        if (_bricksDestroyedCounter >= _currentPowerUpThreshold)
        {
            powerUpPool.GetPowerUp(brickPos);
            _bricksDestroyedCounter = 0;
            _currentPowerUpThreshold += powerUpStepIncrement;
        }
    }

    private void ResetPowerUpLogic()
    {
        _bricksDestroyedCounter = 0;
        _currentPowerUpThreshold = startBricksForPowerUp;
    }

    private void LoseLife()
    {
        if (ignoreBottomWall) { RespawnMainBall(); return; }

        SoundManager.Instance.PlayOneShot(SoundType.LifeLost);
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

    // --- НОВАЯ ПОБЕДНАЯ КОРУТИНА ---
    private IEnumerator VictorySequence()
    {
        Debug.Log("VICTORY!");

        // 1. Сразу убираем все мячи и бонусы
        ballPool.ReturnAllBalls();
        powerUpPool.ReturnAllActive();

        // 2. Показываем надпись VICTORY
        // (Старые кирпичи всё еще на экране, как вы просили!)
        if (uiManager != null)
        {
            uiManager.ShowVictory(true);
        }

        // 3. Ждем 2 секунды, пока игрок радуется
        yield return new WaitForSeconds(2f);

        // 4. Скрываем Victory
        if (uiManager != null)
        {
            uiManager.ShowVictory(false);
        }

        // 5. Повышаем уровень
        _currentLevel++;
        if (uiManager != null) uiManager.UpdateLevel(_currentLevel);

        // 6. Запускаем стандартную загрузку нового уровня
        StartCoroutine(LoadLevelRoutine(_currentLevel, true));
    }

    private IEnumerator LoadLevelRoutine(int level, bool showTransition)
    {
        if (showTransition && uiManager != null)
        {
            uiManager.ShowLevelTransition($"Level {level}");
        }

        if (GameInstance.Instance != null)
        {
            int levelSeed = GameInstance.Instance.CurrentLevelSeed;

            if (levelManager != null)
            {
                levelManager.GenerateLevelBySeed(levelSeed);
            }
        }
        else
        {
            if (levelManager != null)
            {
                levelManager.BuildChaosLevel();
            }
        }

        // Сброс бонусов
        ResetPowerUpLogic();

        // 3. Спавним мяч (он ждет на ракетке под черным экраном)
        RespawnMainBall();

        // 4. Ждем 2 секунды
        if (showTransition)
        {
            yield return new WaitForSeconds(2f);
            if (uiManager != null) uiManager.HideLevelTransition();
        }
    }

    private void OnDestroy()
    {
        Brick.OnAnyBrickDestroyed -= HandleBrickDestroyed;
    }
}