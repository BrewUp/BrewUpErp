using BrewUp.MasterData.Domain.Helpers;
using BrewUp.MasterData.Domain.ValueObjects;
using BrewUp.MasterData.SharedKernel.Dtos;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ReadModel;
using Lena.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BrewUp.MasterData.Domain;

internal sealed class MasterDataDomainService([FromKeyedServices("masterdata")] IPersister persister) : IMasterDataDomainService
{
    public async Task<Result<string>> CreateCustomerAsync(CustomerId customerId, RagioneSociale ragioneSociale, PartitaIva partitaIva,
        Indirizzo indirizzo, CancellationToken cancellationToken = default)
    {
        Customer customer = Customer.Create(customerId.Value, ragioneSociale.Value, partitaIva.Value,
            indirizzo.ToIndirizzoJson());

        var insertResult = await persister.InsertAsync(customer, cancellationToken);
        
        return insertResult.Match(
            _ => Result<string>.Success(customerId.Value),
            error => Result<string>.Error(error.Message)
        );
    }
}