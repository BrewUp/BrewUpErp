namespace BrewUp.Mother.CustomTypes;

public sealed record McpError(
    int Code,
    string Message);