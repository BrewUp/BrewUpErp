using BrewSpa.Shared.Helpers;

namespace BrewSpa.Shared.Components.CustomTypes;

public class ToolbarButtons(int id, string name) : Enumeration(id, name)
{
    public static ToolbarButtons AddNewItem = new (1, nameof(AddNewItem));
    public static ToolbarButtons EditCurrentItem = new (2, nameof(EditCurrentItem));
    public static ToolbarButtons DeleteCurrentItem = new (3, nameof(DeleteCurrentItem));
    public static ToolbarButtons Refresh = new (4, nameof(Refresh));
    public static ToolbarButtons Close = new (5, nameof(Close));
    
    public static IEnumerable<ToolbarButtons> List() => [AddNewItem, EditCurrentItem, DeleteCurrentItem, Refresh, Close];
    
    public static ToolbarButtons FromName(string name)
    {
        var state = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        return state ??
               throw new Exception($"Possible values for ToolbarButtons: {string.Join(",", List().Select(s => s.Name))}");
    }
    
    public static ToolbarButtons From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        return state ??
               throw new Exception($"Possible values for ToolbarButtons: {string.Join(",", List().Select(s => s.Name))}");
    }
}