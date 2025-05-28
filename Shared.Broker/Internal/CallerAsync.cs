namespace Shared.Broker.Internal
{
    public interface IAsyncCaller<TReturn>
    {
        Task<TReturn> Handle(IBrokeredAsyncReturns<TReturn> input, object handler, CancellationToken token);
    }

    public class AsyncCaller<TInput, TReturn> : IAsyncCaller<TReturn> where TInput : IBrokeredAsyncReturns<TReturn>
    {
        public async Task<TReturn> Handle(IBrokeredAsyncReturns<TReturn> input, object handler, CancellationToken token)
        {
            if (handler is IBrokeredAsyncHandler<TInput, TReturn>)
                return await (handler as IBrokeredAsyncHandler<TInput, TReturn>).Handle((TInput)input, token);
            else
            {
                var providers = (handler as IList<object>).Cast<IBrokeredAsyncProvider<TInput, TReturn>>();
                foreach (var provider in providers.OrderBy(v => v.OrderPreference))
                {
                    token.ThrowIfCancellationRequested();
                    var provided = await provider.Provide((TInput)input, token);
                    if (provided) return provided.Result;
                }
                if (!(input is IBrokeredHandlerIsOptional)) throw new InvalidOperationException($"The question {typeof(TInput).Name} was not answered by any registered providers.");
                else return default(TReturn);
            }
        }
    }
}
