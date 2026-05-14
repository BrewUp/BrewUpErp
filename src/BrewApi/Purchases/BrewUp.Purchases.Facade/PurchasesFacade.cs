using BrewUp.Shared.ExternalContracts.Purchases;
using Lena.Core;

namespace BrewUp.Purchases.Facade;

internal sealed class PurchasesFacade : IPurchasesFacade
{
    public Task<Result<string>> CreatePurchaseOrderAsync(CreatePurchaseOrderJson body, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}