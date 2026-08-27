# BrewUp ERP — Full Stack E2E Test Report

**Date:** 2026-08-27
**Author:** OpenCode session with Nando
**Context:** Goal was to bring up the whole BrewUp stack and exercise the web application end to end (create a sales order from the UI and see it flow through the whole pipeline).

---

## 1. Stack status

Everything was already running when the session started; the job became "make the E2E loop actually work".

| Component | Port | Status |
|---|---|---|
| KurrentDB (EventStoreDB fork) | 4113 | up (docker, healthy) |
| MongoDB sagas | 27017 | up (docker) |
| MongoDB sales read model | 37017 | up (docker) |
| RabbitMQ 4.x + stream plugin | 5672 / 5552 / 15672 | up (docker) |
| Knowledge Agent (A2A) | 5005 | up (`dotnet run`) |
| MasterData MCP | 5007 | up |
| Sales MCP | 5229 | up |
| Knowledge MCP | 5236 | up |
| Warehouse MCP | 5279 | up |
| `BrewUp.Rest` API | 6094 | up |
| `BrewReact` (Vite dev) | 5173 | up |

Note: the API runs with `dotnet run --project src/BrewApi/BrewUp.Rest/BrewUp.Rest.csproj --environment Development`. Its `appsettings.Development.json` points the MCP/agent URLs at the `dotnet run` ports above (not the container ports), which is why the MCP servers are run as processes, not containers.

---

## 2. Blockers found and fixed

The web UI and every backend endpoint answered, but **creating a sales order never reached the read model**: `POST /v1/sales` returned `201` yet `GET /v1/sales` never showed the new order. Three separate problems were stacked on top of each other.

### Blocker 1 — Message bus: Azure Service Bus unreachable

**Symptom:** endless `Azure.Messaging.ServiceBus.ServiceBusException: TryAgain (ServiceCommunicationProblem)` on `brewupservicebus.servicebus.windows.net` in the API logs. Order created in the event store, but the events never reached the read-model projections.

**Diagnosis:** `appsettings.Development.json` had `BrewUp:RabbitMQ:UseRMQ: false` and `BrewUp:AzureServiceBus:UseAzureServiceBus: true`. TCP to the Service Bus endpoint was reachable but the AMQP connection kept failing.

**Fix** (`src/BrewApi/BrewUp.Rest/appsettings.Development.json`, git-ignored — local config only):
- `UseRMQ: true`
- `UseAzureServiceBus: false`
- API restarted.

RabbitMQ (already running with the `rabbitmq_stream` plugin enabled) now carries all commands and events. This is the message-bus path the repo supports for local development.

### Blocker 2 — Beer read model dropped the price currency → 400 "Currency is mandatory"

**Symptom:** creating an order from the React form failed with HTTP 400. Reproduced via `curl`:
```
{"errors":{"Rows[0].Price":["Currency is mandatory"],"Rows[0].Price.Currency":["The Currency field is required."]}}
```

**Root cause:** `Beer.ToJson()` built `new Price(Price, string.Empty)` — the `Beer` DTO only stored the numeric `Price` and the currency was **hardcoded to empty** in the read model projection. The React form copies `price.currency` from the beer, so it sent `currency: ""`, which the API validator rejects. (The domain event `BeerHelper.ToBeerCreated/Updated` meanwhile hardcoded `"EUR"` — the two layers were inconsistent.)

**Fix (both layers, as agreed):**
- Backend `src/BrewApi/MasterData/BrewUp.MasterData.Entities/Dtos/Beer.cs`:
  - added `public string PriceCurrency { get; set; } = "EUR";`
  - populated in `Create(...)`, `UpdatePrice(...)`
  - `ToJson()` now returns `new Price(Price, PriceCurrency)`
- Backend `src/BrewApi/MasterData/BrewUp.MasterData.Domain/Helpers/BeerHelper.cs`:
  - `ToBeerCreated` / `ToBeerUpdated` use `beer.PriceCurrency` instead of hardcoded `"EUR"`
- Frontend `src/BrewReact/src/features/sales/components/CreateOrderForm.tsx` (line ~76):
  - `currency: beer?.price.currency || 'EUR'` (defensive default)

After a rebuild/restart, `GET /v1/masterdata/beers` returns `"currency": "EUR"` for every beer.

### Blocker 3 — EventDispatcher was subscribed to an unreachable event-store position

**Symptom:** even with RabbitMQ in place, no message ever appeared on the `brewup.event.exchange` (RabbitMQ `message_stats: none`), so no read-model projection ever ran.

**Root cause (the deep one):** the read model is fed by `Muflone.Eventstore.gRPC.Persistence.EventDispatcher`, an `IHostedService` that subscribes to KurrentDB `$all` **from the last processed position**, which is persisted by `BrewUp.Infrastructure/ReadModel/EventStorePositionRepository.cs` in MongoDB. The stored position was:

```
LastEventPosition (db "BrewUp", _id "EventStoreCommitPosition"):
  CommitPosition: 81524399
  PreparePosition: 81524399
```

But the local KurrentDB `$all` head is **commit 23709 with only 37 events total** (verified with a probe console app using `EventStore.Client.Grpc.Streams` 23.3.9). The stored position came from a previous/different event store and was **81M commits ahead of reality**: the dispatcher subscribed to `$all` at a position the local log will never reach, so it silently received nothing and nothing was ever published to the bus.

