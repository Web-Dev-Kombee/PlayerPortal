using PlayerPortal.Data.BrokerRequests;
using PlayerPortal.Data.DataTransferModels;
using PlayerPortal.Helpers;
using Shared.Broker;
namespace PlayerPortal.Services
{
    public class PlayerService : IPlayerServices
    {
        private readonly IBroker _broker;

        public PlayerService(IBroker broker)
        {
            _broker = broker;
        }

        public async Task<(List<PlayerDataTransferModel> Players, int TotalCount)> GetAllPlayers(string? searchTerm, int page, int pageSize)
        {
            return await _broker.RequestAsync(new PlayerListBroker((page - 1) * pageSize, pageSize, searchTerm));
        }

        public async Task<PlayerDataTransferModel> GetPlayerById(int id)
        {
            return await _broker.RequestAsync(new GetPlayerByIdBroker(id));
        }

        public async Task<bool> CreatePlayer(PlayerRequest request)
        {
            try
            {
                var result = await _broker.RequestAsync(request);
                if (!result.Success)
                {
                    throw new InvalidOperationException(ErrorHandler.GenerateFailureMessage("create", failureMessage: result.Message));
                }
                return result.Success;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ErrorHandler.GenerateErrorMessage("creating", innerException: ex), ex);
            }
        }

        public async Task<bool> UpdatePlayer(int id, PlayerRequest request)
        {
            try
            {
                request.Id = id;
                var result = await _broker.RequestAsync(request);
                if (!result.Success)
                {
                    throw new InvalidOperationException(ErrorHandler.GenerateFailureMessage("update", playerId: id, failureMessage: result.Message));
                }
                return result.Success;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ErrorHandler.GenerateErrorMessage("updating", playerId: id, innerException: ex), ex);
            }
        }

        public async Task<bool> DeletePlayer(int id)
        {
            try
            {
                var result = await _broker.RequestAsync(new DeletePlayerBroker(id, 1));
                if (!result.Success)
                {
                    throw new InvalidOperationException(ErrorHandler.GenerateFailureMessage("delete", playerId: id, failureMessage: result.Message));
                }
                return result.Success;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ErrorHandler.GenerateErrorMessage("deleting", playerId: id, innerException: ex), ex);
            }
        }

        public PlayerRequest ToPlayerRequest(PlayerDataTransferModel player)
        {
            return new PlayerRequest(
                  name: player.Name,
                  shirtNo: player.ShirtNo,
                  appearance: player.Appearance,
                  goals: player.Goals,
                  actionPerformedBy: 1,
                  id: player.Id
              );
        }
    }
}