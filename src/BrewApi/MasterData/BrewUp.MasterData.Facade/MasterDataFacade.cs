using BrewUp.MasterData.Domain;
using BrewUp.MasterData.ReadModel.Services;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.MasterData;
using BrewUp.Shared.ReadModel;
using Lena.Core;

namespace BrewUp.MasterData.Facade;

internal sealed class MasterDataFacade(IMasterDataDomainService masterDataDomainService,
    IMasterDataQueryService masterDataQueryService) : IMasterDataFacade
{
    public Task<Result<string>> CreateCustomerAsync(CreateCustomerJson body, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        return masterDataDomainService.CreateCustomerAsync(new CustomerId(Guid.CreateVersion7().ToString()),
            new RagioneSociale(body.RagioneSociale),
            new PartitaIva(body.PartitaIva),
            new Indirizzo(new Via(body.Indirizzo.Via),
                new NumeroCivico(body.Indirizzo.NumeroCivico),
                new Cap(body.Indirizzo.Cap),
                new Citta(body.Indirizzo.Citta),
                new Provincia(body.Indirizzo.Provincia),
                new Nazione(body.Indirizzo.Nazione)),
            cancellationToken);
    }

    public Task<Result<PagedResult<CustomerJson>>> GetCustomersAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken) =>
        masterDataQueryService.GetCustomersAsync(pageNumber, pageSize, cancellationToken);
}