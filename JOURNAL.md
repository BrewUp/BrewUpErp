---
title: BrewUpErp Journal
description: Diario delle sessioni di lavoro sul progetto BrewUpErp
---

## [2026-08-27] - Avvio completo ambiente BrewUpErp

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
