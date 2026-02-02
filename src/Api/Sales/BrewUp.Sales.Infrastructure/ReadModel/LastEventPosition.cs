using BrewUp.Shared.ReadModel;

namespace BrewUp.Sales.Infrastructure.ReadModel;

public class LastEventPosition : DtoBase
{
    public ulong CommitPosition { get; set; }
    public ulong PreparePosition { get; set; }   
}