using System.ComponentModel.DataAnnotations;
using BrewUp.Shared.Validators;

namespace BrewUp.Shared.CustomTypes;

public class Price(decimal value, string currency)
{
    [Required]
    [PriceGreaterThanZero(ErrorMessage = "Price must be greater than 0")]
    public decimal Value { get; init; } = value;

    [Required]
    public string Currency { get; init; } = currency;
}