using Shared.Broker.Internal;

namespace Shared.Broker
{
    public interface IBrokeredProvider<in TInput, TReturn> : IBrokeredManyMapping<TInput> where TInput : IBrokeredReturns<TReturn>
    {
        int OrderPreference { get; }
        Provided<TReturn> Provide(TInput input);
    }

    public interface IBrokeredAsyncProvider<in TInput, TReturn> : IBrokeredManyMapping<TInput> where TInput : IBrokeredAsyncReturns<TReturn>
    {
        int OrderPreference { get; }
        Task<Provided<TReturn>> Provide(TInput input, CancellationToken token);
    }
}
