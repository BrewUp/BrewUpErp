using System.ComponentModel.DataAnnotations;
using BrewUp.Shared.ExternalContracts.MasterData.Customers;

namespace BrewUp.Shared.ExternalContracts.MasterData.Suppliers;

public class CreateSupplierJson
{
    [Required]
    public string RagioneSociale { get; set; } = string.Empty;
    [Required]
    public string PartitaIva { get; set; } = string.Empty;
    
    public IndirizzoJson Indirizzo { get; set; } = new ();
}