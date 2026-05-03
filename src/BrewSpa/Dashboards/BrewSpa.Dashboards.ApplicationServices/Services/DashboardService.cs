using System.Net.Http.Json;
using System.Text;
using BrewSpa.Dashboards.ApplicationServices.Models;
using BrewSpa.Shared.Models;
using Lena.Core;

namespace BrewSpa.Dashboards.ApplicationServices.Services;

internal class DashboardService(HttpClient httpClient) : IDashboardService
{
    public async Task<Result<PagedResult<SalesByCustomerJson>>> GetSalesByCustomerAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var requestUri = $"customers?pageNumber={pageNumber}&pageSize={pageSize}";
            var httpResponse = await httpClient.GetAsync(requestUri);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"[DashboardService] Error Content: {errorContent}");
                errorMessage.AppendLine("[DashboardService] GetSalesByCustomerAsync API call failed");
                return Result<PagedResult<SalesByCustomerJson>>.Error(errorMessage.ToString());
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<PagedResult<SalesByCustomerJson>>();
            return Result<PagedResult<SalesByCustomerJson>>.Success(
                new PagedResult<SalesByCustomerJson>(response!.Results, response.Page, response.PageSize, response.TotalRecords));
        }
        catch (Exception ex)
        {
            var errorMessage = new StringBuilder();
            errorMessage.Append($"[DashboardService] Exception: {ex.Message}");
            errorMessage.Append($"[DashboardService] Stack Trace: {ex.StackTrace}");
            return Result<PagedResult<SalesByCustomerJson>>.Error(errorMessage.ToString());
        }
    }

    public async Task<Result<PagedResult<SalesByProductJson>>> GetSalesByProductAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var requestUri = $"products?pageNumber={pageNumber}&pageSize={pageSize}";
            var httpResponse = await httpClient.GetAsync(requestUri);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"[DashboardService] Error Content: {errorContent}");
                errorMessage.AppendLine("[DashboardService] GetSalesByProductAsync API call failed");
                return Result<PagedResult<SalesByProductJson>>.Error(errorMessage.ToString());
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<PagedResult<SalesByProductJson>>();
            return Result<PagedResult<SalesByProductJson>>.Success(
                new PagedResult<SalesByProductJson>(response!.Results, response.Page, response.PageSize, response.TotalRecords));
        }
        catch (Exception ex)
        {
            var errorMessage = new StringBuilder();
            errorMessage.Append($"[DashboardService] Exception: {ex.Message}");
            errorMessage.Append($"[DashboardService] Stack Trace: {ex.StackTrace}");
            return Result<PagedResult<SalesByProductJson>>.Error(errorMessage.ToString());
        }
    }
}
