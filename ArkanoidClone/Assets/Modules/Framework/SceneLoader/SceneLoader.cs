using MiniIT.AUDIO;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameScene
{
    MainMenu,
    GameScene
}

public class SceneLoader : MonoBehaviour
{

    #region Свойства
    private static SceneLoader _instance;
    public static SceneLoader Instance => _instance;

    //public GameScene SceneToLoad { get => _sceneToLoad; }
    #endregion

    #region Методы UNITY
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    #region Публичные методы
    /// <summary>
    /// Загружает указанную сцену.
    /// </summary>
    /// <param name="newScene">Сцена для загрузки.</param>
    public void LoadNextScene(GameScene newScene)
    {
        // ИЗМЕНЕНО: Метод теперь принимает GameScene вместо string.
        // Больше нет необходимости изменять поле _sceneToLoad.
        StartCoroutine(LoadSceneAndFade(newScene));
    }
    #endregion

    #region Личные методы
    // ИЗМЕНЕНО: Корутина теперь принимает GameScene и сама получает имя сцены.
    private IEnumerator LoadSceneAndFade(GameScene scene)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopMusic();
        }

        string sceneName = GetSceneName(scene); // Получаем строковое имя сцены из enum.

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.ShowLoadingScreen();
            //yield return new WaitForSeconds(0.5f);
            yield return new WaitForSeconds(1f);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
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
                Debug.LogError($"[SceneLoader] Имя для сцены '{scene}' не найдено!");
                return null;
        }
    }
    #endregion
}