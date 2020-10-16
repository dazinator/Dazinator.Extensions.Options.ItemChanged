namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;
    using System.Collections.Generic;

    public class OptionsItemsChangedEventArgs<TKey, TOptionsItem> : EventArgs
    {
        public OptionsItemsChangedEventArgs()
        {
        }
        public string MemberName { get; internal set; }

        public HashSet<Difference<TOptionsItem, TOptionsItem>> Differences { get; set; }

    }
}
