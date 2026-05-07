using BrewUp.Shared.CustomTypes;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ReadModel;
using BrewUp.Warehouse.Entities.Dtos;

namespace BrewUp.Warehouse.ReadModel.Dtos;

public class WarehouseDto : DtoBase
{
    public string Name { get; set; } = string.Empty;
    public List<ItemStockJson> ItemStocks { get; set; } = new();

    protected WarehouseDto() { }
    
    public static WarehouseDto Create(WarehouseId warehouseId, WarehouseName name, List<ItemStockJson> itemStocks) =>
        new (warehouseId.Value, name.Value, itemStocks);
    
    private WarehouseDto(string warehouseId, string name, List<ItemStockJson> itemStocks)
    {
        Id = warehouseId;
        Name = name;
        ItemStocks = itemStocks;
    }
    
}