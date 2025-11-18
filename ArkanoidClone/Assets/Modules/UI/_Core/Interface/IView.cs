namespace MiniIT.UI
{
    /// <summary>
    /// Basic contract for any view element.
    /// </summary>
    public interface IView
    {
        bool IsVisible { get; }
        void Show();
        void Hide();
    }
}