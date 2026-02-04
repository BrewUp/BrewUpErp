using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.DomainIds;
using Muflone.Messages.Events;

namespace BrewUp.Sales.SharedKernel.Messages.Events;

public sealed class CustomerCreated(CustomerId aggregateId, 
    RagioneSociale ragioneSociale,
    PartitaIva partitaIva,
    ConsumerLevel consumerLevel,
    Indirizzo indirizzo) : IntegrationEvent(aggregateId)
{
    public RagioneSociale RagioneSociale => ragioneSociale;
    public PartitaIva PartitaIva => partitaIva;
    public ConsumerLevel ConsumerLevel => consumerLevel;
    public Indirizzo Indirizzo => indirizzo;
}