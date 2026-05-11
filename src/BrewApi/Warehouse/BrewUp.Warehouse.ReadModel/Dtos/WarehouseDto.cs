using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ReadModel;

namespace BrewUp.Warehouse.ReadModel.Dtos;

public class WarehouseDto : DtoBase
{
    public string Name { get; set; } = string.Empty;

    protected WarehouseDto() { }
    
    public static WarehouseDto Create(WarehouseId warehouseId, WarehouseName name) =>
        new (warehouseId.Value, name.Value);

    internal Shared.ExternalContracts.Warehouse.WarehouseJson ToJson()
    {
        return new Shared.ExternalContracts.Warehouse.WarehouseJson
        {
            Id = Id,
            Name = Name,
        };
    }

    private WarehouseDto(string warehouseId, string name)
    {
        Id = warehouseId;
        Name = name;
    }
}