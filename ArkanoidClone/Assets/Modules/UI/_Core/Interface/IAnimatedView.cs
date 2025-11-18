using System;

namespace MiniIT.UI
{
    /// <summary>
    /// Contract for views that support animated transitions.
    /// </summary>
    public interface IAnimatedView : IView
    {
        void Show(float duration, Action onComplete = null);
        void Hide(float duration, Action onComplete = null);
    }
}