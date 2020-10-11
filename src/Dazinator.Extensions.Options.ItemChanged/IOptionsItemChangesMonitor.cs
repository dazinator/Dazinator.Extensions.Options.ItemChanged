namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;

    public interface IOptionsItemChangesMonitor<TKey, TOptionsItem> where TOptionsItem : class, IHaveKey<TKey>
    {
        IDisposable OnChange(Action<OptionsItemChangedEventArgs<TKey, TOptionsItem>> listener);
    }
}
