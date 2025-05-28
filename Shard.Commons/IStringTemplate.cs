namespace Shard.Commons
{
    public interface IStringTemplate
    {
        string Format(SimpleStringTemplate.ArgumentResolver argFactory, IFormatProvider provider = null);
    }
}
