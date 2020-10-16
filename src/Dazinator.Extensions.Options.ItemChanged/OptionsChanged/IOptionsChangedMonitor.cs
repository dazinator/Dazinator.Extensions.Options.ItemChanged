using System;

namespace Dazinator.Extensions.Options.ItemChanged
{
    public interface IOptionsChangedMonitor<TOptions>
    {
        IDisposable OnChange(Action<OptionsChangedEventArgs<TOptions>> listener);
    }
}