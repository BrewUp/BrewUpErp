using BrewUp.Shared.ExternalContracts;
using FluentValidation;

namespace BrewUp.Sales.Facade.Validators;

public class CreateSalesOrderValidator : AbstractValidator<CreateSalesOrderJson>
{
    public CreateSalesOrderValidator()
    {
        RuleFor(x => x.OrderNumber).NotNull().NotEmpty();
        RuleFor(x => x.OrderDate).NotNull();
        
        RuleFor(x => x.CustomerId).NotNull().NotEmpty();
        RuleFor(x => x.CustomerName).NotNull().NotEmpty();
        
        RuleForEach(x => x.Rows).SetValidator(new SalesOrderRowValidator());
    }
}