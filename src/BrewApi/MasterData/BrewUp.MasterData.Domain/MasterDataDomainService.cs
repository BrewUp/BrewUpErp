using BrewUp.MasterData.Domain.Helpers;
using BrewUp.MasterData.SharedKernel.Dtos;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ReadModel;
using Lena.Asyncs;
using Lena.Core;
using Microsoft.Extensions.DependencyInjection;
using Muflone;
using Muflone.Messages.Events;

namespace BrewUp.MasterData.Domain;

internal sealed class MasterDataDomainService([FromKeyedServices("masterdata")] IPersister persister,
    IEventBus eventBus) : IMasterDataDomainService
{
    public async Task<Result<string>> CreateCustomerAsync(CustomerId customerId, RagioneSociale ragioneSociale, PartitaIva partitaIva,
        Indirizzo indirizzo, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        Customer customer = Customer.Create(customerId, ragioneSociale, partitaIva,
            indirizzo.ToIndirizzoJson());

        // Railway-Oriented Programming pattern
        return (await (await persister.InsertAsync(customer, cancellationToken))
                .BindAsync(_ => SentIntegrationEventAsync(customer.ToCustomerCreated(), cancellationToken)))
            .Match(
                _ => Result<string>.Success(customerId.Value),
                Result<string>.Error);
    }

    public async Task<Result<bool>> SaveCustomerAsync(CustomerId customerId, RagioneSociale ragioneSociale, PartitaIva partitaIva, Indirizzo indirizzo,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var customerResult = await persister.GetByIdAsync<Customer>(customerId.Value, cancellationToken);
        if (!customerResult.IsSuccess) 
            return Result<bool>.Error($"Failed to save customer: {customerId.Value}");
        
        customerResult.TryGetValue(out Customer customer);
        customer.UpdateRagioneSociale(ragioneSociale);
        customer.UpdatePartitaIva(partitaIva);
        customer.UpdateIndirizzo(indirizzo.ToIndirizzoJson());
        
        return (await (await persister.UpdateAsync(customer, cancellationToken))
                .BindAsync(_ => SentIntegrationEventAsync(customer.ToCustomerUpdated(), cancellationToken)))
            .Match(
                _ => Result<bool>.Success(true),
                Result<bool>.Error);
    }

    public async Task<Result<bool>> DeleteCustomerAsync(CustomerId customerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var customerResult = await persister.GetByIdAsync<Customer>(customerId.Value, cancellationToken);
        if (!customerResult.IsSuccess) 
            return Result<bool>.Error($"Failed to delete customer: {customerId.Value}");
        
        customerResult.TryGetValue(out Customer customer);
        return (await (await persister.DeleteAsync(customer, cancellationToken))
                .BindAsync(_ => SentIntegrationEventAsync(customer.ToCustomerDeleted(), cancellationToken)))
            .Match(
                _ => Result<bool>.Success(true),
                Result<bool>.Error);
    }

    private async Task<Result<bool>> SentIntegrationEventAsync(IntegrationEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await eventBus.PublishAsync(@event, cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error($"Failed to send integration event: {ex.Message}");
        }
    }
}