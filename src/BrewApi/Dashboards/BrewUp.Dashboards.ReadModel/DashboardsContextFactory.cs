using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BrewUp.Dashboards.ReadModel;

public class DashboardsContextFactory : IDesignTimeDbContextFactory<DashboardsContext>
{
    public DashboardsContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DashboardsContext>();
        optionsBuilder.UseSqlServer("Server=brewup-sql-server.database.windows.net;Database=Sales;User Id=brewup-admin;Password=AzureDayRome!2025;TrustServerCertificate=True");

        return new DashboardsContext(optionsBuilder.Options);
    }
}

