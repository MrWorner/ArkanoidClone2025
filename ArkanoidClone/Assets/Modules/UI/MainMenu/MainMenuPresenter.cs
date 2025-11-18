using UnityEngine;
using UnityEditor; // Нужно для остановки игры в редакторе
using NaughtyAttributes;

public class MainMenuPresenter : MonoBehaviour, IPresenter
{
    [BoxGroup("References"), Required]
    [SerializeField] private MainMenuView _view;

    [BoxGroup("References"), Required]
    [SerializeField] private LevelSelectPresenter _levelSelectPresenter;

    private void Start()
    {
        Initialize();
        _view.Show();

        // При старте игры в главном меню кирпичи не должны быть видны
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetLevelVisibility(false);
        }
    }

    public void Initialize()
    {
        // Основные кнопки
        _view.NewGameButton.onClick.AddListener(OnNewGameClicked);
        _view.QuitButton.onClick.AddListener(OnQuitClicked);

        // Кнопки подтверждения
        _view.ConfirmYesButton.onClick.AddListener(OnConfirmQuit);
        _view.ConfirmNoButton.onClick.AddListener(OnCancelQuit);
    }

    #region Handlers

    private void OnNewGameClicked()
    {
        _view.Hide(0.3f, () =>
        {
            if (_levelSelectPresenter != null)
                _levelSelectPresenter.Show();
        });
    }

    // 1. Нажали кнопку "Exit" -> Показываем попап
    private void OnQuitClicked()
    {
        _view.SetConfirmationActive(true);
    }

    // 2. Нажали "Нет" -> Скрываем попап
    private void OnCancelQuit()
    {
        _view.SetConfirmationActive(false);
    }

    // 3. Нажали "Да" -> Выходим
    private void OnConfirmQuit()
    {
        Debug.Log("[MainMenu] Quitting Game...");

        // Эта конструкция работает и в Редакторе Unity, и в сбилженной игре
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    public void Show() => _view.Show();

    public void Dispose()
    {
        // Хорошая практика - отписываться, хотя для меню это не критично
        _view.NewGameButton.onClick.RemoveAllListeners();
        _view.QuitButton.onClick.RemoveAllListeners();
    }
}