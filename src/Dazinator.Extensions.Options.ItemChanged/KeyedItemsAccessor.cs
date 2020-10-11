namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;
    using System.Collections.Generic;

    public class KeyedItemsAccessor<TInstance, TItem, TKey>
        where TItem: IHaveKey<TKey>
    {
        public string MemberName { get; set; }

        public Func<TInstance, IEnumerable<TItem>> GetItems { get; set; }
    }
}
