namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Monitors changes to options and raises an event when a change occurs, supplying both the old and the new instance.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class OptionsChangedMonitor<TOptions> : IDisposable
    {
        private TOptions _instance;
        private IDisposable _listening;
        private bool _disposedValue;

        internal event Action<OptionsChangedEventArgs<TOptions>> OnOptionsChanged;

        internal class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<OptionsChangedEventArgs<TOptions>> _listener;
            private readonly OptionsChangedMonitor<TOptions> _monitor;

            public ChangeTrackerDisposable(OptionsChangedMonitor<TOptions> monitor, Action<OptionsChangedEventArgs<TOptions>> listener)
            {
                _listener = listener;
                _monitor = monitor;
            }

            public void OnOptionsChanged(OptionsChangedEventArgs<TOptions> args) => _listener.Invoke(args);

            public void Dispose() => _monitor.OnOptionsChanged -= OnOptionsChanged;
        }


        public OptionsChangedMonitor(IOptionsMonitor<TOptions> optionsMonitor)
        {
            var debouncer = new Debouncer(TimeSpan.FromMilliseconds(500));

            _instance = optionsMonitor.CurrentValue;
            _listening = optionsMonitor.OnChange((a) => debouncer.Debouce(() => OnChanged(a, _instance)));
        }

        protected virtual void OnChanged(TOptions newInstance, TOptions oldInstance)
        {
            InvokeChanged(new OptionsChangedEventArgs<TOptions>() { Current = newInstance, Old = oldInstance });
            _instance = newInstance;
        }

        private void InvokeChanged(OptionsChangedEventArgs<TOptions> args)
        {
            if (OnOptionsChanged != null)
            {
                OnOptionsChanged?.Invoke(args);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _listening?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~OptionsChangedMonitor()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
