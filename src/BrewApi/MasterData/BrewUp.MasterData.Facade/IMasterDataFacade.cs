using BrewUp.Shared.ExternalContracts.MasterData;
using BrewUp.Shared.ReadModel;
using Lena.Core;

namespace BrewUp.MasterData.Facade;

public interface IMasterDataFacade
{
    Task<Result<string>> CreateCustomerAsync(CreateCustomerJson body, CancellationToken cancellationToken);

    Task<Result<PagedResult<CustomerJson>>> GetCustomersAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken);
}