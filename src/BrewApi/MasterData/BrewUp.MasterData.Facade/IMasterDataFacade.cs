using BrewUp.Shared.ExternalContracts.MasterData;
using BrewUp.Shared.ReadModel;
using Lena.Core;

namespace BrewUp.MasterData.Facade;

public interface IMasterDataFacade
{
    Task<Result<string>> CreateCustomerAsync(CreateCustomerJson body, CancellationToken cancellationToken);

    Task<Result<PagedResult<CustomerJson>>> GetCustomersAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken);

    Task<Result<CustomerJson>> GetCustomerByIdAsync(string customerId, CancellationToken cancellationToken);
    Task<Result<bool>> SaveCustomerAsync(EditCustomerJson body, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteCustomerAsync(string customerId, CancellationToken cancellationToken);
}