using BrewUp.MasterData.Domain.Helpers;
using BrewUp.MasterData.SharedKernel.CustomTypes;
using BrewUp.MasterData.SharedKernel.Dtos;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ReadModel;
using Lena.Asyncs;
using Lena.Core;
using Microsoft.Extensions.DependencyInjection;
using Muflone;

namespace BrewUp.MasterData.Domain;

internal sealed class MasterDataDomainService([FromKeyedServices("masterdata")] IPersister persister,
    IEventBus eventBus) : IMasterDataDomainService
{
    public async Task<Result<string>> CreateCustomerAsync(CustomerId customerId, RagioneSociale ragioneSociale, PartitaIva partitaIva,
        Indirizzo indirizzo, CancellationToken cancellationToken = default)
    {
        Customer customer = Customer.Create(customerId.Value, ragioneSociale.Value, partitaIva.Value,
            indirizzo.ToIndirizzoJson());

        // Railway-Oriented Programming pattern
        return (await (await persister.InsertAsync(customer, cancellationToken))
            .BindAsync(_ => SentIntegrationEventAsync(customer, cancellationToken)))
            .Match(
                _ => Result<string>.Success(customerId.Value),
                Result<string>.Error
            );
    }
    
    private async Task<Result<bool>> SentIntegrationEventAsync(Customer customer, CancellationToken cancellationToken)
    {
        try
        {
            await eventBus.PublishAsync(customer.ToCustomerCreated(), cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error($"Failed to send integration event: {ex.Message}");
        }
    }
}