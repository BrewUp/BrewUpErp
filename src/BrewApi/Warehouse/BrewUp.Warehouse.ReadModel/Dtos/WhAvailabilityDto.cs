using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Warehouse;
using BrewUp.Shared.ReadModel;

namespace BrewUp.Warehouse.ReadModel.Dtos;

public class WhAvailabilityDto : DtoBase
{
    public string WarehouseId { get; private set; }
    public string BeerId { get; private set; }
    public decimal Quantity { get; private set; }
    public string UnitOfMeasure { get; private set; }

    protected WhAvailabilityDto() { }
    
    public static WhAvailabilityDto Create(AvailabilityId aggregateId, 
        WarehouseId warehouseId,
        BeerId beerId, 
        Quantity quantity) => new (aggregateId.Value, 
            warehouseId.Value, 
            beerId.Value, 
            quantity.Value, 
            quantity.UnitOfMeasure);

    internal WhAvailabilityJson ToJson()
    {
        return new WhAvailabilityJson
        {
            Id = Id,
            WarehouseId = WarehouseId,
            BeerId = BeerId,
            Quantity = Quantity,
            UnitOfMeasure = UnitOfMeasure
        };
    }

    private WhAvailabilityDto(string aggregateId, 
        string warehouseId,
        string beerId,
        decimal quantity,
        string unitOfMeasure)
    {
        Id = aggregateId;
        WarehouseId = warehouseId;
        BeerId = beerId;
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure;
    }

    public void UpdateQuantity(Quantity quantity)
    {
        Quantity = quantity.Value;
        UnitOfMeasure = quantity.UnitOfMeasure;
    }
}