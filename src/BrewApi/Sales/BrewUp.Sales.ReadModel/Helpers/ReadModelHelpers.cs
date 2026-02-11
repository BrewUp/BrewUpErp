using BrewUp.Sales.ReadModel.Dtos;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.ExternalContracts;
using BrewUp.Shared.ExternalContracts.MasterData;
using BrewUp.Shared.ExternalContracts.MasterData.Customers;
using BrewUp.Shared.ExternalContracts.Sales;

namespace BrewUp.Sales.ReadModel.Helpers;

public static class ReadModelHelpers
{
    public static IEnumerable<SalesOrderRow> ToReadModelEntities(this IEnumerable<SalesOrderRowJson> dtos)
    {
        return dtos.Select(dto =>
            new SalesOrderRow
            {
                BeerId = dto.BeerId.ToString(),
                BeerName = dto.BeerName,
                Quantity = dto.Quantity,
                Price = dto.Price
            });
    }
    
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