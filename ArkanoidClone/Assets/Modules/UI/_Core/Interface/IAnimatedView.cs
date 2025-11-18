using System;

public interface IAnimatedView : IView
{
    void Show(float duration, Action onComplete = null);
    void Hide(float duration, Action onComplete = null);
}