using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.MasterData;
using BrewUp.Shared.ReadModel;

namespace BrewUp.Purchases.ReadModel.Dtos;

public class Supplier : DtoBase
{
    public string RagioneSociale { get; private set; } = string.Empty;
    public string PartitaIva { get; private set; } = string.Empty;

    private IndirizzoJson Indirizzo { get; set; } = new ();
    
    protected Supplier() 
    { }
    
    public static Supplier
        Create(SupplierId supplierId, RagioneSociale ragioneSociale, PartitaIva partitaIva, IndirizzoJson indirizzo) =>
             new (supplierId.Value, ragioneSociale.Value, partitaIva.Value, indirizzo);
    
    private Supplier(string supplierId, string ragioneSociale, string partitaIva, IndirizzoJson indirizzo)
    {
        Id = supplierId;
        RagioneSociale = ragioneSociale;
        PartitaIva = partitaIva;
        Indirizzo = indirizzo;
    }
}