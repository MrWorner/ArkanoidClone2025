using MiniIT.AUDIO;
using MiniIT.BALL;
using MiniIT.BRICK;
using MiniIT.LEVELS;
using MiniIT.POWERUP;
using MiniIT.SCENELOADER;
using MiniIT.UI;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniIT.CORE
{
    /// <summary>
    /// Manages the main game loop, score, lives, and level progression.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        /// <summary>
        /// Global singleton instance.
        /// </summary>
        public static GameManager Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Current number of player lives.
        /// </summary>
        public int CurrentLives
        {
            get;
            private set;
        }

        /// <summary>
        /// Current player score.
        /// </summary>
        public int CurrentScore
        {
            get;
            private set;
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("SETTINGS")]
        [Header("Game Settings")]
        [SerializeField]
        private int startLives = 3;

        [BoxGroup("SETTINGS")]
        [SerializeField]
        private bool ignoreBottomWall = false;

        [BoxGroup("REFERENCES")]
        [Header("References")]
        [SerializeField, Required]
        private BallPool ballPool = null;

        [BoxGroup("REFERENCES")]
        [SerializeField, Required]
        private LevelManager levelManager = null;

        [BoxGroup("REFERENCES")]
        [SerializeField, Required]
        private GameHUDView hudView = null;

        [BoxGroup("REFERENCES")]
        [SerializeField, Required]
        private GameScreenManager screenManager = null;

        [BoxGroup("REFERENCES")]
        [SerializeField, Required]
        private PowerUpPool powerUpPool = null;

        [BoxGroup("BONUSES")]
        [Header("Bonuses")]
        [SerializeField]
        private int startBricksForPowerUp = 10;

        [BoxGroup("BONUSES")]
        [SerializeField]
        private int powerUpStepIncrement = 5;

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private int currentLevel = 1;
        private int activeBrickCount = 0;
        private int bricksDestroyedCounter = 0;
        private int currentPowerUpThreshold = 0;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// Sets the number of active bricks for the current level.
        /// </summary>
        /// <param name="count">Total bricks count.</param>
        public void SetBrickCount(int count)
        {
            activeBrickCount = count;

            // If there are no bricks left initially, then trigger victory immediately.
            if (activeBrickCount <= 0)
            {
                StartCoroutine(VictorySequence());
            }
        }

        /// <summary>
        /// Adds points to the score and updates the UI.
        /// </summary>
        /// <param name="amount">Points to add.</param>
        public void AddScore(int amount)
        {
            CurrentScore += amount;

            if (hudView != null)
            {
                hudView.UpdateScore(CurrentScore);
            }
        }

        /// <summary>
        /// Clones all active balls to create a triple-ball effect.
        /// </summary>
        public void ActivateTripleBall()
        {
            List<BallController> activeBalls = ballPool.GetActiveBalls();

            if (activeBalls.Count == 0)
            {
                return;
            }

            // Create a copy of the list to iterate safely while spawning new balls.
            List<BallController> ballsToClone = new List<BallController>(activeBalls);

            foreach (BallController sourceBall in ballsToClone)
            {
                if (sourceBall == null || !sourceBall.gameObject.activeSelf)
                {
                    continue;
                }

                Rigidbody2D rb = sourceBall.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    continue;
                }

                Vector2 currentVelocity = rb.velocity;
                float speed = currentVelocity.magnitude;

                // If the ball is moving too slowly, then enforce a minimum speed.
                if (speed < 5f)
                {
                    speed = 7f;
                }

                Vector2 direction = currentVelocity.normalized;
                Vector3 position = sourceBall.transform.position;

                // Calculate directions rotated by -20 and +20 degrees.
                Vector2 dir1 = (Quaternion.Euler(0, 0, -20) * direction) * speed;
                Vector2 dir2 = (Quaternion.Euler(0, 0, 20) * direction) * speed;

                ballPool.GetBall().SpawnAsClone(position, dir1);
                ballPool.GetBall().SpawnAsClone(position, dir2);
            }
        }

        /// <summary>
        /// Handles logic when a ball falls out of bounds.
        /// </summary>
        /// <param name="ball">The ball object involved.</param>
        public void HandleBallLost(BallController ball)
        {
            SoundManager.Instance.PlayOneShot(SoundType.BallLost);
            ballPool.ReturnBall(ball);

            int remainingBalls = ballPool.GetActiveBalls().Count;

            // If no balls remain in play, then the player loses a life.
            if (remainingBalls <= 0)
            {
                LoseLife();
            }
        }

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Awake()
        {
            // If an instance already exists, then destroy this duplicate.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // If any essential reference is missing, then log an error and stop execution.
            if (hudView == null || screenManager == null || levelManager == null || ballPool == null || powerUpPool == null)
            {
                Debug.LogError("GameManager: Missing references to core components!");
                return;
            }

            Brick.OnAnyBrickDestroyed += HandleBrickDestroyed;
            PowerUp.OnPowerUpPickedUp += HandlePowerUpPickup;

            StartNewGame();
        }

        private void OnDestroy()
        {
            Brick.OnAnyBrickDestroyed -= HandleBrickDestroyed;
            PowerUp.OnPowerUpPickedUp -= HandlePowerUpPickup;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Resets game state and loads the initial level.
        /// </summary>
        private void StartNewGame()
        {
            CurrentLives = startLives;
            CurrentScore = 0;

            // If a global game instance exists, then retrieve the selected level index.
            if (GameInstance.Instance != null)
            {
                currentLevel = GameInstance.Instance.SelectedLevelIndex;
            }
            else
            {
                currentLevel = 1;
            }

            if (hudView != null)
            {
                hudView.UpdateLives(CurrentLives);
                hudView.UpdateScore(CurrentScore);
                hudView.UpdateLevel(currentLevel);
            }

            ResetPowerUpLogic();
            StartCoroutine(LoadLevelRoutine(currentLevel, false));
        }

        /// <summary>
        /// Handles the loss of a life or respawns the ball if god mode is active.
        /// </summary>
        private void LoseLife()
        {
            // If the bottom wall is ignored (god mode), then just respawn the ball.
            if (ignoreBottomWall)
            {
                RespawnMainBall();
                return;
            }

            CurrentLives--;

            if (hudView != null)
            {
                hudView.UpdateLives(CurrentLives);
            }

            // If lives have run out, then start the Game Over sequence.
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

        /// <summary>
        /// Callback triggered when a brick is destroyed.
        /// </summary>
        /// <param name="brickPos">Position of the destroyed brick.</param>
        private void HandleBrickDestroyed(Vector3 brickPos)
        {
            activeBrickCount--;

            // If all bricks are destroyed, then proceed to the victory sequence.
            if (activeBrickCount <= 0)
            {
                StartCoroutine(VictorySequence());
                return;
            }

            bricksDestroyedCounter++;

            // If the destroyed counter reaches the threshold, then spawn a power-up.
            if (bricksDestroyedCounter >= currentPowerUpThreshold)
            {
                if (powerUpPool != null)
                {
                    powerUpPool.GetPowerUp(brickPos);
                }

                bricksDestroyedCounter = 0;
                currentPowerUpThreshold += powerUpStepIncrement;
            }
        }

        private void ResetPowerUpLogic()
        {
            bricksDestroyedCounter = 0;
            currentPowerUpThreshold = startBricksForPowerUp;
        }

        private void RespawnMainBall()
        {
            BallController newBall = ballPool.GetBall();
            newBall.ResetToPaddle();
        }

        private IEnumerator GameOverSequence()
        {
            Time.timeScale = 0f;
            SoundManager.Instance.PlayOneShot(SoundType.GameOver);
            MusicManager.Instance.PlayGameOverMusic();

            if (screenManager != null)
            {
                screenManager.ShowGameOver(true);
            }

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

            if (screenManager != null)
            {
                screenManager.ShowVictory(true);
            }

            yield return new WaitForSeconds(2f);

            if (screenManager != null)
            {
                screenManager.ShowVictory(false);
            }

            currentLevel++;

            if (GameInstance.Instance != null)
            {
                GameInstance.Instance.SetLevelData(currentLevel);
            }

            StartCoroutine(LoadLevelRoutine(currentLevel, true));
        }

        /// <summary>
        /// Loads the level geometry and resets game objects.
        /// </summary>
        /// <param name="level">Level index to load.</param>
        /// <param name="showTransition">Whether to show the UI transition.</param>
        private IEnumerator LoadLevelRoutine(int level, bool showTransition)
        {
            if (showTransition && screenManager != null)
            {
                screenManager.ShowLevelTransition($"Level {level}");
            }

            // If a GameInstance exists, then use its seed, otherwise build a chaos level.
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

            if (hudView != null)
            {
                hudView.UpdateLevel(currentLevel);
            }

            ResetPowerUpLogic();
            RespawnMainBall();
            MusicManager.Instance.PlayGameplayMusic();

            if (showTransition)
            {
                yield return new WaitForSeconds(2f);

                if (screenManager != null)
                {
                    screenManager.HideLevelTransition();
                }
            }
        }

        /// <summary>
        /// Handles effects when a power-up is collected.
        /// </summary>
        /// <param name="bonusPoints">Score value of the power-up.</param>
        private void HandlePowerUpPickup(int bonusPoints)
        {
            SoundManager.Instance.PlayOneShot(SoundType.PowerUpPickup);
            AddScore(bonusPoints);
            ActivateTripleBall();
        }
    }
}