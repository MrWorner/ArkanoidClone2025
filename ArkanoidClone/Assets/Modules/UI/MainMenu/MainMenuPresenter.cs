using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPresenter : MonoBehaviour, IPresenter
{
    [SerializeField] private MainMenuView _view;
    [SerializeField] private LevelSelectPresenter _levelSelectPresenter;

    private void Start()
    {
        Initialize();
        // Сразу показываем меню при старте сцены
        _view.Show();
    }

    public void Initialize()
    {
        _view.NewGameButton.onClick.AddListener(OnNewGameClicked);
    }

    private void OnNewGameClicked()
    {
        _view.Hide(0.3f, () =>
        {
            // После скрытия меню, показываем выбор уровня
            _levelSelectPresenter.Show();
        });
    }

    public void Show() => _view.Show();

    public void Dispose() { }
}