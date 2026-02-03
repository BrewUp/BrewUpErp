using BrewUp.Shared.ExternalContracts.MasterData;
using Lena.Core;

namespace BrewUp.MasterData.Facade;

internal sealed class MasterDataFacade : IMasterDataFacade
{
    public Task<Result<string>> CreateCustomerAsync(CreateCustomerJson body, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}