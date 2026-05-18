namespace BrewUp.Mother.Tests;

/// <summary>
/// Forces all Mother test classes to run sequentially.
/// This is required because <see cref="Muflone.Transport.InMemory.MufloneBroker"/> holds
/// static <c>Commands</c> and <c>Events</c> collections that are shared across the
/// entire process. Running two hosts concurrently would cause double-subscription and
/// therefore double handler invocations for every published event.
/// </summary>
[CollectionDefinition(Name)]
public sealed class MotherTestCollection
{
    public const string Name = "BrewUp.Mother";
}

