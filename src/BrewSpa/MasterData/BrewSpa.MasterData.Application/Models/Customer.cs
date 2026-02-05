namespace BrewSpa.MasterData.Application.Models;

public class CustomerJson
{
    public string CustomerId { get; set; } = string.Empty;
    public string RagioneSociale { get; set; } = string.Empty;
    public string PartitaIva { get; set; } = string.Empty;
    public string ConsumerLevel { get; set; } = string.Empty;
    public IndirizzoJson Indirizzo { get; set; } = new();
}