namespace BrewUp.Chat.SharedKernel.Chat;

public sealed record ChatResponse(string Answer, string? ConversationId = null);
