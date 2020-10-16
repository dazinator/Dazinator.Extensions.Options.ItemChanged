namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;

    public class OptionsChangedEventArgs<TOptionsItem> : EventArgs
    {
        public OptionsChangedEventArgs()
        {
        }

        public TOptionsItem Old { get; set; }

        public TOptionsItem Current { get; set; }

    }
}
