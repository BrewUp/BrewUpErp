namespace BrewSpa.Chat.Application.Models;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
}
