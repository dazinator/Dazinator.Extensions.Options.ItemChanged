namespace Dazinator.Extensions.Options.ItemChanged.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CollectionDifferUsingKeyExpression<TItem, TKey> : CollectionDifferUsingKeyExpression<TItem, TItem, TKey> where TItem : class
    {
        public CollectionDifferUsingKeyExpression(Func<TItem, TKey> targetKeySelector) : base(targetKeySelector, targetKeySelector)
        {
        }
    }

    public class CollectionDifferUsingKeyExpression<TCurrentItem, TOriginalItem, TKey> : CollectionDiffer<TCurrentItem, TOriginalItem> where TCurrentItem : class
        where TOriginalItem : class
    {
        private readonly Func<TCurrentItem, TKey> _targetKeySelector;
        private readonly Func<TOriginalItem, TKey> _originalItemsKeySelector;

        private bool _isSameType;

        public CollectionDifferUsingKeyExpression(Func<TCurrentItem, TKey> targetKeySelector, Func<TOriginalItem, TKey> originalItemsKeySelector)
        {
            _targetKeySelector = targetKeySelector;
            _originalItemsKeySelector = originalItemsKeySelector;
            _isSameType = typeof(TCurrentItem) == typeof(TOriginalItem);
        }

        public override IEnumerable<Difference<TCurrentItem, TOriginalItem>> GetChanges(IEnumerable<TCurrentItem> target, IEnumerable<TOriginalItem> original)
        {

            var oldDictionary = original.ToDictionary(_originalItemsKeySelector, a => a);

            //originalOptions.Mappings.ToDictionary(a => a.Key, b => b);

            foreach (var item in target)
            {
                // Is it new?
                var newKey = _targetKeySelector(item);
                if (!oldDictionary.ContainsKey(newKey))
                {
                    yield return new Difference<TCurrentItem, TOriginalItem>() { CurrentItem = item, OldItem = null };
                    continue;
                }

                // it exists but has it changed?
                var previousItem = oldDictionary[newKey];
                oldDictionary.Remove(newKey); // whats left in old, won't be present in new, so therefore is deleted.

                if (!AreSame(item, previousItem))
                {
                    yield return new Difference<TCurrentItem, TOriginalItem>() { CurrentItem = item, OldItem = previousItem };
                    continue;
                }

            }

            foreach (var notFound in oldDictionary)
            {
                yield return new Difference<TCurrentItem, TOriginalItem>() { CurrentItem = null, OldItem = notFound.Value };
            }
        }

        protected virtual bool AreSame(TCurrentItem item, TOriginalItem previousItem)
        {
            if (_isSameType)
            {
                var forCompare = previousItem as TCurrentItem;
                return item == forCompare;
            }
            // objects of different types are inherently different.
            return false;
        }
    }

    //public static class EnumerableExtensions
    //{
    //    public IEnumerable<Tuple<TItem, ItemChangeType>> GetChanges(IEnumerable<TItem> originalOptions, TOptions newOptions, Func<TOptions, IEnumerable<TOptionsItem>> itemsAccessor)
    //    {
    //        var oldItems = itemsAccessor(originalOptions);
    //        var oldDictionary = oldItems.ToDictionary(a => a.Key, b => b);

    //        //originalOptions.Mappings.ToDictionary(a => a.Key, b => b);
    //        var newItems = itemsAccessor(newOptions);
    //        foreach (var item in newItems)
    //        {
    //            // Is it new?
    //            if (!oldDictionary.ContainsKey(item.Key))
    //            {
    //                yield return new Tuple<TOptionsItem, ItemChangeType>(item, ItemChangeType.Added);
    //                continue;
    //            }

    //            // it exists but has it changed?
    //            var previousItem = oldDictionary[item.Key];
    //            oldDictionary.Remove(item.Key); // whats left in old, won't be present in new, so therefore is deleted.

    //            if (previousItem != item) // user can override .Equals() to ascertain if the two instances should b seen as equal
    //            {
    //                yield return new Tuple<TOptionsItem, ItemChangeType>(item, ItemChangeType.Modified);
    //                continue;
    //            }
    //        }

    //        foreach (var notFound in oldDictionary)
    //        {
    //            yield return new Tuple<TOptionsItem, ItemChangeType>(notFound.Value, ItemChangeType.Removed);
    //        }
    //    }
    //}
}
