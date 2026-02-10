using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using Lena.Core;

namespace BrewUp.MasterData.Domain;

public interface IMasterDataDomainService
{
    Task<Result<string>> CreateCustomerAsync(CustomerId customerId, RagioneSociale ragioneSociale,
        PartitaIva partitaIva, Indirizzo indirizzo, CancellationToken cancellationToken = default);

    Task<Result<bool>> SaveCustomerAsync(CustomerId customerId, RagioneSociale ragioneSociale, PartitaIva partitaIva,
        Indirizzo indirizzo, CancellationToken cancellationToken);

    Task<Result<bool>> DeleteCustomerAsync(CustomerId customerId, CancellationToken cancellationToken);
}