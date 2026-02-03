using BrewApp.PageModels.Sales;

namespace BrewApp.Pages.Sales;

public partial class SalesOrderListPage : ContentPage
{
    private readonly SalesOrderListPageModel _viewModel;

    public SalesOrderListPage(SalesOrderListPageModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSalesOrdersCommand.ExecuteAsync(null);
    }
}
