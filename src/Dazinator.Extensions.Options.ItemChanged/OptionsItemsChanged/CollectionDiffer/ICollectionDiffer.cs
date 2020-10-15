namespace Dazinator.Extensions.Options.ItemChanged
{
    using System.Collections.Generic;

    public interface ICollectionDiffer<TCurrentItem, TOriginalItem>
        where TCurrentItem : class
        where TOriginalItem : class
    {
        IEnumerable<Difference<TCurrentItem, TOriginalItem>> GetChanges(IEnumerable<TCurrentItem> target, IEnumerable<TOriginalItem> original);
    }
}
