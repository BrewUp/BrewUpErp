using System.Text.Json;

namespace BrewUp.Shared.Agents;

public sealed record McpToolMetadata(
    string Name,
    string? Description,
    JsonElement? InputSchema);
