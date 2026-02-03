using BrewApp.PageModels.Sales;

namespace BrewApp.Pages.Sales;

public partial class CreateSalesOrderPage : ContentPage
{
    public CreateSalesOrderPage(CreateSalesOrderPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
