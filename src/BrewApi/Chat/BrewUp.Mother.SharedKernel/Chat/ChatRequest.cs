namespace BrewUp.Mother.SharedKernel.Chat;

public sealed record ChatRequest(string Message, string? ConversationId = null);
