namespace BrewUp.Shared.Configuration;

public class MongoDbSettings
{
    public required string ConnectionString { get; init; } = string.Empty;
    public required string DatabaseName { get; init; } = string.Empty;
}