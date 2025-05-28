namespace Shared.Broker.Internal
{
    public interface ICaller<TReturn>
    {
        TReturn Handle(IBrokeredReturns<TReturn> input, object handler);
    }

    public class Caller<TInput, TReturn> : ICaller<TReturn> where TInput : IBrokeredReturns<TReturn>
    {
        public TReturn Handle(IBrokeredReturns<TReturn> input, object handler)
        {
            if (handler is IBrokeredHandler<TInput, TReturn>)
                return (handler as IBrokeredHandler<TInput, TReturn>).Handle((TInput)input);
            else
            {
                var providers = (handler as IList<object>).Cast<IBrokeredProvider<TInput, TReturn>>();
                foreach (var provider in providers.OrderBy(v => v.OrderPreference))
                {
                    var provided = provider.Provide((TInput)input);
                    if (provided) return provided.Result;
                }
                if (!(input is IBrokeredHandlerIsOptional)) throw new InvalidOperationException($"The question {typeof(TInput).Name} was not answered by any registered providers.");
                else return default(TReturn);
            }
        }
    }
}
