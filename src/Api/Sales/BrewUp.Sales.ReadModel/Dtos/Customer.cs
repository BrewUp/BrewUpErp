using BrewUp.Shared.ExternalContracts.MasterData;
using BrewUp.Shared.Helpers;
using BrewUp.Shared.ReadModel;

namespace BrewUp.Sales.ReadModel.Dtos;

public class Customer : DtoBase
{
    public string RagioneSociale { get; private set; } = string.Empty;
    public string PartitaIva { get; private set; } = string.Empty;
    
    public string ConsumerLevel { get; private set; } = string.Empty;
    
    public IndirizzoJson Indirizzo { get; private set; } = new ();
    
    protected Customer() 
    { }

    public static Customer
        Create(string customerId, string ragioneSociale, string partitaIva, IndirizzoJson indirizzo) =>
            new (customerId, ragioneSociale, partitaIva, indirizzo);

    private Customer(string customerId, string ragioneSociale, string partitaIva, IndirizzoJson indirizzo)
    {
        Id = customerId;
        RagioneSociale = ragioneSociale;
        PartitaIva = partitaIva;

        ConsumerLevel = BeerConsumerLevel.Teetotaler.Name;

        Indirizzo = indirizzo;
    }
}