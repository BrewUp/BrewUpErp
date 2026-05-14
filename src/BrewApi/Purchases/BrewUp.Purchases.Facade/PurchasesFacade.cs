using BrewUp.Purchases.Domain;
using BrewUp.Purchases.ReadModel.Services;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Purchases;
using Lena.Core;

namespace BrewUp.Purchases.Facade;

internal sealed class PurchasesFacade(IPurchaseDomainService purchaseDomainService,
    ISupplierService supplierService,
    IBeerService beerService) : IPurchasesFacade
{
    public async Task<Result<string>> CreatePurchaseOrderAsync(CreatePurchaseOrderJson body,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var supplierResult = await supplierService.GetSupplierByIdAsync(new SupplierId(body.SupplierId), cancellationToken);
        supplierResult.TryGetValue(out var supplier);
        
        return Result<string>.Success(string.Empty);
    }
}