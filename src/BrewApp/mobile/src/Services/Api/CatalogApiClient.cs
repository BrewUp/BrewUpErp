using Microsoft.Extensions.Options;
using BrewApp.Mobile.Configuration;
using BrewApp.Mobile.Services.Api.Dtos;

namespace BrewApp.Mobile.Services.Api;

/// <summary>
/// API client for catalog (beers) operations.
/// Implements GET /catalog/beers endpoint from the external BrewUp API.
/// </summary>
public interface ICatalogApiClient
{
    /// <summary>
    /// Retrieves all beers from the catalog with optional filtering.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by name or style</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of beers matching the search criteria</returns>
    Task<List<BeerDto>?> GetBeersAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of ICatalogApiClient using the configured Catalog API endpoint.
/// </summary>
public class CatalogApiClient : BaseApiClient, ICatalogApiClient
{
    private readonly string _baseUrl;

    public CatalogApiClient(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> settings)
        : base(httpClientFactory, settings)
    {
        _baseUrl = settings.Value.CatalogBaseUrl?.TrimEnd('/') 
            ?? throw new InvalidOperationException("CatalogBaseUrl is not configured in ApiSettings");
    }

    public async Task<List<BeerDto>?> GetBeersAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            queryParams.Add("search", searchTerm);
        }

        var queryString = BuildQueryString(queryParams);
        var endpoint = $"{_baseUrl}/catalog/beers{queryString}";

        return await GetAsync<List<BeerDto>>(endpoint, cancellationToken);
    }
}
