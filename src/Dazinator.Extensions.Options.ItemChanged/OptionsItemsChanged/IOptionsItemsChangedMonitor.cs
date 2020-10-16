namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;

    public interface IOptionsItemsChangedMonitor<TOptions, TOptionsItem, TKey> where TOptionsItem : class
    {
        IDisposable OnChange(Action<OptionsItemsChangedEventArgs<TOptions, TOptionsItem, TKey>> listener);
    }
}
