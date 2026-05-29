using System.Net.Http.Json;
using System.Text;
using BrewSpa.Chat.Application.Models;
using Lena.Core;

namespace BrewSpa.Chat.Application.Services;

internal sealed class ChatService(HttpClient httpClient) : IChatService
{
    public async Task<Result<ChatResponse>> AskBrewUpChatAsync(ChatRequest request)
    {
        try
        {
            var requestUri = "";
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
