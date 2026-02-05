namespace BrewUp.Shared.ExternalContracts.MasterData;

public record CustomerJson(string CustomerId,
    string RagioneSociale,
    string PartitaIva,
    string ConsumerLevel,
    IndirizzoJson Indirizzo);