---
name: rabbitmq-amqp10-dotnet
description: Use when building or reviewing .NET code that talks to RabbitMQ over AMQP 1.0 with the RabbitMQ.AMQP.Client library. Applies to connection setup, topology management, publishing, consuming, settlement, recovery, RPC, streams, and RabbitMQ-specific AMQP 1.0 address patterns.
---

# RabbitMQ AMQP 1.0 for .NET

Use this skill when the task involves RabbitMQ over **AMQP 1.0** in a **.NET / C#** codebase.

This skill is specifically for the **RabbitMQ AMQP 1.0 .NET client** (`RabbitMQ.AMQP.Client`) and RabbitMQ **4.x+**. Do **not** generate code that uses the AMQP 0-9-1 `RabbitMQ.Client` API unless the user explicitly asks for the older protocol.

## Primary goals

- Prefer the RabbitMQ-maintained AMQP 1.0 .NET client and its high-level abstractions.
- Generate idiomatic asynchronous C# with `async` / `await`.
- Use RabbitMQ-specific AMQP 1.0 concepts correctly: addresses, management API, message settlement, recovery, and queue/exchange/binding topology.
- Keep examples production-oriented: durable topology, persistent messages, explicit settlement, graceful shutdown, and idempotent consumers.

## First checks

Before writing code or suggesting refactors:

1. Verify the code is intended for **RabbitMQ 4.x+** and **AMQP 1.0**.
2. Verify the package is **`RabbitMQ.AMQP.Client`**, not `RabbitMQ.Client`.
3. Verify the server-side object model is still RabbitMQ's **exchange / queue / binding** model.
4. Prefer the official client abstractions:
   - `IEnvironment`
   - `IConnection`
   - `IManagement`
   - `IPublisher`
   - `IConsumer`
   - `IRequester`
   - `IResponder`
5. Prefer **all-async** code paths. Avoid sync wrappers and `.Result` / `.Wait()`.

## Protocol and broker assumptions

Treat these as default truths unless the repo shows otherwise:

- RabbitMQ natively supports **AMQP 1.0** on RabbitMQ **4.0+**.
- AMQP 1.0 and AMQP 0-9-1 are different protocols with different client libraries.
- RabbitMQ still routes through **exchanges**, **queues**, and **bindings**; AMQP 1.0 addresses map onto those broker concepts.
- The RabbitMQ AMQP 1.0 client libraries are **safe by default**: they create durable entities and publish persistent messages.
- The client libraries provide **at-least-once** guarantees, so consumer logic must be idempotent.
- Publishers get broker outcomes; consumers must settle messages or they will run out of credits.

## What to generate by default

### Connections and environment

Use the client's environment and connection model:

```csharp
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;

var settings = new ConnectionSettings(new Uri("amqp://guest:guest@localhost:5672"));
IEnvironment environment = AmqpEnvironment.Create(settings);
IConnection connection = await environment.CreateConnectionAsync();
```

Rules:

- Use `AmqpEnvironment.Create(...)` as the entry point.
- Reuse the environment and connection where appropriate instead of opening one connection per operation.
- Close publishers, consumers, and connections explicitly with `CloseAsync()` when the scope ends.
- For secure production deployments, prefer TLS and appropriate SASL authentication.

### Topology management

Prefer the built-in management API instead of instructing users to declare topology elsewhere when the application owns its topology:

```csharp
IManagement management = connection.Management();

await management.Queue("orders")
    .Type(QueueType.QUORUM)
    .DeclareAsync();

await management.Exchange("orders.events")
    .Type(ExchangeType.TOPIC)
    .DeclareAsync();

await management.Binding()
    .SourceExchange("orders.events")
    .DestinationQueue("orders")
    .Key("orders.created")
    .BindAsync();
```

Rules:

- Keep queue and exchange declaration explicit in examples.
- Prefer **quorum queues** for business-critical work queues unless the task clearly needs another queue type.
- When suggesting queue arguments, use the typed management API where possible.
- Do not assume consumers can receive directly from exchanges; consumers consume from **queues**.

### Publishing

Use a publisher builder and publish asynchronously:

```csharp
IPublisher publisher = await connection.PublisherBuilder()
    .Exchange("orders.events")
    .Key("orders.created")
    .BuildAsync();

var message = new AmqpMessage("{\"orderId\":123}");
PublishResult result = await publisher.PublishAsync(message);
```

Rules:

- Prefer configuring the default target at publisher creation time.
- Explain publish outcomes when relevant:
  - `Accepted` means the broker accepted the message.
  - `Rejected` means the broker rejected it.
  - `Released` means it was not accepted for durable handling.
- When generating robust code, inspect the publish result and fail fast on non-accepted outcomes.
- For direct-to-queue publishing, prefer queue-oriented builders or RabbitMQ queue addresses.

### Consuming

Use explicit settlement unless the task explicitly calls for pre-settled behavior:

```csharp
IConsumer consumer = await connection.ConsumerBuilder()
    .Queue("orders")
    .MessageHandler(async (context, message) =>
    {
        string body = message.BodyAsString();

        await ProcessOrderAsync(body);
        await context.AcceptAsync();
    })
    .BuildAndStartAsync();
```

Rules:

