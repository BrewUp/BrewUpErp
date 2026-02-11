using Microsoft.Extensions.Options;
using BrewApp.Mobile.Configuration;
using BrewApp.Mobile.Services.Api.Dtos;

namespace BrewApp.Mobile.Services.Api;

/// <summary>
/// API client for sales operations and summaries.
/// Implements GET /sales/summary endpoint from the external BrewUp API.
/// </summary>
public interface ISalesApiClient
{
    /// <summary>
    /// Retrieves sales summary data for a specified period.
    /// </summary>
    /// <param name="period">Period filter: "today", "week", "month", "year"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sales summary for the period</returns>
    Task<SalesSummaryDto?> GetSalesSummaryAsync(string period = "today", CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of ISalesApiClient using the configured Sales API endpoint.
/// </summary>
public class SalesApiClient : BaseApiClient, ISalesApiClient
{
    private readonly string _baseUrl;

    public SalesApiClient(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> settings)
        : base(httpClientFactory, settings)
    {
        _baseUrl = settings.Value.SalesBaseUrl?.TrimEnd('/') 
            ?? throw new InvalidOperationException("SalesBaseUrl is not configured in ApiSettings");
    }

    public async Task<SalesSummaryDto?> GetSalesSummaryAsync(string period = "today", CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string?> { { "period", period } };
        var queryString = BuildQueryString(queryParams);
        var endpoint = $"{_baseUrl}/sales/summary{queryString}";

        return await GetAsync<SalesSummaryDto>(endpoint, cancellationToken);
    }
}
