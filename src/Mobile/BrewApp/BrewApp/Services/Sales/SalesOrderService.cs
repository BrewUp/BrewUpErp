using System.Net.Http.Json;
using System.Text.Json;
using BrewApp.Models.Sales;

namespace BrewApp.Services.Sales;

public class SalesOrderService : ISalesOrderService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public SalesOrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<SalesOrderListResponse> GetSalesOrdersAsync(int page = 0, int pageSize = 10)
    {
        try
        {
            var response = await _httpClient.GetAsync($"sales?page={page}&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<SalesOrderListResponse>(_jsonOptions);
            return result ?? new SalesOrderListResponse();
        }
        catch (Exception ex)
        {
            // Log error or handle appropriately
            Console.WriteLine($"Error fetching sales orders: {ex.Message}");
            return new SalesOrderListResponse();
        }
    }

    public async Task<SalesOrder?> GetSalesOrderByIdAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"sales/{id}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<SalesOrder>(_jsonOptions);
        }
        catch (Exception ex)
        {
            // Log error or handle appropriately
            Console.WriteLine($"Error fetching sales order: {ex.Message}");
            return null;
        }
    }

    public async Task<SalesOrder?> CreateSalesOrderAsync(CreateSalesOrderRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("sales", request, _jsonOptions);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<SalesOrder>(_jsonOptions);
        }
        catch (Exception ex)
        {
            // Log error or handle appropriately
            Console.WriteLine($"Error creating sales order: {ex.Message}");
            return null;
        }
    }
}
