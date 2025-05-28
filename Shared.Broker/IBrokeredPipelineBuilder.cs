using Shared.Broker.Internal;

namespace Shared.Broker
{
    public interface IBrokeredPipelineBuilder<in TData> : IBrokeredManyMapping<TData> where TData : IBrokeredPipeline
    {
        int OrderPreference { get; }
        PipelineExecution Build(TData data);
    }

    public interface IBrokeredPipelineAsyncBuilder<in TData> : IBrokeredManyMapping<TData> where TData : IBrokeredPipelineAsync
    {
        int OrderPreference { get; }
        Task<PipelineExecution> Build(TData data, CancellationToken token);
    }
}
