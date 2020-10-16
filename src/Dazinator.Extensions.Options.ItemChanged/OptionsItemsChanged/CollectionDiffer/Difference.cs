namespace Dazinator.Extensions.Options.ItemChanged
{
    public class Difference<TItem> : Difference<TItem, TItem>
    {

    }
    public class Difference<TCurrent, TOriginal>
    {
        public TCurrent CurrentItem { get; set; }
        public TOriginal OldItem { get; set; }
        public ItemChangeType ChangeType
        {
            get
            {
                if (CurrentItem == null)
                {
                    return ItemChangeType.Removed;
                }

                if (OldItem == null)
                {
                    return ItemChangeType.Added;
                }

                return ItemChangeType.Modified;
            }
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
