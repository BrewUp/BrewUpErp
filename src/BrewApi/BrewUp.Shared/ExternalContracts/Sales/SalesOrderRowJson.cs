using System.ComponentModel.DataAnnotations;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.Validators;

namespace BrewUp.Shared.ExternalContracts.Sales;

public class SalesOrderRowJson
{
    [Required]
    public string BeerId { get; init; } = string.Empty;
    [Required]
    public string BeerName { get; init; } = string.Empty;
    
    [Required]
    [QuantityGreaterThanZero(ErrorMessage = "Quantity must be greater than 0")]
    [AttributeIsMandatory("UnitOfMeasure", ErrorMessage = "Unit of Measure is mandatory")]
    public Quantity Quantity { get; init; } = new (0, string.Empty);

    [Required]
    [PriceGreaterThanZero(ErrorMessage = "Price must be greater than 0")]
    [AttributeIsMandatory("Currency", ErrorMessage = "Currency is mandatory")]
    public Price Price { get; init; } = new(0, string.Empty);
}