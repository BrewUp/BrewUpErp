namespace BrewUp.Mother.SharedKernel.Chat;

public sealed record ChatResponse(string Answer, string? ConversationId = null);
