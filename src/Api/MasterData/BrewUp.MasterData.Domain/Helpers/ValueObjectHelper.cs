using BrewUp.MasterData.Domain.ValueObjects;
using BrewUp.Shared.ExternalContracts.MasterData;

namespace BrewUp.MasterData.Domain.Helpers;

public static class ValueObjectHelper
{
    public static IndirizzoJson ToIndirizzoJson(this Indirizzo indirizzo) =>
        new()
        {
            Via = indirizzo.Via.Value,
            Citta = indirizzo.Citta.Value,
            Cap = indirizzo.Cap.Value,
            Provincia = indirizzo.Provincia.Value,
            Nazione = indirizzo.Nazione.Value
        };
}