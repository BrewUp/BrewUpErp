namespace BrewApp.Mobile.Configuration;

/// <summary>
/// Strongly-typed configuration for external API endpoints and settings.
/// Bound from the ApiSettings section in apiSettings.json.
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// Environment name (Development, Staging, Production)
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Base URL for Sales API endpoints
    /// </summary>
    public string SalesBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Catalog API endpoints
    /// </summary>
    public string CatalogBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Orders API endpoints
    /// </summary>
    public string OrdersBaseUrl { get; set} = string.Empty;

    /// <summary>
    /// Base URL for Warehouse API endpoints
    /// </summary>
    public string WarehouseBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API key or token for authentication (should be stored securely in production)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// HTTP request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
