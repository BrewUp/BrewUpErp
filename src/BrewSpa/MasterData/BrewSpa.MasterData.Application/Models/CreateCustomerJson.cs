namespace BrewSpa.MasterData.Application.Models;

public class CreateCustomerJson
{
    public string RagioneSociale { get; set; } = string.Empty;
    public string PartitaIva { get; set; } = string.Empty;
    public IndirizzoJson Indirizzo { get; set; } = new ();
}