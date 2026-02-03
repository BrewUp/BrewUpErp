using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Shared.ExternalContracts;
using BrewUp.Shared.ReadModel;
using Lena.Core;

namespace BrewUp.Sales.ReadModel.Services;

public interface ISalesOrderService
{
    Task<Result<bool>> CreateSalesOrderAsync(SalesOrderId salesOrderId, SalesOrderNumber salesOrderNumber, CustomerId customerId,
        CustomerName customerName, SalesOrderDate orderDate, IEnumerable<SalesOrderRowJson> rows, CancellationToken cancellationToken);
    
    Task<Result<PagedResult<SalesOrderJson>>> GetSalesOrdersAsync(int page, int pageSize, CancellationToken cancellationToken);
}