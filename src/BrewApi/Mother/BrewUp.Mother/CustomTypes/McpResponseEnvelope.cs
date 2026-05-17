using System.Text.Json;

namespace BrewUp.Mother.CustomTypes;

public sealed record McpResponseEnvelope(
    string Jsonrpc,
    string Id,
    JsonElement? Result,
    McpError? Error);