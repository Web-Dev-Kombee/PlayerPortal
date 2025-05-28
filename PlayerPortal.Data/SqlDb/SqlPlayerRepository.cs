using Microsoft.EntityFrameworkCore;
using PlayerPortal.Data.BrokerRequests;
using PlayerPortal.Data.DataTransferModels;
using PlayerPortal.Data.Extensions;
using PlayerPortal.Data.Infrastructure;
using PlayerPortal.Data.Infrastructure.Tables;
using Shard.Commons;
using Shared.Broker;

namespace PlayerPortal.Data.SqlDb
{
    public class SqlPlayerRepository : SqlDbRepository<EMDboDBContext>,
        IBrokeredAsyncHandler<PlayerListBroker, (List<PlayerDataTransferModel> Players, int TotalCount)>,
        IBrokeredAsyncHandler<GetPlayerByIdBroker, PlayerDataTransferModel>,
        IBrokeredAsyncHandler<PlayerRequest, Result>,
        IBrokeredAsyncHandler<DeletePlayerBroker, Result>
    {
        public SqlPlayerRepository(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public Task<(List<PlayerDataTransferModel> Players, int TotalCount)> Handle(PlayerListBroker input, CancellationToken token) => GetAllPlayers(input.Search, input.Offset, input.Limit);

        public async Task<PlayerDataTransferModel> Handle(GetPlayerByIdBroker input, CancellationToken token) => await GetPlayerById(input.PlayerId) ?? throw new Exception("Player not found.");
        public Task<Result> Handle(PlayerRequest input, CancellationToken token) => Task.FromResult(CreateOrUpdate(input.Id, input.ShirtNo, input.Name, input.Appearance, input.Goals, input.ActionPerformedBy));

        public Task<Result> Handle(DeletePlayerBroker input, CancellationToken token) => Task.FromResult(Delete(input.PlayerId, input.ActionPerformedBy));

        public async Task<PlayerDataTransferModel?> GetPlayerById(int id)
        {
            using var scope = GetScope();
            var player = await scope.Db.Players.FirstOrDefaultAsync(p => p.Id == id);
            if (player == null)
                return null;
            return new PlayerDataTransferModel(player.Id, player.ShirtNo, player.Name, player.Appearance, player.Goals);
        }

        private async Task<(List<PlayerDataTransferModel> Players, int TotalCount)> GetAllPlayers(string? search, int offset, int limit)
        {
            using var scope = GetScope();
            var query = scope.Db.Players.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                if (int.TryParse(search, out int shirtNo))
                {
                    query = query.Where(p => p.ShirtNo == shirtNo || p.Name.ToLower().Contains(search.ToLower()));
                }
                else
                {
                    query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
                }
            }

            var totalCount = await query.CountAsync();
            var players = await query
                .OrderByDescending(s => s.CreatedOn)
                .Skip(offset)
                .Take(limit)
                .Select(p => new PlayerDataTransferModel(p.Id, p.ShirtNo, p.Name, p.Appearance, p.Goals))
                .ToListAsync();

            return (players, totalCount);
        }

        private Result CreateOrUpdate(int id, int shirtNo, string name, int appearance, int goals, int actionPerformedBy)
        {
            using var scope = GetScope();
            PlayerTable player;

            if (id == 0)
            {
                player = new PlayerTable
                {
                    CreatedOn = DateTimeOffset.Now,
                    CreatedBy = actionPerformedBy
                };
                scope.Db.Players.Add(player);
            }
            else
            {
                player = scope.Db.Players.FirstOrDefault(p => p.Id == id);
                if (player == null)
                    return new Result(false, "Player does not exist.");

                player.UpdatedOn = DateTimeOffset.Now;
                player.UpdatedBy = actionPerformedBy;
            }

            player.ShirtNo = shirtNo;
            player.Name = name;
            player.Appearance = appearance;
            player.Goals = goals;

            return scope.Db.SaveChangesWithResult(player);
        }

        private Result Delete(int id, int actionPerformedBy)
        {
            using var scope = GetScope();
            var player = scope.Db.Players.FirstOrDefault(p => p.Id == id);
            if (player == null)
                return new Result(false, "Player not found.");

            scope.Db.Players.Remove(player);
            return scope.Db.SaveChangesWithResult(player);
        }
    }
}