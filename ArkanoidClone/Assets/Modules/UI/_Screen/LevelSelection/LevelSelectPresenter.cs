using MiniIT.AUDIO;
using MiniIT.CORE;
using MiniIT.LEVELS;
using MiniIT.SCENELOADER;
using NaughtyAttributes;
using UnityEngine;

namespace MiniIT.UI
{
    public class LevelSelectPresenter : MonoBehaviour, IPresenter
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("DEPENDENCIES")]
        [SerializeField, Required]
        private LevelSelectView view = null;

        [BoxGroup("DEPENDENCIES")]
        [SerializeField, Required]
        private MainMenuPresenter mainMenuPresenter = null;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        public void Initialize()
        {
            // Subscribe to view events
            view.OnNextClicked += () => ChangeLevel(1);
            view.OnPrevClicked += () => ChangeLevel(-1);
            view.OnNextBigClicked += () => ChangeLevel(10);
            view.OnPrevBigClicked += () => ChangeLevel(-10);

            view.OnBackClicked += OnBackClicked;
            view.OnStartClicked += OnStartClicked;
        }

        public void Show()
        {
            view.Show();

            // 1. Enable brick visibility when entering level selection
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SetLevelVisibility(true);
            }

            // 2. Regenerate current level preview
            RefreshLevelGeneration();
        }

        public void Hide()
        {
            view.Hide();
        }

        public void Dispose()
        {
            // Cleanup logic if needed
        }

        // ========================================================================
        // --- PRIVATE METHODS & UNITY CALLBACKS ---
        // ========================================================================

        private void Start()
        {
            Initialize();
        }

        private void ChangeLevel(int amount)
        {
            SoundManager.Instance.PlayOneShot(SoundType.ButtonClick);

            if (GameInstance.Instance == null)
            {
                return;
            }

            int current = GameInstance.Instance.SelectedLevelIndex;

            // Update global game instance (handles bounds check internally)
            GameInstance.Instance.SetLevelData(current + amount);

            RefreshLevelGeneration();
        }

        private void RefreshLevelGeneration()
        {
            if (GameInstance.Instance == null || LevelManager.Instance == null)
            {
                return;
            }

            // Update UI
            view.UpdateView(GameInstance.Instance.SelectedLevelIndex);

            // Generate Level
            LevelManager.Instance.GenerateLevelBySeed(GameInstance.Instance.CurrentLevelSeed);
        }

        private void OnBackClicked()
        {
            SoundManager.Instance.PlayOneShot(SoundType.ButtonClick);

            view.Hide(0.3f, () =>
            {
                // 1. Disable brick visibility
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.SetLevelVisibility(false);
                }

                // 2. Show Main Menu
                if (mainMenuPresenter != null)
                {
                    mainMenuPresenter.Show();
                }
            });
        }

        private void OnStartClicked()
        {
            SoundManager.Instance.PlayOneShot(SoundType.ButtonClickStart);
            SceneLoader.Instance.LoadNextScene(GameScene.GameScene);
        }
    }
}