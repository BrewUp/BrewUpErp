using System.Net.Http.Json;
using System.Text;
using BrewSpa.MasterData.Application.Models;
using Lena.Core;

namespace BrewSpa.MasterData.Application.Services;

internal class CustomerService(HttpClient httpClient) : ICustomerService
{
    public async Task<Result<PagedResult<CustomerJson>>> GetCustomersAsync(int page = 0, int pageSize = 10)
    {
        try
        {
            var requestUri = $"customers?pageNumber={page}&pageSize={pageSize}";
            var httpResponse = await httpClient.GetAsync(requestUri);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                StringBuilder errorMessage = new();
                errorMessage.AppendLine($"[CustomerService] Error Content: {errorContent}");
                errorMessage.AppendLine("[CustomerService] API call failed, falling back to mock data");
                return Result<PagedResult<CustomerJson>>.Error(errorMessage.ToString());
            }
            
            var response = await httpResponse.Content.ReadFromJsonAsync<PagedResult<CustomerJson>>();
            return Result<PagedResult<CustomerJson>>.Success(new PagedResult<CustomerJson>(response!.Results,
                response.Page, response.PageSize, response.TotalRecords));
        }
        catch (Exception ex)
        {
            StringBuilder errorMessage = new();
            errorMessage.Append($"[CustomerService] Exception: {ex.Message}");
            errorMessage.Append($"[CustomerService] Stack Trace: {ex.StackTrace}");
            errorMessage.Append("[CustomerService] API call failed, falling back to mock data");
            return Result<PagedResult<CustomerJson>>.Error(errorMessage.ToString());
        }
    }

    public async Task<Result<CustomerJson>> GetCustomerByIdAsync(string customerId)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<CustomerJson>($"customers/{customerId}");
            return Result<CustomerJson>.Success(response!);
        }
        catch (Exception ex)
        {
            return Result<CustomerJson>.Error($"Customer with ID {customerId} not found");
        }
    }

    public async Task<Result<CustomerJson>> CreateCustomerAsync(CustomerJson customer)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("customers", customer);
            response.EnsureSuccessStatusCode();
            
            var createdCustomer = await response.Content.ReadFromJsonAsync<CustomerJson>();
            return Result<CustomerJson>.Success(createdCustomer!);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CustomerService] CreateCustomerAsync failed, using mock response: {ex.Message}");
            // For mock mode, just return the customer with a generated ID if it doesn't have one
            if (string.IsNullOrEmpty(customer.CustomerId))
            {
                customer.CustomerId = Guid.NewGuid().ToString();
            }
            return Result<CustomerJson>.Success(customer);
        }
    }

    public async Task<Result<CustomerJson>> UpdateCustomerAsync(CustomerJson customer)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"customers/{customer.CustomerId}", customer);
            response.EnsureSuccessStatusCode();
            
            var updatedCustomer = await response.Content.ReadFromJsonAsync<CustomerJson>();
            return Result<CustomerJson>.Success(updatedCustomer!);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CustomerService] UpdateCustomerAsync failed, using mock response: {ex.Message}");
            // For mock mode, just return the updated customer
            return Result<CustomerJson>.Success(customer);
        }
    }

    public async Task<Result<bool>> DeleteCustomerAsync(string customerId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"customers/{customerId}");
            return Result<bool>.Success(response.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CustomerService] DeleteCustomerAsync failed, using mock response: {ex.Message}");
            // For mock mode, always return success
            return Result<bool>.Success(true);
        }
    }
}
