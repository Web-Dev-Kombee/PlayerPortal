using PlayerPortal.Data.DataTransferModels;
using Shared.Broker;

namespace PlayerPortal.Data.BrokerRequests
{
    public class PlayerListBroker : IBrokeredAsyncReturns<(List<PlayerDataTransferModel> Players, int TotalCount)>
    {
        public string? Search { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public PlayerListBroker(int offset, int limit, string? search = null)
        {
            Offset = offset;
            Limit = limit;
            Search = search;
        }
    }
}

