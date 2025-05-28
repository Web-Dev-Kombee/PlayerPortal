using Shared.Broker.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Broker
{
    public interface IBrokeredHandler<in TInput> : IBrokeredSingleMapping<TInput> where TInput : IBrokered
    {
        void Handle(TInput input);
    }

    public interface IBrokeredAsyncHandler<in TInput> : IBrokeredSingleMapping<TInput> where TInput : IBrokeredAsync
    {
        Task Handle(TInput input, CancellationToken token);
    }

    public interface IBrokeredHandler<in TInput, out TReturn> : IBrokeredSingleMapping<TInput> where TInput : IBrokeredReturns<TReturn>
    {
        TReturn Handle(TInput input);
    }

    public interface IBrokeredAsyncHandler<in TInput, TReturn> : IBrokeredSingleMapping<TInput> where TInput : IBrokeredAsyncReturns<TReturn>
    {
        Task<TReturn> Handle(TInput input, CancellationToken token);
    }
}
