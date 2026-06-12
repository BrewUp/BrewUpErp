namespace BrewUp.Knowledge.SharedKernel.Configuration;

public sealed class SqlServerKnowledgeVectorStoreOptions
{
    public const string SectionName = "BrewUp:SqlServer";

    public string ConnectionString { get; init; } = string.Empty;
    public int Dimensions { get; init; } = 256;
}
