using Shared.Broker.Internal;

namespace Shared.Broker
{
    public interface IBrokeredNoticeReceiver<in TInput> : IBrokeredManyMapping<TInput> where TInput : IBrokeredNotice
    {
        void Receive(TInput input);
    }

    public interface IBrokeredNoticeAsyncReceiver<in TInput> : IBrokeredManyMapping<TInput> where TInput : IBrokeredNoticeAsync
    {
        Task Receive(TInput input, CancellationToken token);
    }
}
