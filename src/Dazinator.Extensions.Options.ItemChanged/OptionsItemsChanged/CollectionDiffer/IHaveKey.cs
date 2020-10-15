namespace Dazinator.Extensions.Options.ItemChanged
{
    public interface IHaveKey<TKey>
    {
        TKey Key { get; set; }
    }
}
