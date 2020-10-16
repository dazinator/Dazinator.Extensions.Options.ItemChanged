namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CollectionDifferUsingInterface<TItem, TKey> : CollectionDiffer<TItem, TItem>
   where TItem : class, IHaveKey<TKey>
    {

        public override IEnumerable<Difference<TItem, TItem>> GetChanges(IEnumerable<TItem> target, IEnumerable<TItem> original)
        {
            var oldItems = original;
            var oldDictionary = oldItems.ToDictionary(a => a.Key, b => b);

            //originalOptions.Mappings.ToDictionary(a => a.Key, b => b);
            var newItems = target;
            foreach (var item in newItems)
            {
                // Is it new?
                if (!oldDictionary.ContainsKey(item.Key))
                {
                    yield return new Difference<TItem, TItem>() { CurrentItem = item, OldItem = null };
                    continue;
                }

                // it exists but has it changed?
                var previousItem = oldDictionary[item.Key];
                oldDictionary.Remove(item.Key); // whats left in old, won't be present in new, so therefore is deleted.

                if (previousItem != item) // user can override .Equals() to ascertain if the two instances should b seen as equal
                {
                    yield return new Difference<TItem, TItem>() { CurrentItem = item, OldItem = previousItem };
                    continue;
                }
            }

            foreach (var notFound in oldDictionary)
            {
                yield return new Difference<TItem, TItem>() { CurrentItem = null, OldItem = notFound.Value };
            }
        }
    }
}

