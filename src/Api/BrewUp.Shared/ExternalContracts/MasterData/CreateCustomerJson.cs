using System.ComponentModel.DataAnnotations;

namespace BrewUp.Shared.ExternalContracts.MasterData;

public class CreateCustomerJson
{
    [Required]
    public string RagioneSociale { get; set; } = string.Empty;
    [Required]
    public string PartitaIva { get; set; } = string.Empty;
    [Required]
    public string CodiceFiscale { get; set; } = string.Empty;
    public IndirizzoJson Indirizzo { get; set; } = new ();
}