namespace MiniIT.UI
{
    /// <summary>
    /// Generic contract for views that display specific data.
    /// </summary>
    /// <typeparam name="T">The type of data to display.</typeparam>
    public interface IDataView<T> : IAnimatedView
    {
        void UpdateView(T data);
    }
}