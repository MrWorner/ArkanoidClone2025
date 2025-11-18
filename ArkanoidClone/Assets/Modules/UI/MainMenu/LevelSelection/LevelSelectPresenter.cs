using UnityEngine;
using System.Reflection;
using NaughtyAttributes;

public class LevelSelectPresenter : MonoBehaviour, IPresenter
{
    [BoxGroup("Dependencies"), Required, SerializeField] private LevelSelectView _view;
    // Ссылка на Главное меню, чтобы вернуться назад
    [BoxGroup("Dependencies"), Required, SerializeField] private MainMenuPresenter _mainMenuPresenter;
    [BoxGroup("Dependencies"), Required, SerializeField] private GameObject _brickPool;

    private LevelSelectionModel _model;

    private void Awake()
    {
        _brickPool.SetActive(false);
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {

        _model = new LevelSelectionModel();

        // ДЕМОНСТРАЦИЯ: Проверка на null через паттер matching
        if (_view is null)
        {
            Debug.LogError("View is not assigned!");
            return;
        }

        // Подписка на события View
        _view.OnBackClicked += HandleBack;
        _view.OnStartClicked += HandleStart;
        _view.OnNextClicked += () => ChangeLevel(1);
        _view.OnPrevClicked += () => ChangeLevel(-1);
        _view.OnNextBigClicked += () => ChangeLevel(10);
        _view.OnPrevBigClicked += () => ChangeLevel(-10);

        // Инициализация отображения
        _view.UpdateView(_model.CurrentLevel);

        // Для демонстрации Reflection (как просили в ТЗ)
        LogMethodNamesViaReflection();
    }

    public void Show()
    {
        _view.Show();
        _brickPool.SetActive(true);
    }

    public void Hide()
    {
        _brickPool.SetActive(false);
        _view.Hide();
    }
    #region Logic Handlers

    private void ChangeLevel(int amount)
    {
        _model.SetLevel(_model.CurrentLevel + amount);
        _view.UpdateView(_model.CurrentLevel);
    }

    private void HandleStart()
    {
        // Сохраняем выбранный уровень (например, в PlayerPrefs или глобальный менеджер)
        PlayerPrefs.SetInt("SelectedLevel", _model.CurrentLevel);
        PlayerPrefs.Save();

        // Используем ваш SceneLoader
        SceneLoader.Instance.LoadNextScene(GameScene.GameScene);
    }

    private void HandleBack()
    {
        _view.Hide(0.3f, () =>
        {
            // Логика возврата в главное меню

            _brickPool.SetActive(false);

            if (_mainMenuPresenter != null)
                _mainMenuPresenter.Show();
        });
    }

    public void Dispose()
    {
        // Хорошим тоном является отписка от событий
        if (_view != null)
        {
            _view.OnBackClicked -= HandleBack;
            _view.OnStartClicked -= HandleStart;
            // ... остальные отписки
        }
    }
    #endregion

    // ДЕМОНСТРАЦИЯ: Reflection (Требование ТЗ)
    [Button("Debug Reflection")]
    private void LogMethodNamesViaReflection()
    {
        var type = this.GetType();
        MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        /*
        Debug.Log($"[Reflection] Methods in {type.Name}:");
        foreach (var method in methods)
        {
            Debug.Log($"- {method.Name}");
        }
        */
    }
}