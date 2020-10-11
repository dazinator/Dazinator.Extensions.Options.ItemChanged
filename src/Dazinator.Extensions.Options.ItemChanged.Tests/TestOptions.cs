namespace Dazinator.Extensions.Options.ItemChanged.Tests
{
    using System.Collections.Generic;

    public class TestOptions
    {
        public TestOptions()
        {
            Items = new List<TestItemOptions>();
            OtherItems = new List<TestItemOptions>();
        }
        public string Foo { get; }
        public List<TestItemOptions> Items { get; internal set; }
        public List<TestItemOptions> OtherItems { get; internal set; }
    }
}
