using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.ExternalContracts;
using BrewUp.Shared.ExternalContracts.Sales;

namespace BrewUp.Sales.ReadModel.Dtos;

public class SalesOrderRow
{
    public string BeerId { get; set; } = string.Empty;
    public string BeerName { get; set; } = string.Empty;
    public Quantity Quantity { get; set; } = default!;
    public Price Price { get; set; } = default!;

    internal SalesOrderRowJson ToJson => new()
    {
        BeerId = BeerId,
        BeerName = BeerName,
        Quantity = Quantity,
        Price = Price
    };
}