using System.Text.Json.Serialization;

namespace BrewUp.Shared.Agents;

public sealed record AgentCard
{
    [JsonConstructor]
    public AgentCard(
        string name,
        string description,
        string version,
        IReadOnlyCollection<AgentSkill> skills,
        IReadOnlyCollection<AgentCapability> capabilities,
        IReadOnlyCollection<string> examples)
    {
        Name = name;
        Description = description;
        Version = version;
        Skills = skills;
        Capabilities = capabilities;
        Examples = examples;
    }

    public AgentCard(
        string name,
        string description,
        string version,
        IReadOnlyCollection<AgentSkill> skills,
        IReadOnlyCollection<AgentCapability> capabilities)
        : this(name, description, version, skills, capabilities, [])
    {
    }

    public string Name { get; init; }
    public string Description { get; init; }
    public string Version { get; init; }
    public IReadOnlyCollection<AgentSkill> Skills { get; init; }
    public IReadOnlyCollection<AgentCapability> Capabilities { get; init; }
    public IReadOnlyCollection<string> Examples { get; init; }
}
