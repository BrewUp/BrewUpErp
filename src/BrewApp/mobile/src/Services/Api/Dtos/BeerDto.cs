namespace BrewApp.Mobile.Services.Api.Dtos;

/// <summary>
/// DTO for Beer entity from the Catalog API.
/// Matches the schema from contracts/mobile-app-apis.openapi.yaml.
/// </summary>
public class BeerDto
{
    public Guid? BeerId { get; set; }
    public string? Name { get; set; }
    public string? Style { get; set; }
    public decimal? Abv { get; set; }
    public string? Description { get; set; }
    public bool? IsAvailable { get; set; }
}
