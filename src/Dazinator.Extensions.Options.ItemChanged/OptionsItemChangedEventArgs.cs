namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;
    using System.Collections.Generic;

    public class OptionsItemChangedEventArgs<TKey, TOptionsItem> : EventArgs
    {
        public OptionsItemChangedEventArgs()
        {
        }
        public string MemberName { get; internal set; }
        public Dictionary<ItemChangeType, List<TOptionsItem>> Changes { get; set; }

    }
}
