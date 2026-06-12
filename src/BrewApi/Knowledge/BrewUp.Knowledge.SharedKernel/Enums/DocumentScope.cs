using BrewUp.Shared.Helpers;

namespace BrewUp.Knowledge.SharedKernel.Enums;

public sealed class DocumentScope(int id, string name) : Enumeration(id, name)
{
    public static readonly DocumentScope General = new (0, nameof(General).ToLowerInvariant());
    public static readonly DocumentScope Sales = new (1, nameof(Sales).ToLowerInvariant());
    public static readonly DocumentScope Warehouse = new (2, nameof(Warehouse).ToLowerInvariant());
    public static readonly DocumentScope MasterData = new (3, nameof(MasterData).ToLowerInvariant());
    public static readonly DocumentScope Production = new (4, nameof(Production).ToLowerInvariant());

    private static IEnumerable<DocumentScope> List() =>
        [General, Sales, Warehouse, MasterData, Production];
    
    public static DocumentScope FromName(string name)
    {
        var documentScope = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        return documentScope ??
               throw new Exception($"Possible values for DocumentScope: {string.Join(",", List().Select(s => s.Name))}");
    }

    public static DocumentScope From(int id)
    {
        var documentScope = List().SingleOrDefault(s => s.Id == id);

        return documentScope ??
               throw new Exception($"Possible values for DocumentScope: {string.Join(",", List().Select(s => s.Name))}");
    }
}
