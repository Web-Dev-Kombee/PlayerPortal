using PlayerPortal.Data.BrokerRequests;
using PlayerPortal.Data.DataTransferModels;

namespace PlayerPortal.Services
{
    public interface IPlayerServices
    {
        Task<(List<PlayerDataTransferModel> Players, int TotalCount)> GetAllPlayers(string? searchTerm, int page, int pageSize);
        Task<PlayerDataTransferModel> GetPlayerById(int id);
        Task<bool> CreatePlayer(PlayerRequest request);
        Task<bool> UpdatePlayer(int id, PlayerRequest request);
        Task<bool> DeletePlayer(int id);
        PlayerRequest ToPlayerRequest(PlayerDataTransferModel player);
    }
}
