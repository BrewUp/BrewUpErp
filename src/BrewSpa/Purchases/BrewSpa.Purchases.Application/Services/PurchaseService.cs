using System.Net.Http.Json;
using System.Text;
using BrewSpa.Purchases.Application.Models;
using Lena.Core;

namespace BrewSpa.Purchases.Application.Services;

internal class PurchaseService(HttpClient httpClient) : IPurchaseService
{
    public async Task<Result<bool>> CreatePurchaseOrderAsync(CreatePurchaseOrderJson purchaseOrder)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("purchases", purchaseOrder);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                StringBuilder errorMessage = new();
                errorMessage.AppendLine($"[PurchaseService] Error Content: {errorContent}");
                errorMessage.AppendLine("[PurchaseService] API call failed");
                return Result<bool>.Error(errorMessage.ToString());
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            StringBuilder errorMessage = new();
            errorMessage.Append($"[PurchaseService] Exception: {ex.Message}");
            errorMessage.Append($"[PurchaseService] Stack Trace: {ex.StackTrace}");
            return Result<bool>.Error(errorMessage.ToString());
        }
    }
}
