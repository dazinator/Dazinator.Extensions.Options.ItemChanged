namespace Dazinator.Extensions.Options.ItemChanged
{
    using System.Collections.Generic;

    public abstract class CollectionDiffer<TCurrentItem, TOriginalItem> : ICollectionDiffer<TCurrentItem, TOriginalItem>
        where TCurrentItem : class
        where TOriginalItem : class
    {
        public abstract IEnumerable<Difference<TCurrentItem, TOriginalItem>> GetChanges(IEnumerable<TCurrentItem> target, IEnumerable<TOriginalItem> original);
        

        protected Difference<TCurrentItem, TOriginalItem> CreateAddedItemDifference(TCurrentItem newItem) => new Difference<TCurrentItem, TOriginalItem>() { CurrentItem = newItem, OldItem = null };

        protected Difference<TCurrentItem, TOriginalItem> CreateModifiedItemDifference(TCurrentItem newItem, TOriginalItem oldItem) => new Difference<TCurrentItem, TOriginalItem>() { CurrentItem = newItem, OldItem = oldItem };

        protected Difference<TCurrentItem, TOriginalItem> CreateRemovedItemDifference(TOriginalItem oldItem) => new Difference<TCurrentItem, TOriginalItem>() { CurrentItem = null, OldItem = oldItem };

    }
}