- Default to **explicit settlement**.
- Accept only after successful processing.
- Use `Reject` / `RejectAsync` for invalid, unprocessable messages.
- Use `Release` / `ReleaseAsync` to requeue when processing should be retried.
- Make handler logic **idempotent** because at-least-once delivery can produce duplicates.
- Avoid pre-settled consumers unless low-latency fire-and-forget semantics are intentionally required.

### Graceful shutdown

For consumers, prefer quiescing before closing when duplicates matter:

```csharp
consumer.Pause();
while (consumer.UnsettledMessageCount() > 0)
{
    await Task.Delay(250);
}
await consumer.CloseAsync();
```

Rules:

- Pause delivery first.
- Wait for unsettled messages to reach zero when practical.
- Then close the consumer.
- Mention that closing with in-flight unsettled messages can cause redelivery.

## RabbitMQ AMQP 1.0 addressing rules

When prompting or generating code, follow RabbitMQ's AMQP 1.0 model:

- Publishing target can map to an **exchange plus routing key** or directly to a **queue**.
- Receiving source must be a **queue**.
- Do not model RabbitMQ consumption as subscribing to a topic address the way some generic brokers do.

When the codebase already uses helper methods such as `ExchangeAddress` or `QueueAddress`, keep using the established helper conventions. Otherwise prefer the high-level publisher and consumer builders shown above.

## Recovery and resilience

Prefer the client's built-in recovery support over hand-rolled reconnect loops.

Rules:

- If the repository already configures `RecoveryConfiguration`, preserve and extend that approach.
- Enable topology recovery only when the application actually owns and can safely replay topology.
- Keep consumers idempotent and publishers outcome-aware.
- For user-facing guidance, distinguish between:
  - connection recovery
  - topology recovery
  - business-level retry / replay

## RPC patterns

If the task is request-response, prefer the client's requester/responder abstractions instead of inventing a custom correlation loop.

Rules:

- Use `IRequester` and `IResponder` for RPC-style patterns.
- Keep timeouts explicit.
- Avoid using RPC for bulk eventing or fan-out workflows.

## Streams and advanced features

When the task mentions streams or advanced RabbitMQ AMQP 1.0 features:

- Use stream-specific consumer configuration instead of pretending streams are ordinary queues.
- Mention that SQL filter expressions for streams require **RabbitMQ 4.2+**.
- Mention that WebSockets, OAuth2, direct reply-to, node affinity, topology recovery, and pre-settled consumers are supported features in this client.

## Things to avoid

Do **not** do any of the following unless the user explicitly requests them:

- Do not use `RabbitMQ.Client` APIs like `IModel`, `BasicPublish`, `BasicConsume`, or channel-based samples.
- Do not describe AMQP 1.0 as a minor revision of AMQP 0-9-1.
- Do not tell users to consume from an exchange.
- Do not skip settlement in examples unless using a deliberate pre-settled strategy.
- Do not build synchronous wrappers around asynchronous APIs.
- Do not suggest transactions; RabbitMQ's AMQP 1.0 implementation does not support them.
- Do not claim link resume / exactly-once semantics.

## Review checklist

When reviewing code, flag these issues:

- Wrong package or protocol (`RabbitMQ.Client` instead of `RabbitMQ.AMQP.Client`)
- Missing `await` or sync-over-async
- Publisher outcomes ignored in critical paths
- Consumer handler never settles messages
- Settlement performed before business logic completes
- Non-idempotent consumer side effects
- Direct exchange consumption assumptions
- Reconnect logic that duplicates built-in recovery
- Topology declared inconsistently across services
- Missing graceful shutdown for long-running consumers

## Preferred implementation patterns

### Minimal publish/consume setup

1. Create `ConnectionSettings`
2. Create `IEnvironment`
3. Create `IConnection`
4. Use `IManagement` to declare queue / exchange / binding
5. Build `IPublisher`
6. Build and start `IConsumer`
7. Publish messages
8. Accept or release messages explicitly
9. Close resources with `CloseAsync()`

### ASP.NET Core background worker pattern

For hosted services:

- Create the environment and connection once during service startup.
- Keep publisher and consumer instances long-lived.
- Put message processing in a dedicated application service.
- Keep `MessageHandler` thin.
- On shutdown, pause the consumer, drain unsettled work, then close resources.

## Output style for Copilot

When using this skill:

- Prefer concise, production-ready C#.
- Include only the RabbitMQ AMQP 1.0 APIs that are needed.
- Explain any RabbitMQ-specific AMQP 1.0 caveat near the code, not in a long theory section.
- If uncertain whether the repo is AMQP 1.0 or 0-9-1, say so explicitly and inspect package references, namespaces, and sample code before editing.

## Example prompts this skill should help with

- "Add a RabbitMQ AMQP 1.0 publisher using RabbitMQ.AMQP.Client to this worker service."
- "Refactor this RabbitMQ integration from RabbitMQ.Client to RabbitMQ.AMQP.Client."
- "Create a quorum queue, topic exchange, and binding using the management API."
- "Implement a consumer with explicit settlement and graceful shutdown."
- "Use AMQP 1.0 requester/responder for a small RPC flow."
- "Review this RabbitMQ AMQP 1.0 code for settlement, recovery, and topology mistakes."
