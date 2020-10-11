namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    public class OptionsItemChangesMonitor<TKey, TOptions, TOptionsItem> : IDisposable, IOptionsItemChangesMonitor<TKey, TOptionsItem> where TOptionsItem : class, IHaveKey<TKey>
    {
        private readonly ILogger<OptionsItemChangesMonitor<TKey, TOptions, TOptionsItem>> _logger;
        private readonly List<KeyedItemsAccessor<TOptions, TOptionsItem, TKey>> _itemAccessors;
        private readonly IDisposable _listening;

        internal event Action<OptionsItemChangedEventArgs<TKey, TOptionsItem>> OnItemChange;
        private readonly OptionsItemsDiffer<TKey, TOptions, TOptionsItem> _differ;
        private TOptions _instance;

        //  private readonly string _memberName;


        public OptionsItemChangesMonitor(
            IOptionsMonitor<TOptions> optionsMonitor,
            ILogger<OptionsItemChangesMonitor<TKey, TOptions, TOptionsItem>> logger,
            IEnumerable<KeyedItemsAccessor<TOptions, TOptionsItem, TKey>> itemAccessors
           )
        {
            // _optionsMonitor = optionsMonitor;
            _instance = optionsMonitor.CurrentValue;
            _logger = logger;
            _itemAccessors = itemAccessors.ToList();


            //  _itemsAccessor = itemsAccessor;
            _differ = new OptionsItemsDiffer<TKey, TOptions, TOptionsItem>();

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
                var changes = _differ.GetChanges(_instance, newInstance, itemsEnumerable.GetItems).ToList();
                if(!changes.Any())
                {
                    continue;
                }

                var changeDictionary = new Dictionary<ItemChangeType, List<TOptionsItem>>();
                var adds = new List<TOptionsItem>();
                var updates = new List<TOptionsItem>();
                var deletes = new List<TOptionsItem>();

                changeDictionary.Add(ItemChangeType.Added, adds);
                changeDictionary.Add(ItemChangeType.Modified, updates);
                changeDictionary.Add(ItemChangeType.Removed, deletes);

                foreach (var item in changes)
                {
                    switch (item.Item2)
                    {
                        case ItemChangeType.Added:
                            adds.Add(item.Item1);
                            continue;
                        case ItemChangeType.Modified:
                            updates.Add(item.Item1);
                            continue;
                        case ItemChangeType.Removed:
                            deletes.Add(item.Item1);
                            continue;
                    }
                }               

                var args = new OptionsItemChangedEventArgs<TKey, TOptionsItem>()
                {
                    MemberName = itemsEnumerable.MemberName,
                    Changes = changeDictionary
                };

                InvokeChanged(args);
            }
            _instance = newInstance;
        }


        private void InvokeChanged(OptionsItemChangedEventArgs<TKey, TOptionsItem> args)
        {
            if (OnItemChange != null)
            {
                OnItemChange?.Invoke(args);
            }
        }

        /// <summary>
        /// Registers a listener to be called whenever a mapping item on <see cref="MappingOptions{TKey, TOptions}"/> changes.
        /// </summary>
        /// <param name="listener">The action to be invoked when whenever a mapping item on <see cref="MappingOptions{TKey, TOptions}"/> changes.</param>
        /// <returns>An <see cref="IDisposable"/> which should be disposed to stop listening for changes.</returns>
        public IDisposable OnChange(Action<OptionsItemChangedEventArgs<TKey, TOptionsItem>> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            OnItemChange += disposable.OnItemChange;
            return disposable;
        }

        public void Dispose() => _listening?.Dispose();

        internal class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<OptionsItemChangedEventArgs<TKey, TOptionsItem>> _listener;
            private readonly OptionsItemChangesMonitor<TKey, TOptions, TOptionsItem> _monitor;

            public ChangeTrackerDisposable(OptionsItemChangesMonitor<TKey, TOptions, TOptionsItem> monitor, Action<OptionsItemChangedEventArgs<TKey, TOptionsItem>> listener)
            {
                _listener = listener;
                _monitor = monitor;
            }

            public void OnItemChange(OptionsItemChangedEventArgs<TKey, TOptionsItem> args) => _listener.Invoke(args);

            public void Dispose() => _monitor.OnItemChange -= OnItemChange;
        }
    }
}
