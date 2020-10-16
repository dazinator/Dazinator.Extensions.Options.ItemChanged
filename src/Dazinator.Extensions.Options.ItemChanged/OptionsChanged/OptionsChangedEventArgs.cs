namespace Dazinator.Extensions.Options.ItemChanged
{
    using System;

    public class OptionsChangedEventArgs<TOptions> : EventArgs
    {
        public OptionsChangedEventArgs()
        {
        }

        public TOptions Old { get; set; }

        public TOptions Current { get; set; }

    }
}
