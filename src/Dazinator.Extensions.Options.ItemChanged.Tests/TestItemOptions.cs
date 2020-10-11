namespace Dazinator.Extensions.Options.ItemChanged.Tests
{
    using Dazinator.Extensions.Options.ItemChanged;

    public class TestItemOptions : IHaveKey<string>
    {
        public string Key { get; set; }
    }
}
