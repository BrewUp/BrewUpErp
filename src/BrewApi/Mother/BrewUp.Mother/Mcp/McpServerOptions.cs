namespace BrewUp.Mother.Mcp;

/// <summary>
/// Configuration options for the remote MCP servers invoked by the Chat module.
/// </summary>
public sealed class McpServerOptions
{
    public const string SectionName = "BrewUp:McpServers";

    /// <summary>URL of the MasterData MCP Server (e.g. http://localhost:5007/mcp).</summary>
    public string MasterDataUrl { get; init; } = string.Empty;
    
    /// <summary>URL of the Sales MCP Server (e.g. http://localhost:5229/mcp).</summary>
    public string SalesUrl { get; init; } = string.Empty;

    /// <summary>URL of the Warehouse MCP Server (e.g. http://localhost:5279/mcp).</summary>
    public string WarehouseUrl { get; init; } = string.Empty;
    
    /// <summary>URL of the Mother MCP Server (e.g. http://localhost:5015/mcp).</summary>
    public string MotherUrl { get; init; } = string.Empty;
}

