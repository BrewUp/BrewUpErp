namespace BrewApp.Mobile.Models;

/// <summary>
/// Internal model for Beer entity.
/// Represents a beer product in the brewery's catalog.
/// </summary>
public class Beer
{
    public Guid BeerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public decimal Abv { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}
