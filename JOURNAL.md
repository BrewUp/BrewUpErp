---
title: BrewUpErp Journal
description: Diario delle sessioni di lavoro sul progetto BrewUpErp
---

## [2026-08-27] - Test E2E dello stack: fix pipeline eventi, currency birre e ordini

**Attività:**
- Verificato che tutto lo stack fosse già attivo (docker backing services, 5 MCP/agent, API :6094, React :5173)
- Diagnostico e risolto il blocco del giro E2E: `POST /v1/sales` restituiva 201 ma l'ordine non compariva mai nel read model
- Ripristinata la pipeline eventi: passaggio dal message bus Azure Service Bus (non raggiungibile) al RabbitMQ locale, riavvio API
- Corretto il read model delle birre: il DTO `Beer` ora salva `PriceCurrency` (prima `ToJson()` hardcodava `string.Empty`), aggiornati `Beer.cs`, `BeerHelper.cs` e il form React `CreateOrderForm` (default currency EUR)
- Azzerata la `LastEventPosition` in MongoDB (era a commit `81524399`, molto oltre l'head reale di KurrentDB `23709`): il replay ha riproiettato tutti i read model
- Test browser E2E automatizzato (Puppeteer): 13/14 check pass, creazione ordine dal form UI verificata via API
- Scritto il report `E2Etest.md` (root repo) per la discussione con Alberto Acerbis
- Committato il fix currency (`0e4f5f2`) e aperta la PR **#5** (`feat/e2e-fixes` → `main`)

**Decisioni:**
- Passaggio a RabbitMQ locale (`UseRMQ: true`, `UseAzureServiceBus: false` in `appsettings.Development.json`) perché Azure Service Bus falliva con `TryAgain (ServiceCommunicationProblem)` nonostante il TCP fosse raggiungibile. Impatto: file git-ignored, solo ambiente locale
- Reset della `LastEventPosition` a `(0,0)` invece di impostarla all'head corrente: replay completo e consistente di tutto l'event store locale. Impatto: una tantum, idempotente (gli handler read model fanno upsert)
- PR creata con l'account `jesuswasrasta`

**Appreso:**
- In Muflone il read model è alimentato da `EventDispatcher` (IHostedService) che si sottoscrive a KurrentDB `$all` dalla posizione persistita (`IEventStorePositionRepository` su MongoDB, db `BrewUp`, collection `LastEventPosition`): se la posizione salvata è oltre l'head reale (event store ricreato o diverso), il dispatcher resta in attesa e nessun evento raggiunge il bus
- L'errore console SignalR `The connection was stopped during negotiation` è un artefatto di React StrictMode (double-mount): la seconda connessione si stabilisce regolarmente (badge dashboard "Live")
- L'errore saga `AggregateNotFoundException (SalesOrderSaga)` su ogni `SalesOrderPlaced` è pre-esistente: il read model Sales pubblica l'evento all'avvio dell'ordine, ma la saga nasce solo via `POST /v1/sagas` con correlationId casuale non allineato

**Da ricordare:**
- API riavviata con `nohup`, log su `/tmp/opencode/brewup-rest.log`
- `E2Etest.md` non è committato (valutare se includerlo nella PR)
- Problemi pre-esistenti non toccati: saga che non completa il ciclo, errori tsc/lint nella feature Chat, arch test MasterData fallisce (assembly McpServer non in output), totali dashboard corrotti nel read model storico
- Lista sales senza sort esplicito: gli ordini nuovi compaiono in fondo alla paginazione

---

**Attività:**
- Avviati i servizi di supporto Docker (KurrentDB, MongoDB sagas/sales, RabbitMQ) con `docker compose -f docker/docker-compose.yml up -d`
- Avviata l'API Rest (`BrewUp.Rest`) su :6094, il frontend React (`BrewReact`) su :5173, il Knowledge Agent su :5005 e i 4 MCP server (Knowledge :5236, MasterData :5007, Sales :5229, Warehouse :5279)
- Risolti i crash all'avvio dell'API e di 3 MCP server causati da configurazione mancante
- Allineate le URL MCP nella config della Rest API alle porte `dotnet run`

**Decisioni:**
- Aggiunta la sezione `BrewUp:FoundryLimits` a `BrewUp.Rest/appsettings.Development.json`: il modulo Chat (`BrewUpChatHelper.cs:71`) lancia `InvalidOperationException` se la sezione non esiste, perché `Get<FoundryLimitsOptions>()` ritorna null quando la sezione è assente (i default della classe non vengono applicati). Impatto: il file è git-ignored, quindi la modifica non finisce in git
- Creati `appsettings.Development.json` per MasterData, Sales e Warehouse McpServer: i loro `appsettings.json` sono stub minimali (solo Logging/AllowedHosts) e in container vengono popolati via `env_file`, quindi con `dotnet run` mancava la sezione `BrewUp:MongoDbSettings` che i rispettivi `Module` richiedono con `throw` esplicito. Impatto: file locali git-ignored, nessuna modifica al repo
- Ripuntate `BrewUp:McpServers:*` e `BrewUp:Mother:A2A:KnowledgeAgentUrl` dalle porte container (8081–8084, agent 8080) alle porte launchSettings (5229/5279/5007/5236, agent 5005), coerenti con l'avvio `dotnet run`. Impatto: in modalità container va ripristinata la configurazione originale

**Appreso:**
- `**/appsettings.Development.json` è git-ignored ovunque nel repo: i McpServer si aspettano la config via `env_file` (`.env`) in container, ma senza di essa `dotnet run` fallisce all'avvio
- I moduli MCP espongono il percorso `/mcp` e un endpoint `/health` di verifica

**Da ricordare:**
- La Rest API usa la connessione MongoDB cloud (`mongodb+srv://...`), non i MongoDB Docker locali
- Se si passano da `dotnet run` a container MCP, riallineare le URL in `appsettings.Development.json` della Rest API (porte 8081–8084)

---
