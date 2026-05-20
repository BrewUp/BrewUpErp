---
name: net-expert
description: Expert guidance for the BrewUp ERP solution. Covers naming conventions, dependency injection patterns, and testing. Use when adding new types, registering services, or writing tests.
---

# BrewUp .NET Developer Guidelines

## Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Classes, interfaces, records | PascalCase | `CustomerDomainService`, `ICustomerDomainService` |
| Methods | PascalCase, verb+noun | `CreateCustomerAsync`, `GetCustomersAsync` |
| Private fields | camelCase with `_` prefix | `_salesOrderNumber`, `_database` |
| Local variables / parameters | camelCase | `customerId`, `cancellationToken` |
| Domain IDs | `<Entity>Id` suffix, extends `DomainId` | `CustomerId`, `SalesOrderId` |
| Value objects | Descriptive noun | `RagioneSociale`, `PartitaIva` |
| Commands | Imperative verb phrase | `CreateSalesOrder`, `CloseSalesOrder` |
| Domain events | Past tense | `SalesOrderCreated`, `SalesOrderClosed` |
| Integration events | Past tense (in `BrewUp.Shared.Messages.Events`) | `CustomerCreated`, `BeerUpdated` |
| JSON DTOs (ExternalContracts) | `<Action><Entity>Json` | `CreateCustomerJson`, `CustomerJson` |
| Read model DTOs | Entity name only, in `Dtos/` folder | `Customer`, `SalesOrder` |
| DI helpers | `Add<Layer>` extension on `IServiceCollection` | `AddMasterDataDomain()`, `AddSalesFacade()` |
| Endpoint classes | `<Entity>Endpoint` or `<Entity>Endpoints` | `CustomersEndpoint`, `BeersEndpoints` |
| Test classes | `<Scenario>` (BDD style) | `CreateSalesOrderSuccessfully`, `CloseSalesOrderSuccessfully` |

### Visibility rules
- **`internal sealed`**: domain services, command/event handlers, facade implementations, query implementations, persisters.
- **`public`**: DI helper extension methods, endpoint mapping methods, interfaces, ExternalContracts DTOs, SharedKernel messages.
- **`private`**: aggregate state fields, constructors used only internally by the factory method.
- **`protected`**: parameterless constructor on every DTO and Aggregate (required for MongoDB/EventStore deserialization).

---

## Dependency Injection

### Registration pattern
Each layer exposes one `internal static` extension method. The Facade layer's helper is the **single entry point** called from the composition root:

```
Add<Module>Facade()
  └─ Add<Module>Domain()
  └─ Add<Module>Infrastructure()
  └─ Add<Module>ReadModel()
```

### Service lifetimes
- **`AddScoped`**: domain services, facades, query services, queries, persisters, ACL event handlers.
- **`AddSingleton`**: `IMongoClient` (one connection pool per app).
- **Keyed scoped** (`AddKeyedScoped`): `IPersister` — each module registers under its own key to avoid cross-module collisions:

```csharp
// Infrastructure helper
services.AddKeyedScoped<IPersister, MasterDataPersister>("masterdata");

// Resolved in domain service constructor
internal sealed class CustomerDomainService(
    [FromKeyedServices("masterdata")] IPersister persister, ...)
```

### Command & event handler registration (Muflone helpers)
```csharp
services.AddCommandHandler<CreateSalesOrderCommandHandler>();       // sends via IServiceBus
services.AddDomainEventHandler<SalesOrderCreatedEventHandler>();   // projection
services.AddIntegrationEventHandler<CustomerCreatedEventHandler>(); // ACL / cross-module
```

### Module registration in composition root
Each feature module implements `IModule` and is passed to `RegisterModules()`:
```csharp
public class MasterDataModule : IModule
{
    public bool IsEnabled => true;
    public int Order => 0;

    public IServiceCollection Register(WebApplicationBuilder builder)
    {
        builder.Services.AddMasterDataFacade();
        return builder.Services;
    }

    public WebApplication Configure(WebApplication app)
    {
        app.MapMasterDataEndpoints();
        return app;
    }
}
```

---

## Testing

### Domain tests — `CommandSpecification<TCommand>` (Muflone.SpecificationTests + xUnit)
Use this for every command handler. `Given()` provides prior events, `When()` issues the command, `Expect()` declares the expected domain events:

```csharp
public sealed class CreateSalesOrderSuccessfully : CommandSpecification<CreateSalesOrder>
{
    private readonly SalesOrderId _salesOrderId = new(Guid.CreateVersion7().ToString());
    private readonly SalesOrderNumber _salesOrderNumber = new("SO-1000");
    // ... other private fields

    protected override IEnumerable<DomainEvent> Given() { yield break; } // no prior state

    protected override CreateSalesOrder When() =>
        new(_salesOrderId, _salesOrderNumber, _salesOrderDate,
            _customerId, _customerName, _salesOrderDeliveryDate, _rows, _correlationId);

    protected override ICommandHandlerAsync<CreateSalesOrder> OnHandler() =>
        new CreateSalesOrderCommandHandler(Repository, new NullLoggerFactory());

    protected override IEnumerable<DomainEvent> Expect()
    {
        yield return new SalesOrderCreated(_salesOrderId, _salesOrderNumber, _salesOrderDate,
            _customerId, _customerName, _salesOrderDeliveryDate, _rows, _correlationId);
    }
}
```

When testing a command that requires prior state, yield the earlier events in `Given()`:
```csharp
protected override IEnumerable<DomainEvent> Given()
{
    yield return new SalesOrderCreated(_salesOrderId, ...);
}
```

### Architecture tests — NetArchTest.Rules
Every module must have an architecture test asserting no dependency on sibling modules' internals, and that all namespaces start with `BrewUp.<ModuleName>`:

```csharp
[ExcludeFromCodeCoverage]
public class MasterDataArchitectureTests
{
    [Fact]
    public void Should_MasterDataArchitecture_BeCompliant()
    {
        var result = Types.InAssembly(typeof(MasterDataHelper).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BrewUp.Sales.Domain", "BrewUp.Sales.Facade",
                "BrewUp.Sales.Infrastructure", "BrewUp.Sales.ReadModel")
            .GetResult()
            .IsSuccessful;

        Assert.True(result);
    }
}
```

### Serialization tests
Verify that integration events round-trip correctly through Muflone's serializer:
```csharp
[Fact]
public async Task Can_Serialized_And_Deserialized_IntegrationEvent()
{
    Serializer serializer = new();
    var @event = new CustomerCreated(aggregateId, ragioneSociale, partitaIva,
        BeerConsumerLevel.Teetotaler, indirizzo);

    var serialized   = await serializer.SerializeAsync(@event, CancellationToken.None);
    var deserialized = await serializer.DeserializeAsync<CustomerCreated>(serialized, CancellationToken.None);

    Assert.Equal(@event.BeerConsumerLevel, deserialized!.BeerConsumerLevel);
}
```

