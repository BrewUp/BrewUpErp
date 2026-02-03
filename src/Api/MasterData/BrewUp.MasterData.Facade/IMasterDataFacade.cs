using BrewUp.Shared.ExternalContracts.MasterData;
using Lena.Core;

namespace BrewUp.MasterData.Facade;

public partial interface IMasterDataFacade
{
    Task<Result<string>> CreateCustomerAsync(CreateCustomerJson body, CancellationToken cancellationToken);
}