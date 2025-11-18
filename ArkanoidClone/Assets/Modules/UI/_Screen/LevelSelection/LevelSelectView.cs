using DG.Tweening;
using NaughtyAttributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectView : BaseView, IDataView<int>
{
    #region UI Elements
    [BoxGroup("UI References"), Required, SerializeField] private TextMeshProUGUI _levelText;

    [BoxGroup("Buttons"), Required, SerializeField] private Button _btnBack;
    [BoxGroup("Buttons"), Required, SerializeField] private Button _btnStart;

    [BoxGroup("Buttons"), Required, SerializeField] private Button _btnPrevBig;  // <<
    [BoxGroup("Buttons"), Required, SerializeField] private Button _btnPrev;     // <
    [BoxGroup("Buttons"), Required, SerializeField] private Button _btnNext;     // >
    [BoxGroup("Buttons"), Required, SerializeField] private Button _btnNextBig;  // >>
    #endregion

    #region Events
    // События, на которые подпишется Presenter
    public event Action OnBackClicked;
    public event Action OnStartClicked;
    public event Action OnNextClicked;
    public event Action OnPrevClicked;
    public event Action OnNextBigClicked;
    public event Action OnPrevBigClicked;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        BindButtons();
    }

    // ДЕМОНСТРАЦИЯ: Лямбда-выражения для подписки
    private void BindButtons()
    {
        _btnBack.onClick.AddListener(() => OnBackClicked?.Invoke());
        _btnStart.onClick.AddListener(() => OnStartClicked?.Invoke());

        _btnPrev.onClick.AddListener(() => OnPrevClicked?.Invoke());
        _btnNext.onClick.AddListener(() => OnNextClicked?.Invoke());
        _btnPrevBig.onClick.AddListener(() => OnPrevBigClicked?.Invoke());
        _btnNextBig.onClick.AddListener(() => OnNextBigClicked?.Invoke());
    }

    // Реализация IDataView - обновление отображения
    public void UpdateView(int currentLevel)
    {
        // Можно добавить анимацию текста через DOTween punch
        _levelText.transform.DOKill();
        _levelText.transform.localScale = Vector3.one;
        _levelText.text = $"Level {currentLevel}";
        _levelText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
    }
}