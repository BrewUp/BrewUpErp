using BrewSpa.MasterData.Application.Models;
using Lena.Core;

namespace BrewSpa.MasterData.Application.Services;

public interface ICustomerService
{
    Task<Result<PagedResult<CustomerJson>>> GetCustomersAsync(int page = 0, int pageSize = 10);
    Task<Result<CustomerJson>> GetCustomerByIdAsync(string customerId);
    Task<Result<CustomerJson>> CreateCustomerAsync(CustomerJson customer);
    Task<Result<CustomerJson>> UpdateCustomerAsync(CustomerJson customer);
    Task<Result<bool>> DeleteCustomerAsync(string customerId);
}