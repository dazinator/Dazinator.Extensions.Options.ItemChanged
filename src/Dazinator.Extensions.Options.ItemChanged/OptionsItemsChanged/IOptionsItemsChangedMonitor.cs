namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;

    public interface IOptionsItemsChangedMonitor<TKey, TOptionsItem> where TOptionsItem : class
    {
        IDisposable OnChange(Action<OptionsItemsChangedEventArgs<TKey, TOptionsItem>> listener);
    }
}
