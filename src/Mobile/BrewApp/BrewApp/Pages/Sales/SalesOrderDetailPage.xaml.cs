using BrewApp.PageModels.Sales;

namespace BrewApp.Pages.Sales;

public partial class SalesOrderDetailPage : ContentPage
{
    public SalesOrderDetailPage(SalesOrderDetailPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
