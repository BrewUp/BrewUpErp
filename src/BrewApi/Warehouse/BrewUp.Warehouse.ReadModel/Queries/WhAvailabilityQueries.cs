using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.ReadModel.Dtos;
using Lena.Core;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace BrewUp.Warehouse.ReadModel.Queries
{
    internal sealed class WhAvailabilityQueries(IMongoClient mongoClient) : IQueries<WhAvailabilityDto>
    {
        private readonly IMongoDatabase _database = mongoClient.GetDatabase("Warehouse");

        public async Task<Result<WhAvailabilityDto>> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var collection = _database.GetCollection<WhAvailabilityDto>(nameof(WhAvailabilityDto));
            var filter = Builders<WhAvailabilityDto>.Filter.Eq("_id", id);

            return await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken) > 0
                ? Result<WhAvailabilityDto>.Success((await collection.FindAsync(filter, cancellationToken: cancellationToken)).First(cancellationToken: cancellationToken))
                : Result<WhAvailabilityDto>.Success(ConstructAggregate<WhAvailabilityDto>());
        }

        public async Task<Result<PagedResult<WhAvailabilityDto>>> GetByFilterAsync(Expression<Func<WhAvailabilityDto, bool>>? query, int page, int pageSize, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (--page < 0)
                page = 0;

            var collection = _database.GetCollection<WhAvailabilityDto>(nameof(WhAvailabilityDto));
            var queryable = query != null
                ? collection.AsQueryable()
                    .Where(query)
                : collection.AsQueryable();

            var count = await queryable.CountAsync(cancellationToken: cancellationToken);
            var results = await queryable.Skip(page * pageSize).Take(pageSize)
                .ToListAsync(cancellationToken: cancellationToken);

            return Result<PagedResult<WhAvailabilityDto>>.Success(new PagedResult<WhAvailabilityDto>(results, page, pageSize, count));
        }

        private static TAggregate ConstructAggregate<TAggregate>()
        {
            return (TAggregate)Activator.CreateInstance(typeof(TAggregate), true)!;
        }
    }
}
