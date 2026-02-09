using BrewSpa.Dashboards.Facade.Models;
using Microsoft.AspNetCore.Components;

namespace BrewSpa.Dashboards.Facade;

public partial class Dashboard : ComponentBase, IDisposable
{
    private List<SummaryData> Data { get; set; } = [];
    
    protected override void OnInitialized()
    {
        Data.Add(new SummaryData { Category = "Jan", NetProfit = 12, Revenue = 33 });
        Data.Add(new SummaryData { Category = "Feb", NetProfit = 43, Revenue = 42 });
        Data.Add(new SummaryData { Category = "Mar", NetProfit = 112, Revenue = 23 });
    }
    
    #region Dispose
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Dashboard()
    {
        Dispose(false);
    }
    #endregion
}