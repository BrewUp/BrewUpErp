using BrewUp.Shared.DomainIds;

namespace BrewUp.Warehouse.SharedKernel.CustomTypes
{
    public record ItemRequest(BeerId BeerId, decimal Quantity);
}
