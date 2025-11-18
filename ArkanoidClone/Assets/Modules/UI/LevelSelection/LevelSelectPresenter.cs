using UnityEngine;
using NaughtyAttributes;

public class LevelSelectPresenter : MonoBehaviour, IPresenter
{
    [BoxGroup("Dependencies"), Required]
    [SerializeField] private LevelSelectView _view;

    [BoxGroup("Dependencies"), Required]
    [SerializeField] private MainMenuPresenter _mainMenuPresenter;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        // Подписка на кнопки навигации
        _view.OnNextClicked += () => ChangeLevel(1);
        _view.OnPrevClicked += () => ChangeLevel(-1);
        _view.OnNextBigClicked += () => ChangeLevel(10);
        _view.OnPrevBigClicked += () => ChangeLevel(-10);

        _view.OnBackClicked += OnBackClicked;
        _view.OnStartClicked += OnStartClicked;
    }

    public void Show()
    {
        _view.Show();

        // 1. При открытии экрана выбора уровня - ВКЛЮЧАЕМ видимость кирпичей
        LevelManager.Instance.SetLevelVisibility(true);

        // 2. Генерируем уровень который сейчас сохранен в GameInstance
        RefreshLevelGeneration();
    }

    private void ChangeLevel(int amount)
    {
        // 1. Берем текущий уровень
        int current = GameInstance.Instance.SelectedLevelIndex;

        // 2. Меняем и сохраняем в GameInstance (он там внутри сам посчитает Seed)
        GameInstance.Instance.SetLevelData(current + amount);

        // 3. Обновляем UI и Генерируем мир
        RefreshLevelGeneration();
    }

    private void RefreshLevelGeneration()
    {
        // Обновляем текст во View
        _view.UpdateView(GameInstance.Instance.SelectedLevelIndex);

        // Запускаем генерацию уровня по Seed
        LevelManager.Instance.GenerateLevelBySeed(GameInstance.Instance.CurrentLevelSeed);
    }

    private void OnBackClicked()
    {
        _view.Hide(0.3f, () =>
        {
            // 1. При выходе назад - ВЫКЛЮЧАЕМ видимость кирпичей
            LevelManager.Instance.SetLevelVisibility(false);

            // 2. Показываем главное меню
            if (_mainMenuPresenter != null)
                _mainMenuPresenter.Show();
        });
    }

    private void OnStartClicked()
    {
        SceneLoader.Instance.LoadNextScene(GameScene.GameScene);
    }

    public void Hide() => _view.Hide();
    public void Dispose() { }
}