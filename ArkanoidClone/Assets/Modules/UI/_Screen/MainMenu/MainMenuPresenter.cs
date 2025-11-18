using UnityEngine;
using UnityEditor;
using NaughtyAttributes;
using MiniIT.LEVELS;
using MiniIT.AUDIO;

namespace MiniIT.UI
{
    public class MainMenuPresenter : MonoBehaviour, IPresenter
    {
        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("REFERENCES")]
        [SerializeField, Required]
        private MainMenuView view = null;

        [BoxGroup("REFERENCES")]
        [SerializeField, Required]
        private LevelSelectPresenter levelSelectPresenter = null;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        public void Initialize()
        {
            // Main Buttons
            view.NewGameButton.onClick.AddListener(OnNewGameClicked);
            view.QuitButton.onClick.AddListener(OnQuitClicked);

            // Confirmation Buttons
            view.ConfirmYesButton.onClick.AddListener(OnConfirmQuit);
            view.ConfirmNoButton.onClick.AddListener(OnCancelQuit);
        }

        public void Show()
        {
            view.Show();
        }

        public void Dispose()
        {
            // Clean up listeners
            view.NewGameButton.onClick.RemoveAllListeners();
            view.QuitButton.onClick.RemoveAllListeners();
        }

        // ========================================================================
        // --- PRIVATE METHODS & UNITY CALLBACKS ---
        // ========================================================================

        private void Start()
        {
            Initialize();
            view.Show();

            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayMenuMusic();
            }

            // Ensure bricks are hidden in Main Menu
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SetLevelVisibility(false);
            }
        }

        // --- EVENT HANDLERS ---

        private void OnNewGameClicked()
        {
            SoundManager.Instance.PlayOneShot(SoundType.ButtonClick);

            view.Hide(0.3f, () =>
            {
                if (levelSelectPresenter != null)
                {
                    levelSelectPresenter.Show();
                }
            });
        }

        private void OnQuitClicked()
        {
            SoundManager.Instance.PlayOneShot(SoundType.ButtonClick);
            view.SetConfirmationActive(true);
        }

        private void OnCancelQuit()
        {
            SoundManager.Instance.PlayOneShot(SoundType.ButtonClick);
            view.SetConfirmationActive(false);
        }

        private void OnConfirmQuit()
        {
            SoundManager.Instance.PlayOneShot(SoundType.ButtonClick);
            Debug.Log("[MainMenu] Quitting Game...");

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}