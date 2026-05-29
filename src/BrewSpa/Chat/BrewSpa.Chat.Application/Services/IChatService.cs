using BrewSpa.Chat.Application.Models;
using Lena.Core;

namespace BrewSpa.Chat.Application.Services;

public interface IChatService
{
Task<Result<ChatResponse>> AskBrewUpChatAsync(ChatRequest request);
}
