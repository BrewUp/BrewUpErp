using BrewUp.MasterData.Domain.ValueObjects;
using BrewUp.Shared.DomainIds;
using Lena.Core;

namespace BrewUp.MasterData.Domain;

public interface IMasterDataDomainService
{
    Task<Result<string>> CreateCustomerAsync(CustomerId customerId, RagioneSociale ragioneSociale,
        PartitaIva partitaIva, Indirizzo indirizzo, CancellationToken cancellationToken = default);
}