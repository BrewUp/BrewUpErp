using System.ComponentModel.DataAnnotations;
using BrewUp.Shared.Validators;

namespace BrewUp.Shared.CustomTypes;

public class Quantity(decimal value, string unitOfMeasure)
{
    [Required]
    [QuantityGreaterThanZero(ErrorMessage = "Quantity must be greater than 0")]
    public decimal Value { get; init; } = value;

    [Required]
    public string UnitOfMeasure { get; init; } = unitOfMeasure;
}