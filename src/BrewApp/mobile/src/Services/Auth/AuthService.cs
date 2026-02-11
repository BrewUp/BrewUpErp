namespace BrewApp.Mobile.Services.Auth;

/// <summary>
/// Manages authentication state and token storage for API requests.
/// In v1, this is a stub that works with the existing BrewUp backend authentication.
/// Future versions may implement full OAuth2/OIDC flows.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Gets the current API key/token for authenticated requests.
    /// </summary>
    Task<string?> GetApiKeyAsync();

    /// <summary>
    /// Gets the current user role (Manager or Customer).
    /// </summary>
    Task<string?> GetUserRoleAsync();

    /// <summary>
    /// Checks if the user is authenticated.
    /// </summary>
    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// Stores authentication credentials (for future login flows).
    /// </summary>
    Task SetAuthenticationAsync(string apiKey, string role);

    /// <summary>
    /// Clears authentication state (logout).
    /// </summary>
    Task ClearAuthenticationAsync();
}

/// <summary>
/// Stub implementation of IAuthService that uses the API key from configuration.
/// In v1, we assume the backend handles auth and provides a static API key.
/// </summary>
public class AuthService : IAuthService
{
    private string? _apiKey;
    private string? _userRole;

    public Task<string?> GetApiKeyAsync()
    {
        // In v1, we use the static API key from ApiSettings
        // Future: retrieve from secure storage after login flow
        return Task.FromResult(_apiKey);
    }

    public Task<string?> GetUserRoleAsync()
    {
        // Stub: assume Manager role by default
        // Future: decode from JWT or retrieve from user profile API
        return Task.FromResult(_userRole ?? "Manager");
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        // Stub: always authenticated if API key is set
        return Task.FromResult(!string.IsNullOrWhiteSpace(_apiKey));
    }

    public Task SetAuthenticationAsync(string apiKey, string role)
    {
        _apiKey = apiKey;
        _userRole = role;
        // Future: store in secure storage (Preferences.Secure or platform keychain)
        return Task.CompletedTask;
    }

    public Task ClearAuthenticationAsync()
    {
        _apiKey = null;
        _userRole = null;
        return Task.CompletedTask;
    }
}
