using BrewSpa.Purchases.Application.Models;
using Lena.Core;

namespace BrewSpa.Purchases.Application.Services;

public interface IPurchaseService
{
    Task<Result<bool>> CreatePurchaseOrderAsync(CreatePurchaseOrderJson purchaseOrder);
}
