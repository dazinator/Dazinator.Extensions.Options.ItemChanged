namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Monitors changes to options and raises an event when a change occurs, supplying both the old and the new instance.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class OptionsChangedMonitor<TOptions> : IDisposable, IOptionsChangedMonitor<TOptions>
    {
        private readonly IDisposable _listening;
        private bool _disposedValue;

        public TOptions Instance { get; private set; }

        internal event Action<OptionsChangedEventArgs<TOptions>> OnOptionsChanged;

        public OptionsChangedMonitor(IOptionsMonitor<TOptions> optionsMonitor)
        {
            var debouncer = new Debouncer(TimeSpan.FromMilliseconds(500));

            Instance = optionsMonitor.CurrentValue;
            _listening = optionsMonitor.OnChange((a) => debouncer.Debouce(() => OnChanged(a, Instance)));
        }

        protected virtual void OnChanged(TOptions newInstance, TOptions oldInstance)
        {
            Instance = newInstance;
            InvokeChanged(new OptionsChangedEventArgs<TOptions>() { Current = newInstance, Old = oldInstance });            
        }

        private void InvokeChanged(OptionsChangedEventArgs<TOptions> args)
        {
            if (OnOptionsChanged != null)
            {
                OnOptionsChanged?.Invoke(args);
            }
        }

        /// <summary>
        /// Registers a listener to be called whenever options changes.
        /// </summary>
        /// <param name="listener">The action to be invoked the options changes.</param>
        /// <returns>An <see cref="IDisposable"/> which should be disposed to stop listening for changes.</returns>
        public IDisposable OnChange(Action<OptionsChangedEventArgs<TOptions>> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            OnOptionsChanged += disposable.OnOptionsChanged;
            return disposable;
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

    }
}
