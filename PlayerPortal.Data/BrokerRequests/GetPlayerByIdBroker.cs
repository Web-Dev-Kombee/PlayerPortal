using PlayerPortal.Data.DataTransferModels;
using Shared.Broker;

namespace PlayerPortal.Data.BrokerRequests
{
    public class GetPlayerByIdBroker : IBrokeredAsyncReturns<PlayerDataTransferModel>
    {
        public int PlayerId { get; set; }
        public GetPlayerByIdBroker(int playerId)
        {
            PlayerId = playerId;
        }
    }
}