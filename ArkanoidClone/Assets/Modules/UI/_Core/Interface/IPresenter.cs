namespace MiniIT.UI
{
    /// <summary>
    /// Base contract for UI Presenters.
    /// </summary>
    public interface IPresenter
    {
        void Initialize();
        void Dispose();
    }
}