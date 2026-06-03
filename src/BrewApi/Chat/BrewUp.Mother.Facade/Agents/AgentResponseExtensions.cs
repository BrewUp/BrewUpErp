namespace BrewUp.Mother.Facade.Agents;

internal static class AgentResponseExtensions
{
    public static T GetRequired<T>(this AgentResponse response, string key)
    {
        if (!response.Data.TryGetValue(key, out var value) || value is not T typed)
            throw new InvalidOperationException(
                $"{response.AgentName} did not return the expected '{key}' payload.");

        return typed;
    }
}
