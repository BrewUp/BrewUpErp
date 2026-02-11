using BrewUp.MasterData.SharedKernel.Dtos;
using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.Messages.Events;

namespace BrewUp.MasterData.Domain.Helpers;

internal static class BeerHelper
{
    public static BeerCreated ToBeerCreated(this Beer beer) => 
        new (new BeerId(beer.Id), new BeerName(beer.BeerName),
            new BeerStyle(beer.BeerStyle), new AlcoholByVolume(beer.AlcoholByVolume),
            new Packaging(beer.Packaging), new Price(beer.Price, "EUR"), beer.IsActive);
    
    public static BeerUpdated ToBeerUpdated(this Beer beer) => 
        new (new BeerId(beer.Id), new BeerName(beer.BeerName),
            new BeerStyle(beer.BeerStyle), new AlcoholByVolume(beer.AlcoholByVolume),
            new Packaging(beer.Packaging), new Price(beer.Price, "EUR"), beer.IsActive);
    
    public static BeerDeleted ToBeerDeleted(this Beer beer) => 
        new (new BeerId(beer.Id));
}