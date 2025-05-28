using Shard.Commons;
using Shared.Broker;

namespace PlayerPortal.Data.BrokerRequests
{
    public class PlayerRequest : IBrokeredAsyncReturns<Result>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ShirtNo { get; set; }
        public int Appearance { get; set; }
        public int Goals { get; set; }
        public int ActionPerformedBy { get; set; }

        public PlayerRequest() { }

        public PlayerRequest(string name, int shirtNo, int appearance, int goals, int actionPerformedBy, int id = 0)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ShirtNo = shirtNo;
            Appearance = appearance;
            Goals = goals;
            ActionPerformedBy = actionPerformedBy;
            Id = id;
        }
    }
}