namespace BrewApp.Mobile.Services.Ui;

/// <summary>
/// Unified service for displaying user notifications, errors, and dialogs.
/// Provides consistent UX for error handling and informational messages.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Shows a brief toast notification at the bottom of the screen.
    /// </summary>
    Task ShowToastAsync(string message, ToastDuration duration = ToastDuration.Short);

    /// <summary>
    /// Shows an error message with optional retry action.
    /// </summary>
    Task ShowErrorAsync(string title, string message, string? retryButtonText = null);

    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message, string acceptText = "Yes", string cancelText = "No");

    /// <summary>
    /// Shows an informational alert with a single OK button.
    /// </summary>
    Task ShowAlertAsync(string title, string message, string buttonText = "OK");
}

public enum ToastDuration
{
    Short,
    Long
}

/// <summary>
/// Implementation of INotificationService using MAUI's built-in dialogs and community toolkit toasts.
/// </summary>
public class NotificationService : INotificationService
{
    public async Task ShowToastAsync(string message, ToastDuration duration = ToastDuration.Short)
    {
        // Use CommunityToolkit.Maui.Alerts.Toast in production
        // For now, use a simple shell alert as fallback
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("Info", message, "OK");
        }
    }

    public async Task ShowErrorAsync(string title, string message, string? retryButtonText = null)
    {
        if (Application.Current?.MainPage == null) return;

        if (!string.IsNullOrWhiteSpace(retryButtonText))
        {
            await Application.Current.MainPage.DisplayAlert(title, message, retryButtonText, "Cancel");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string acceptText = "Yes", string cancelText = "No")
    {
        if (Application.Current?.MainPage == null) return false;

        return await Application.Current.MainPage.DisplayAlert(title, message, acceptText, cancelText);
    }

    public async Task ShowAlertAsync(string title, string message, string buttonText = "OK")
    {
        if (Application.Current?.MainPage == null) return;

        await Application.Current.MainPage.DisplayAlert(title, message, buttonText);
    }
}
