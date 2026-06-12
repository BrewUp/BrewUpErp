namespace BrewUp.Shared.Helpers;

public sealed class BeerConsumerLevel(int id, string name) : Enumeration(id, name)
{
    public static readonly BeerConsumerLevel Teetotaler = new (0, nameof(Teetotaler).ToLowerInvariant());
    public static readonly BeerConsumerLevel Gold = new (1, nameof(Gold).ToLowerInvariant());
    public static readonly BeerConsumerLevel Silver = new (2, nameof(Silver).ToLowerInvariant());
    public static readonly BeerConsumerLevel Bronze = new (3, nameof(Bronze).ToLowerInvariant());

    private static IEnumerable<BeerConsumerLevel> List() => [Teetotaler, Gold, Silver, Bronze];

    public static BeerConsumerLevel FromName(string name)
    {
        var level = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        return level ??
               throw new Exception($"Possible values for BeerConsumerLevel: {string.Join(",", List().Select(s => s.Name))}");
    }

    public static BeerConsumerLevel From(int id)
    {
        var level = List().SingleOrDefault(s => s.Id == id);

        return level ??
               throw new Exception($"Possible values for BeerConsumerLevel: {string.Join(",", List().Select(s => s.Name))}");
    }
}
