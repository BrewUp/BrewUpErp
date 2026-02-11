using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using BrewApp.Mobile.Configuration;

namespace BrewApp.Mobile.Services.Api;

/// <summary>
/// Base class for all feature-specific API clients. Provides common HTTP methods
/// and standardized error handling for external BrewUp ERP APIs.
/// </summary>
public abstract class BaseApiClient
{
    protected readonly HttpClient HttpClient;
    protected readonly ApiSettings Settings;
    protected readonly JsonSerializerOptions JsonOptions;

    protected BaseApiClient(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> settings)
    {
        HttpClient = httpClientFactory.CreateClient("BrewAppAPI");
        Settings = settings.Value;
        
        // Configure JSON serialization for consistent API communication
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// GET request with JSON deserialization and standardized error handling.
    /// </summary>
    protected async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await HttpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException($"Failed to GET {endpoint}: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new ApiException($"Failed to deserialize response from {endpoint}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// POST request with JSON serialization/deserialization and error handling.
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest payload, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(endpoint, payload, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException($"Failed to POST {endpoint}: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new ApiException($"Failed to deserialize response from {endpoint}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// PUT request with JSON serialization/deserialization and error handling.
    /// </summary>
    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest payload, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await HttpClient.PutAsJsonAsync(endpoint, payload, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException($"Failed to PUT {endpoint}: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new ApiException($"Failed to deserialize response from {endpoint}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// DELETE request with error handling.
    /// </summary>
    protected async Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await HttpClient.DeleteAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException($"Failed to DELETE {endpoint}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Builds a query string from a dictionary of parameters.
    /// </summary>
    protected static string BuildQueryString(Dictionary<string, string?> parameters)
    {
        var queryParams = parameters
            .Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}");
        
        return queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty;
    }
}

/// <summary>
/// Exception thrown when API communication fails.
/// </summary>
public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
    public ApiException(string message, Exception innerException) : base(message, innerException) { }
}
