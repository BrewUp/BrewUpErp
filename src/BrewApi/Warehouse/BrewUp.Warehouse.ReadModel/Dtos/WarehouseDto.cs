using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.Entities.Dtos;

namespace BrewUp.Warehouse.ReadModel.Dtos;

public class WarehouseDto : DtoBase
{
    public string Name { get; set; } = string.Empty;

    protected WarehouseDto() { }
    
    public static WarehouseDto Create(WarehouseId warehouseId, WarehouseName name, List<ItemStock> itemStocks) =>
        new (warehouseId.Value, name.Value, itemStocks);

    internal Shared.ExternalContracts.Warehouse.WarehouseJson ToJson()
    {
        return new Shared.ExternalContracts.Warehouse.WarehouseJson
        {
            Id = Id,
            Name = Name,
        };
    }

    private WarehouseDto(string warehouseId, string name, List<ItemStock> itemStocks)
    {
        Id = warehouseId;
        Name = name;
    }
    
}