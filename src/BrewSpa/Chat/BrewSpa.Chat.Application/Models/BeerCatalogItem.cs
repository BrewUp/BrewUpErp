namespace BrewSpa.Chat.Application.Models;

public class BeerCatalogItem
{
    public string BeerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public decimal? AlcoholByVolume { get; set; }
    public bool IsActive { get; set; }
}
