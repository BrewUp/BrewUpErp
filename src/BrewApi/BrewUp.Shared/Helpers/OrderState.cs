namespace BrewUp.Shared.Helpers;

public sealed class OrderState(int id, string name) : Enumeration(id, name)
{
    public static readonly OrderState Created = new (1, nameof(Created).ToLowerInvariant());
    public static readonly OrderState Sent = new (1, nameof(Sent).ToLowerInvariant());
    public static readonly OrderState Completed = new (2, nameof(Completed).ToLowerInvariant());
    public static readonly OrderState Cancelled = new (3, nameof(Cancelled).ToLowerInvariant());

    private static IEnumerable<OrderState> List() => [Sent, Completed, Cancelled];

    public static OrderState FromName(string name)
    {
        var state = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        return state ??
               throw new Exception($"Possible values for OrderState: {string.Join(",", List().Select(s => s.Name))}");
    }

    public static OrderState From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        return state ??
               throw new Exception($"Possible values for OrderState: {string.Join(",", List().Select(s => s.Name))}");
    }
}