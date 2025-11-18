using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// Абстрактный класс View. Реализует общую логику анимации через CanvasGroup.
/// ДЕМОНСТРАЦИЯ: Абстрактные классы, Protected методы, Виртуальные методы, DOTween.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class BaseView : MonoBehaviour, IAnimatedView
{
    [BoxGroup("View Settings"), SerializeField] protected float defaultFadeDuration = 0.3f;

    // Ленивая инициализация CanvasGroup
    private CanvasGroup _canvasGroup;
    public CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();

    public bool IsVisible => CanvasGroup.alpha > 0;

    // Виртуальный метод Awake, чтобы наследники могли добавить логику
    protected virtual void Awake()
    {
        // Начальное состояние
        CanvasGroup.alpha = 0;
        CanvasGroup.interactable = false;
        CanvasGroup.blocksRaycasts = false;
    }

    [Button("Test Show")] // Odin Button для теста в редакторе
    public void Show() => Show(defaultFadeDuration);

    public void Show(float duration, Action onComplete = null)
    {
        CanvasGroup.DOKill();
        CanvasGroup.interactable = true;
        CanvasGroup.blocksRaycasts = true;
        CanvasGroup.DOFade(1f, duration).OnComplete(() => onComplete?.Invoke());
    }

    [Button("Test Hide")]
    public void Hide() => Hide(defaultFadeDuration);

    public void Hide(float duration, Action onComplete = null)
    {
        CanvasGroup.DOKill();
        CanvasGroup.interactable = false;
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.DOFade(0f, duration).OnComplete(() => onComplete?.Invoke());
    }
}