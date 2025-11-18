using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    // --- Singleton ---
    public static GameManager Instance { get; private set; }

    [Header("Настройки Игры")]
    [SerializeField] private int startLives = 3;
    [SerializeField] private bool ignoreBottomWall = false;

    [Header("Ссылки")]
    [SerializeField] private BallPool ballPool;
    [SerializeField] private LevelManager levelManager;
    // СТАРТ РЕФАКТОРИНГА: Новые ссылки для разделения ответственности
    [SerializeField] private GameHUDView hudView;
    [SerializeField] private GameScreenManager screenManager;
    // КОНЕЦ РЕФАКТОРИНГА
    [SerializeField] private PowerUpPool powerUpPool;

    [Header("Бонусы")]
    [SerializeField] private int startBricksForPowerUp = 10;
    [SerializeField] private int powerUpStepIncrement = 5;

    // --- Свойства ---
    public int CurrentLives { get; private set; }
    public int CurrentScore { get; private set; }

    // --- Внутренние поля ---
    private int _currentLevel = 1;
    private int _activeBrickCount;
    private int _bricksDestroyedCounter;
    private int _currentPowerUpThreshold;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Обновленная проверка ссылок
        if (hudView == null || screenManager == null || levelManager == null || ballPool == null || powerUpPool == null)
        {
            Debug.LogError("GameManager: Отсутствуют ссылки на основные компоненты (HUD, ScreenManager, LevelManager, BallPool или PowerUpPool)!");
            return;
        }

        Brick.OnAnyBrickDestroyed += HandleBrickDestroyed;
        PowerUp.OnPowerUpPickedUp += HandlePowerUpPickup; 

        StartNewGame();
    }

    // ========================================================================
    // --- ИНИЦИАЛИЗАЦИЯ И УПРАВЛЕНИЕ СЧЕТОМ/ЖИЗНЯМИ ---
    // ========================================================================

    void StartNewGame()
    {
        // (Код старта без изменений)
        CurrentLives = startLives;
        CurrentScore = 0;

        if (GameInstance.Instance != null) _currentLevel = GameInstance.Instance.SelectedLevelIndex;
        else _currentLevel = 1;

        // СТАРТ РЕФАКТОРИНГА: Вызов методов HUD
        if (hudView != null) hudView.UpdateLives(CurrentLives);
        if (hudView != null) hudView.UpdateScore(CurrentScore);
        if (hudView != null) hudView.UpdateLevel(_currentLevel);
        // КОНЕЦ РЕФАКТОРИНГА

        ResetPowerUpLogic();
        StartCoroutine(LoadLevelRoutine(_currentLevel, false));
    }

    public void SetBrickCount(int count)
    {
        _activeBrickCount = count;
        if (_activeBrickCount <= 0)
        {
            StartCoroutine(VictorySequence());
        }
    }

    public void AddScore(int amount)
    {
        CurrentScore += amount;
        // СТАРТ РЕФАКТОРИНГА: Вызов методов HUD
        if (hudView != null) hudView.UpdateScore(CurrentScore);
        // КОНЕЦ РЕФАКТОРИНГА
    }

    private void LoseLife()
    {
        if (ignoreBottomWall) { RespawnMainBall(); return; }

        CurrentLives--;
        // СТАРТ РЕФАКТОРИНГА: Вызов методов HUD
        if (hudView != null) hudView.UpdateLives(CurrentLives);
        // КОНЕЦ РЕФАКТОРИНГА

        if (CurrentLives <= 0)
        {
            StartCoroutine(GameOverSequence());
        }
        else
        {
            SoundManager.Instance.PlayOneShot(SoundType.LifeLost);
            RespawnMainBall();
        }
    }

    // ========================================================================
    // --- ЛОГИКА МЯЧА И БОНУСОВ ---
    // ========================================================================

    public void ActivateTripleBall()
    {
        // (Ваш код активации тройного мяча...)
        List<BallController> activeBalls = ballPool.GetActiveBalls();
        if (activeBalls.Count == 0) return;
        var ballsToClone = new List<BallController>(activeBalls);
        foreach (var sourceBall in ballsToClone)
        {
            if (sourceBall == null || !sourceBall.gameObject.activeSelf) continue;
            Vector2 currentVelocity = sourceBall.GetComponent<Rigidbody2D>().velocity;
            float speed = currentVelocity.magnitude;
            if (speed < 5f) speed = 7f;
            Vector2 direction = currentVelocity.normalized;
            Vector3 position = sourceBall.transform.position;
            Vector2 dir1 = (Quaternion.Euler(0, 0, -20) * direction) * speed;
            Vector2 dir2 = (Quaternion.Euler(0, 0, 20) * direction) * speed;
            ballPool.GetBall().SpawnAsClone(position, dir1);
            ballPool.GetBall().SpawnAsClone(position, dir2);
        }
    }

    public void HandleBallLost(BallController ball)
    {
        SoundManager.Instance.PlayOneShot(SoundType.BallLost);
        ballPool.ReturnBall(ball);
        int remainingBalls = ballPool.GetActiveBalls().Count;
        if (remainingBalls <= 0) LoseLife();
    }

    private void HandleBrickDestroyed(Vector3 brickPos)
    {
        _activeBrickCount--;
        Debug.Log($"Осталось кирпичей: {_activeBrickCount}");

        // --- ВОТ ЭТОГО НЕ ХВАТАЛО ---
        // Проверяем, не закончились ли кирпичи прямо сейчас
        if (_activeBrickCount <= 0)
        {
            StartCoroutine(VictorySequence());
            return; // Выходим, чтобы не спавнить бонус, если уровень уже пройден
        }
        // -----------------------------

        _bricksDestroyedCounter++;
        if (_bricksDestroyedCounter >= _currentPowerUpThreshold)
        {
            if (powerUpPool != null)
            {
                powerUpPool.GetPowerUp(brickPos);
            }
            _bricksDestroyedCounter = 0;
            _currentPowerUpThreshold += powerUpStepIncrement;
        }
    }

    private void ResetPowerUpLogic()
    {
        _bricksDestroyedCounter = 0;
        _currentPowerUpThreshold = startBricksForPowerUp;
    }

    private void RespawnMainBall()
    {
        BallController newBall = ballPool.GetBall();
        newBall.ResetToPaddle();
    }

    // ========================================================================
    // --- ПОСЛЕДОВАТЕЛЬНОСТИ И ЗАГРУЗКА УРОВНЕЙ ---
    // ========================================================================

    private IEnumerator GameOverSequence()
    {
        Time.timeScale = 0f;
        SoundManager.Instance.PlayOneShot(SoundType.GameOver);
        MusicManager.Instance.PlayGameOverMusic();

        // СТАРТ РЕФАКТОРИНГА: Вызов методов ScreenManager
        if (screenManager != null) screenManager.ShowGameOver(true);
        // КОНЕЦ РЕФАКТОРИНГА

        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadNextScene(GameScene.MainMenu);
    }

    private IEnumerator VictorySequence()
    {
        Debug.Log("VICTORY!");
        SoundManager.Instance.PlayOneShot(SoundType.LevelComplete);
        ballPool.ReturnAllBalls();
        powerUpPool.ReturnAllActive();

        // СТАРТ РЕФАКТОРИНГА: Вызов методов ScreenManager
        if (screenManager != null) screenManager.ShowVictory(true);
        // КОНЕЦ РЕФАКТОРИНГА

        yield return new WaitForSeconds(2f);

        // СТАРТ РЕФАКТОРИНГА: Вызов методов ScreenManager
        if (screenManager != null) screenManager.ShowVictory(false);
        // КОНЕЦ РЕФАКТОРИНГА

        _currentLevel++;
        if (GameInstance.Instance != null) GameInstance.Instance.SetLevelData(_currentLevel);

        StartCoroutine(LoadLevelRoutine(_currentLevel, true));
    }

    private IEnumerator LoadLevelRoutine(int level, bool showTransition)
    {
        // СТАРТ РЕФАКТОРИНГА: Вызов методов ScreenManager
        if (showTransition && screenManager != null) screenManager.ShowLevelTransition($"Level {level}");
        // КОНЕЦ РЕФАКТОРИНГА

        if (GameInstance.Instance != null)
        {
            int levelSeed = GameInstance.Instance.CurrentLevelSeed;
            if (levelManager != null) levelManager.GenerateLevelBySeed(levelSeed);
        }
        else
        {
            if (levelManager != null) levelManager.BuildChaosLevel();
        }

        // СТАРТ РЕФАКТОРИНГА: Вызов методов HUD
        if (hudView != null) hudView.UpdateLevel(_currentLevel);
        // КОНЕЦ РЕФАКТОРИНГА

        ResetPowerUpLogic();
        RespawnMainBall();
        MusicManager.Instance.PlayGameplayMusic();

        if (showTransition)
        {
            yield return new WaitForSeconds(2f);
            // СТАРТ РЕФАКТОРИНГА: Вызов методов ScreenManager
            if (screenManager != null) screenManager.HideLevelTransition();
            // КОНЕЦ РЕФАКТОРИНГА
        }
    }

    private void HandlePowerUpPickup(int bonusPoints)
    {
        // 1. Звук
        SoundManager.Instance.PlayOneShot(SoundType.PowerUpPickup);

        // 2. Очки (и обновление HUD)
        AddScore(bonusPoints);

        // 3. Логика тройного мяча
        ActivateTripleBall();
    }

    private void OnDestroy()
    {
        Brick.OnAnyBrickDestroyed -= HandleBrickDestroyed;
        PowerUp.OnPowerUpPickedUp -= HandlePowerUpPickup;
        // На всякий случай возвращаем время при уничтожении
        Time.timeScale = 1f;
    }
}