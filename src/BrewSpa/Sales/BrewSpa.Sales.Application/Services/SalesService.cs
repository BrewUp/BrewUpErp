using System.Net.Http.Json;
using System.Text;
using BrewSpa.Sales.Application.Models;
using BrewSpa.Shared.Models;
using Lena.Core;

namespace BrewSpa.Sales.Application.Services;

internal class SalesService(HttpClient httpClient, HttpClient sagaHttpClient) : ISalesService
{
    public async Task<Result<PagedResult<SalesOrderJson>>> GetSalesOrdersAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var requestUri = $"sales?pageNumber={page}&pageSize={pageSize}";
            var httpResponse = await httpClient.GetAsync(requestUri);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                StringBuilder errorMessage = new();
                errorMessage.AppendLine($"[SalesService] Error Content: {errorContent}");
                errorMessage.AppendLine("[SalesService] API call failed");
                return Result<PagedResult<SalesOrderJson>>.Error(errorMessage.ToString());
            }
            
            var response = await httpResponse.Content.ReadFromJsonAsync<PagedResult<SalesOrderJson>>();
            return Result<PagedResult<SalesOrderJson>>.Success(new PagedResult<SalesOrderJson>(
                response!.Results,
                response.Page, 
                response.PageSize, 
                response.TotalRecords));
        }
        catch (Exception ex)
        {
            StringBuilder errorMessage = new();
            errorMessage.Append($"[SalesService] Exception: {ex.Message}");
            errorMessage.Append($"[SalesService] Stack Trace: {ex.StackTrace}");
            errorMessage.Append("[SalesService] API call failed");
            return Result<PagedResult<SalesOrderJson>>.Error(errorMessage.ToString());
        }
    }

    public async Task<Result<SalesOrderJson>> GetSalesOrderByIdAsync(string orderId)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<SalesOrderJson>($"sales/{orderId}");
            return Result<SalesOrderJson>.Success(response!);
        }
        catch (Exception ex)
        {
            return Result<SalesOrderJson>.Error(
                $"Sales order with ID {orderId} not found. [SalesService] Exception: {ex.Message}");
        }
    }

    public async Task<Result<SalesOrderJson>> CreateSalesOrderAsync(CreateSalesOrderJson order)
    {
        try
        {
            var response = await sagaHttpClient.PostAsJsonAsync("sales", order);
            response.EnsureSuccessStatusCode();
            
            var createdOrder = await response.Content.ReadFromJsonAsync<SalesOrderJson>();
            return Result<SalesOrderJson>.Success(createdOrder!);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SalesService] CreateSalesOrderAsync failed: {ex.Message}");
            return Result<SalesOrderJson>.Error($"Failed to create sales order: {ex.Message}");
        }
    }
}
