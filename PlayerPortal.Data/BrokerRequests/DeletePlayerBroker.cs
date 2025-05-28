using Shard.Commons;
using Shared.Broker;

namespace PlayerPortal.Data.BrokerRequests
{
    public class DeletePlayerBroker : IBrokeredAsyncReturns<Result>
    {
        public int PlayerId { get; set; }
        public int ActionPerformedBy { get; set; }
        public DeletePlayerBroker(int playerId, int actionPerformedBy)
        {
            PlayerId = playerId;
            ActionPerformedBy = actionPerformedBy;
        }
    }
}