**Fix:** reset the persisted position to `(0,0)` and restart the API:
```javascript
db.getSiblingDB("BrewUp").LastEventPosition.updateOne(
  { _id: "EventStoreCommitPosition" },
  { $set: { CommitPosition: 0, PreparePosition: 0 } }
);
```
On restart the dispatcher replayed `$all` from the start; every read model was re-projected and the exchange began receiving events (`publish_in` went from 0 to >0).

**Verification of the whole pipeline after the fixes:**
```
POST /v1/sales            → 201
event store append        → KurrentDB BatchAppend 200
EventDispatcher → RabbitMQ → brewup.event.exchange
read-model handler        → upsert into MongoDB "Sales" db
GET /v1/sales             → new order present
```
New orders now appear in the list. The previously "lost" orders from earlier test runs were recovered by the replay.

---

## 3. Browser E2E results

Automated Puppeteer run against `http://localhost:5173` (Chrome headless):

| Check | Result |
|---|---|
| App loads | PASS |
| Chat page visible (default) | PASS |
| Sidebar: Chat / Dashboard / Sales / Warehouse | PASS |
| Sales page | PASS |
| Open "New Order" form | PASS |
| Select customer | PASS |
| Add order row | PASS |
| Select beer | PASS |
| Submit order | PASS |
| Order created (verified via `/v1/sales`) | PASS |
| Warehouse page | PASS |
| Dashboard page | PASS |
| Console errors | FAIL (benign, see below) |

**13/14 pass.** The single "failure" is a console message:

```
Error: Failed to start the connection: Error: The connection was stopped during negotiation.
```

This is a **React StrictMode artifact**: the dashboard hub effect mounts twice, the first `HubConnection.start()` is aborted by the cleanup, and the second one succeeds. Verified via a network probe: the negotiate request returns 200, the WebSocket connects to `ws://localhost:6094/hubs/dashboards`, and the dashboard badge shows **"Live"**. Not a real defect.

---

## 4. Pre-existing issues found (NOT addressed)

These were confirmed to exist independently of the fixes above (the architecture test was re-run with the changes stashed).

1. **Sales-order saga does not complete.**
   - Symptom: on every order creation the API logs
     `Error while processing message of type SalesOrderPlaced, consumer SalesOrderSagaOrchestrator. The message will be discarded` → `AggregateNotFoundException: Aggregate '<id>' (type SalesOrderSaga) was not found`.
   - Cause: `SalesOrderCreatedForSalesOrderPlacedIntegrationEventHandler` (Sales read model) publishes `SalesOrderPlaced` as soon as the order is created, but the saga aggregate is only created when `POST /v1/sagas` runs `SagasFacade.PlaceSalesOrderAsync`, which generates a **random** correlationId. The saga's id and the `SalesOrderPlaced` correlationId never match, so the orchestrator can't load the saga. Calling `POST /v1/sagas` starts the saga but the order still stays `status: "created"` — the lifecycle never advances to accepted/closed.
   - This is pre-existing design/flow breakage in the WIP sample, not caused by this session.

2. **React TypeScript errors (Chat feature).**
   - `src/BrewReact/src/features/chat/components/BeerCatalog.tsx` — `Property 'name'/'style' does not exist on type 'BeerCatalogItem'`.
   - `src/BrewReact/src/mocks/handlers.ts` — same type mismatch.
   - The `BeerCatalogItem` type expects `name`/`style` while the API payload uses `beerName`/`beerStyle`.

3. **React lint errors (pre-existing, none in the files touched here):**
   - `react-hooks/set-state-in-effect` in `useSalesOrders`
   - `react-refresh/only-export-components` in `ThemeProvider.tsx`
   - unused `within` import in `tests/components/chat/ChatPage.integration.test.tsx`

4. **MasterData architecture test fails:**
   - `MasterDataProjects_Should_Having_Namespace_StartingWith_MasterData` → `System.IO.FileNotFoundException: Could not load file or assembly 'BrewUp.MasterData.McpServer'`. NetArchTest can't resolve the McpServer assembly in the test output. Fails on a clean tree.

5. **Dashboard totals look corrupted** (historical read-model data): e.g. "Sales by Customer" shows `8.927.711.095.052.876,00 EUR`. Likely bad data accumulated in MongoDB, worth a read-model reset/verification.

---

## 5. Files changed in this session

```
src/BrewApi/MasterData/BrewUp.MasterData.Entities/Dtos/Beer.cs         # store + emit PriceCurrency
src/BrewApi/MasterData/BrewUp.MasterData.Domain/Helpers/BeerHelper.cs   # use beer.PriceCurrency in events
src/BrewReact/src/features/sales/components/CreateOrderForm.tsx         # default currency to EUR
src/BrewApi/BrewUp.Rest/appsettings.Development.json                    # git-ignored: UseRMQ=true, UseAzureServiceBus=false
```

Plus a runtime data fix (reset `LastEventPosition` to 0 in MongoDB) and an API restart.

---

## 6. Environment / operational notes

- The API was restarted via `nohup dotnet run ...`; its stdout/OpenTelemetry console goes to `/tmp/opencode/brewup-rest.log`. It is no longer attached to a terminal.
- Backend build: clean (only a pre-existing `NU1608` package-constraint warning).
- `dotnet test src/BrewApi/MasterData/BrewUp.MasterData.Tests` → 2 pass, 1 fail (the pre-existing McpServer arch-test issue above).
- Probe tooling used for diagnosis: `ilspycmd` (installed locally in `/tmp/opencode/tools`) to inspect the Muflone 10.2.2/10.0.1 assemblies, and a small console app (`/tmp/opencode/esprobe`) using `EventStore.Client.Grpc.Streams` 23.3.9 to measure the `$all` head position and subscription behavior.
