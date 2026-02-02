using BrewUp.Shared.ExternalContracts;
using FluentValidation;

namespace BrewUp.Sales.Facade.Validators;

public class SalesOrderRowValidator : AbstractValidator<SalesOrderRowJson>
{
    public SalesOrderRowValidator()
    {
        RuleFor(x => x.Quantity.Quantity).GreaterThan(0);
        RuleFor(x => x.Quantity.UnitOfMeasure).NotNull().NotEmpty();
        
        RuleFor(x => x.Price.Price).GreaterThan(0);
        RuleFor(x => x.Price.Currency).NotNull().NotEmpty();
    }
}