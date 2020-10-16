namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    public class OptionsItemsChangedMonitor<TOptions, TOptionsItem, TKey> : OptionsChangedMonitor<TOptions>,
        IDisposable, IOptionsItemsChangedMonitor<TOptions, TOptionsItem, TKey>
        where TOptionsItem : class
    {
        private readonly ILogger<OptionsItemsChangedMonitor<TOptions, TOptionsItem, TKey>> _logger;
        private readonly List<ItemsMemberAccessor<TOptions, TOptionsItem>> _itemAccessors;
        private readonly IDisposable _listening;

        internal event Action<OptionsItemsChangedEventArgs<TOptions, TOptionsItem, TKey>> OnItemChange;
        private readonly ICollectionDiffer<TOptionsItem, TOptionsItem> _differ;
        private TOptions _instance;

        //  private readonly string _memberName;


        public OptionsItemsChangedMonitor(
            IOptionsMonitor<TOptions> optionsMonitor,
            ILogger<OptionsItemsChangedMonitor<TOptions, TOptionsItem, TKey>> logger,
            IEnumerable<ItemsMemberAccessor<TOptions, TOptionsItem>> itemAccessors,
            ICollectionDiffer<TOptionsItem, TOptionsItem> differ
           ) : base(optionsMonitor)
        {
            // _optionsMonitor = optionsMonitor;
            _instance = optionsMonitor.CurrentValue;
            _logger = logger;
            _itemAccessors = itemAccessors.ToList();
            _differ = differ;


            //  _itemsAccessor = itemsAccessor;
            // _differ = new CollectionDifferUsingInterface<TKey, TOptions, TOptionsItem>();

            //  _itemAccessorExpression.Body.pro
            var debouncer = new Debouncer(TimeSpan.FromMilliseconds(500));
            _listening = optionsMonitor.OnChange((a) => debouncer.Debouce(() => OnChanged(a)));
        }

        private void OnChanged(TOptions newInstance)
        {
            _logger.LogInformation("Change detected for options, diffing items.");

            // calculate which items are new, modified, or removed since last time.

            // We should lock in case this fires concurrently on differetn threads and a key gets added?
            foreach (var itemsEnumerable in _itemAccessors)
            {
                var oldItems = itemsEnumerable.GetItems(_instance);
                var newItems = itemsEnumerable.GetItems(newInstance);

                var differences = _differ.GetChanges(newItems, oldItems);
                if (!differences.Any())
                {
                    continue;
                }

                var args = new OptionsItemsChangedEventArgs<TOptions, TOptionsItem, TKey>()
                {
                    Old = _instance,
                    Current = newInstance,
                    MemberName = itemsEnumerable.MemberName,
                    Differences = new HashSet<Difference<TOptionsItem, TOptionsItem>>(differences)
                };

                InvokeChanged(args);
            }
            _instance = newInstance;
        }


        private void InvokeChanged(OptionsItemsChangedEventArgs<TOptions, TOptionsItem, TKey> args)
        {
            if (OnItemChange != null)
            {
                OnItemChange?.Invoke(args);
            }
        }

        /// <summary>
        /// Registers a listener to be called whenever items change.
        /// </summary>
        /// <param name="listener">The action to be invoked when whenever a mapping item on <see cref="MappingOptions{TKey, TOptions}"/> changes.</param>
        /// <returns>An <see cref="IDisposable"/> which should be disposed to stop listening for changes.</returns>
        public IDisposable OnChange(Action<OptionsItemsChangedEventArgs<TOptions, TOptionsItem, TKey>> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            OnItemChange += disposable.OnItemChange;
            return disposable;
        }

        public void Dispose() => _listening?.Dispose();

        internal class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<OptionsItemsChangedEventArgs<TOptions, TOptionsItem, TKey>> _listener;
            private readonly OptionsItemsChangedMonitor<TOptions, TOptionsItem, TKey> _monitor;

            public ChangeTrackerDisposable(OptionsItemsChangedMonitor<TOptions, TOptionsItem, TKey> monitor, Action<OptionsItemsChangedEventArgs<TOptions, TOptionsItem, TKey>> listener)
            {
                _listener = listener;
                _monitor = monitor;
            }

            public void OnItemChange(OptionsItemsChangedEventArgs<TOptions, TOptionsItem, TKey> args) => _listener.Invoke(args);

            public void Dispose() => _monitor.OnItemChange -= OnItemChange;
        }
    }
}
