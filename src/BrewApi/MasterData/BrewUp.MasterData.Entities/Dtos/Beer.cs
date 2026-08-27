using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.MasterData.Beers;
using BrewUp.Shared.ReadModel;

namespace BrewUp.MasterData.Entities.Dtos;

public class Beer : DtoBase
{
    public string BeerName { get; set; } = string.Empty;
    public string BeerStyle { get; set; } = string.Empty;
    public decimal AlcoholByVolume { get; set; }
    public string Packaging { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PriceCurrency { get; set; } = "EUR";
    public bool IsActive { get; set; }
    
    protected  Beer()
    {}

    public static Beer Create(BeerId beerId, BeerName beerName, BeerStyle beerStyle, AlcoholByVolume alcoholByVolume,
        Packaging packaging, Price price, bool isActive) => new(beerId.Value, beerName.Value, beerStyle.Value,
        alcoholByVolume.Value, packaging.Value, price.Value, price.Currency, isActive);
    
    private Beer(string beerId, string beerName, string beerStyle, decimal alcoholByVolume, string packaging, decimal price, string priceCurrency, bool isActive)
    {
        Id = beerId;
        BeerName = beerName;
        BeerStyle = beerStyle;
        AlcoholByVolume = alcoholByVolume;
        Packaging = packaging;
        Price = price;
        PriceCurrency = priceCurrency;
        IsActive = isActive;
    }

    public void UpdateBeerName(BeerName beerName) => BeerName = beerName.Value;
    public void UpdateBeerStyle(BeerStyle beerStyle) => BeerStyle = beerStyle.Value;
    public void UpdateAlcoholByVolume(AlcoholByVolume alcoholByVolume) => AlcoholByVolume = alcoholByVolume.Value;
    public void UpdatePackaging(Packaging packaging) => Packaging = packaging.Value;
    public void UpdatePrice(Price price)
    {
        Price = price.Value;
        PriceCurrency = price.Currency;
    }
    public void UpdateIsActive(bool isActive) => IsActive = isActive;
    
    public BeerJson ToJson() => new()
    {
        BeerId = Id,
        BeerName = BeerName,
        BeerStyle = BeerStyle,
        AlcoholByVolume = AlcoholByVolume,
        Packaging = Packaging,
        Price = new Price(Price, PriceCurrency),
        IsActive = IsActive
    };
}