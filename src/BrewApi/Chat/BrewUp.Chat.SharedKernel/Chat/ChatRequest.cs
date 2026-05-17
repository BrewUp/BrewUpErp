namespace BrewUp.Chat.SharedKernel.Chat;

public sealed record ChatRequest(string Message, string? ConversationId = null);
