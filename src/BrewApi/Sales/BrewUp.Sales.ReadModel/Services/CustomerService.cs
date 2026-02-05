using BrewUp.Sales.ReadModel.Dtos;
using BrewUp.Sales.ReadModel.Helpers;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.Helpers;
using BrewUp.Shared.ReadModel;
using Lena.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrewUp.Sales.ReadModel.Services;

internal sealed class CustomerService (ILoggerFactory loggerFactory, 
    [FromKeyedServices("sales")] IPersister persister) 
    : ServiceBase(loggerFactory, persister),ICustomerService
{
    public async Task<Result<bool>> AddCustomerAsync(CustomerId customerId, RagioneSociale ragioneSociale, PartitaIva partitaIva,
        BeerConsumerLevel consumerLevel, Indirizzo indirizzo, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var customer = Customer.Create(customerId, ragioneSociale, partitaIva, consumerLevel, indirizzo.ToIndirizzoJson());
        var insertResult = await Persister.InsertAsync(customer, cancellationToken);

        return insertResult.Match(
            _ => Result<bool>.Success(true),
            error =>
            {
                Logger.LogError("Error creating customer: {Error}", error);
                return Result<bool>.Error($"Error creating customer: {error}");
            });
    }
}