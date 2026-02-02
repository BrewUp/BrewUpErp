using BrewUp.Shared.ExternalContracts;
using Lena.Core;

namespace BrewUp.Sales.Domain;

public interface ISalesDomainService
{
    Task<Result<string>> CreateSalesOrderAsync(CreateSalesOrderJson body, CancellationToken cancellationToken);
}