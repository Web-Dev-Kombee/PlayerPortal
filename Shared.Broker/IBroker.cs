using Shard.Commons;

namespace Shared.Broker
{
    public interface IBroker
    {
        TReturn Request<TReturn>(IBrokeredReturns<TReturn> with);
        Task<TReturn> RequestAsync<TReturn>(IBrokeredAsyncReturns<TReturn> with);
        Task<TReturn> RequestAsync<TReturn>(IBrokeredAsyncReturns<TReturn> with, CancellationToken token);

        void Do<TInput>(TInput with) where TInput : IBrokered;
        Task DoAsync<TInput>(TInput with) where TInput : IBrokeredAsync;
        Task DoAsync<TInput>(TInput with, CancellationToken token) where TInput : IBrokeredAsync;
        void Dispatch<TInput>(TInput notification) where TInput : IBrokeredNotice;
        Task DispatchAsync<TInput>(TInput notification) where TInput : IBrokeredNoticeAsync;
        Task DispatchAsync<TInput>(TInput notification, CancellationToken token) where TInput : IBrokeredNoticeAsync;

        void Run<TData>(TData pipeline) where TData : IBrokeredPipeline;
        Task RunAsync<TData>(TData pipeline) where TData : IBrokeredPipelineAsync;
        Task RunAsync<TData>(TData pipeline, CancellationToken token) where TData : IBrokeredPipelineAsync;

        Result<List<Type>> ValidateHandlers(ITypeScanner typeScanner);
        bool HasHandler<TInput>();
    }
}
