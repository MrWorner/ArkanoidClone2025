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
            // Если _levelSelectPresenter пустой (null), тут ничего не произойдет
            // и ошибок в консоли не будет (если нет проверки)
            if (_levelSelectPresenter != null)
            {
                Debug.Log("Вызываю Show у выбора уровня"); // Добавьте этот лог
                _levelSelectPresenter.Show();
            }
            else
            {
                Debug.LogError("ЗАБЫЛИ ПРИВЯЗАТЬ LevelSelectPresenter в Инспекторе!");
            }
        });
    }

    public void Show() => _view.Show();

    public void Dispose() { }
}