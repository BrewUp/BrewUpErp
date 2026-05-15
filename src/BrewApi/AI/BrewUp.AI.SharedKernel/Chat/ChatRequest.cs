namespace BrewUp.AI.SharedKernel.Chat;

public sealed record ChatRequest(string Message, string? ConversationId = null);
