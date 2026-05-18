using System.Net.Http.Json;
using System.Text;
using BrewSpa.Chat.Application.Models;
using Lena.Core;

namespace BrewSpa.Chat.Application.Services;

internal class ChatService(HttpClient httpClient) : IChatService
{
    public async Task<Result<List<BeerCatalogItem>>> GetBeersCatalogAsync()
    {
        try
        {
            var requestUri = "beers";
            var httpResponse = await httpClient.GetAsync(requestUri);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"[ChatService] Error Content: {errorContent}");
                errorMessage.AppendLine("[ChatService] GetBeersCatalogAsync API call failed");
                return Result<List<BeerCatalogItem>>.Error(errorMessage.ToString());
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<List<BeerCatalogItem>>();
            return Result<List<BeerCatalogItem>>.Success(response ?? []);
        }
        catch (Exception ex)
        {
            var errorMessage = new StringBuilder();
            errorMessage.Append($"[ChatService] Exception: {ex.Message}");
            errorMessage.Append($"[ChatService] Stack Trace: {ex.StackTrace}");
            return Result<List<BeerCatalogItem>>.Error(errorMessage.ToString());
        }
    }

    public async Task<Result<ChatResponse>> AskBrewUpChatAsync(ChatRequest request)
    {
        try
        {
            var requestUri = "sales";
            var httpResponse = await httpClient.PostAsJsonAsync(requestUri, request);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync();
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"[ChatService] Error Content: {errorContent}");
                errorMessage.AppendLine("[ChatService] AskBrewUpChatAsync API call failed");
                return Result<ChatResponse>.Error(errorMessage.ToString());
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<ChatResponse>();
            return Result<ChatResponse>.Success(response!);
        }
        catch (Exception ex)
        {
            var errorMessage = new StringBuilder();
            errorMessage.Append($"[ChatService] Exception: {ex.Message}");
            errorMessage.Append($"[ChatService] Stack Trace: {ex.StackTrace}");
            return Result<ChatResponse>.Error(errorMessage.ToString());
        }
    }
}
