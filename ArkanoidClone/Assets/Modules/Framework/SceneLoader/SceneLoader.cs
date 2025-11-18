using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using NaughtyAttributes;
using MiniIT.AUDIO;
using MiniIT.UI;

namespace MiniIT.SCENELOADER
{
    public enum GameScene
    {
        MainMenu,
        GameScene
    }

    public class SceneLoader : MonoBehaviour
    {
        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public static SceneLoader Instance
        {
            get;
            private set;
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("SETTINGS")]
        [Tooltip("Minimum time to show the loading screen.")]
        [SerializeField]
        private float minLoadingTime = 1f;

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// Initiates the sequence to load a new scene with a fade effect.
        /// </summary>
        /// <param name="newScene">The enum identifier of the scene to load.</param>
        public void LoadNextScene(GameScene newScene)
        {
            StartCoroutine(LoadSceneAndFade(newScene));
        }

        // ========================================================================
        // --- PRIVATE METHODS & UNITY CALLBACKS ---
        // ========================================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private IEnumerator LoadSceneAndFade(GameScene scene)
        {
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.StopMusic();
            }

            string sceneName = GetSceneName(scene);

            if (ScreenFader.Instance != null)
            {
                ScreenFader.Instance.ShowLoadingScreen();
                yield return new WaitForSeconds(minLoadingTime);
            }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                // Unity loads scene up to 0.9, then waits for allowSceneActivation
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            if (ScreenFader.Instance != null)
            {
                ScreenFader.Instance.HideLoadingScreen();
            }
        }

        private string GetSceneName(GameScene scene)
        {
            switch (scene)
            {
                case GameScene.MainMenu:
                    return "01_MainMenu";

                case GameScene.GameScene:
                    return "02_GameScene";

                default:
                    Debug.LogError($"[SceneLoader] Scene name for '{scene}' not found!");
                    return null;
            }
        }
    }
}