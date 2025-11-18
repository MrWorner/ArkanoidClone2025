using DG.Tweening;
using NaughtyAttributes;
using System;
using UnityEngine;

public abstract class BaseView : MonoBehaviour, IAnimatedView
{
    #region References
    // ТРЕБОВАНИЕ: Назначаем руками в Инспекторе
    [BoxGroup("Base References"), Required]
    [SerializeField] private Canvas _canvas;

    [BoxGroup("Base References"), Required]
    [SerializeField] private CanvasGroup _canvasGroup;
    #endregion

    #region Settings
    [BoxGroup("View Settings"), SerializeField] protected float defaultFadeDuration = 0.3f;
    #endregion

    // Свойство для проверки видимости
    public bool IsVisible => _canvas != null && _canvas.enabled && _canvasGroup.alpha > 0;

    protected virtual void Awake()
    {
        // При старте сразу приводим в скрытое состояние (без анимации)
        ForceHide();
    }

    #region IView (Без параметров)

    [Button("Show Default")]
    public void Show() => Show(defaultFadeDuration);

    [Button("Hide Default")]
    public void Hide() => Hide(defaultFadeDuration);

    #endregion

    #region IAnimatedView (С параметрами)

    public void Show(float duration, Action onComplete = null)
    {
        // ИСПРАВЛЕНИЕ:
        // Мы включаем сам GameObject, на котором висит Canvas.
        // Это сработает, даже если Canvas - это дочерний объект, который был выключен.
        if (_canvas != null)
        {
            _canvas.gameObject.SetActive(true); // Включаем объект
            _canvas.enabled = true;             // Включаем компонент (на всякий случай)
        }

        _canvasGroup.DOKill();
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        // Если альфа уже 1 (например, после быстрого переключения), сбрасываем в 0
        if (_canvasGroup.alpha >= 0.99f) _canvasGroup.alpha = 0f;

        _canvasGroup.DOFade(1f, duration).OnComplete(() => onComplete?.Invoke());
    }

    public void Hide(float duration, Action onComplete = null)
    {
        _canvasGroup.DOKill();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _canvasGroup.DOFade(0f, duration).OnComplete(() =>
        {
            // ИСПРАВЛЕНИЕ:
            // Выключаем GameObject целиком, чтобы гарантированно убрать отрисовку
            if (_canvas != null)
                _canvas.gameObject.SetActive(false);

            onComplete?.Invoke();
        });
    }

    #endregion

    #region Helpers

    // Метод для мгновенного скрытия при старте (без событий)
    private void ForceHide()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        if (_canvas != null)
            _canvas.enabled = false;
    }

    #endregion
}