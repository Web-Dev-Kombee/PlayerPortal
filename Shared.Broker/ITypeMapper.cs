namespace Shared.Broker
{
    public interface ITypeMapper
    {
        ITypeMapper Add(Type t, object objForT);
        ITypeMapper Add<T>(T objForT);
        ITypeMapper AddDynamic(Func<Type, object> getter, bool ignoreEnumerables);
    }
}
