# BrewUpErp: sequenza demo

Documento di lavoro per il talk (1nn0vAI). Descrive cosa contengono davvero i branch,
la sequenza reale dei commit e i tag proposti per la demo. Generato il 2026-09-24.

Riferimenti: slide `1nn0vAI_MCP-A2A.pdf` in questa cartella.

---

## 1. Correzione delle premesse

La memoria iniziale era:

- `DDD` = versione base del repository
- `a2a-no-framework` = A2A con il framework Microsoft Agent2Agent (step intermedio)
- `mother-with-mcp-and-rag` = versione finale

Verificato sui commit reali:

- `DDD` = base corretta come contenuto, ma e' uno **snapshot derivato** dalla mainline
  (commit `1cb2f33`), ripulito dall'AI layer con `0be5b7d "Just DDD"`. Contiene gia' gli
  MCP server per BC. Non e' un antenato del resto.
- `a2a-no-framework` **NON** usa il framework Microsoft. Il nome e' letterale: A2A
  implementato a mano (`KnowledgeAgent : IAgent` su `IMcpToolClient`). Il package
  `Microsoft.Agents.AI` 1.19.0 compare solo in `main` e `feature/mcp-2`
  (progetto `BrewUp.Knowledge.Agent`).
- `mother-with-mcp-and-rag` **NON** e' la versione finale: e' il primo step RAG ed e'
  antenato di a2a/main/feature-mcp-2. La finale (con LLM Wiki e observability) e'
  `feature/mcp-2` (28/08), con `main` subito dietro (25/08).

Prova decisiva: i tool delle slide `query_wiki`, `get_wiki_page`, `get_wiki_page_evidence`
esistono **solo** in `feature/mcp-2`. In `mother`, `a2a` e `main` c'e' solo
`search_knowledge_base`.

---

## 2. Ordine reale dei branch (per ancestry)

```
base ... 1cb2f33 ... mother-with-mcp-and-rag --+-- a2a-no-framework (ramo laterale)
                                               |
                                               +-- feature/a2a -- main -- feature/mcp-2

DDD = ramo staccato da 1cb2f33 con l'AI layer rimosso
```

Relazioni verificate con `git rev-list --left-only --count`:

- `mother-with-mcp-and-rag` IS ancestor of `a2a-no-framework`, `feature/a2a`, `main`, `feature/mcp-2`
- `feature/a2a` IS ancestor of `main` e `feature/mcp-2`
- `main` IS ancestor of `feature/mcp-2`
- `a2a-no-framework` NON e' antenato di `main` (differisce solo per il commit `194fad6 Update gitignore`)
- `DDD` NON e' antenato di nulla

Branch piu' vecchi, non in mainline: `feature/aspire` (15/05), `feature/chat-without-mcp` (28/05).

---

## 3. Stato del codice per branch

| Branch / commit | Moduli | Chat/Mother | Knowledge/RAG | Agent/A2A | Altro |
|---|---|---|---|---|---|
| `DDD` | Dashboards, MasterData, Purchases, Sagas, Sales, Warehouse | no | no | no | MCP server per MasterData/Sales/Warehouse |
| `mother-with-mcp-and-rag` | + Chat, Knowledge | si (Mother + agenti per BC) | si | no | RAG: embeddings Azure OpenAI, Azure AI Search, vector store SQL Server, PDF ingestion |
| `a2a-no-framework` | come mother | si | si | `KnowledgeAgent` + `KnowledgeAgentCardProvider` manuali | agenti e AgentCard spostati nei moduli |
| `main` | come sopra + BrewOrchestrator | si | si | `BrewUp.Knowledge.Agent` standalone con `Microsoft.Agents.AI` 1.19.0 | Aspire, React, telemetria semantica, infra/Bicep |
| `feature/mcp-2` | come main | si | si + Wiki | come main | LLM Wiki (Karpathy) + provenance, integration test MCP, prompt fuori dall'assembly |

`feature/aspire` e `feature/chat-without-mcp` sono esperimenti iniziali con un **unico**
MCP server monolitico (`BrewUp.Mcp.McpServer` / `AI/BrewUp.AI.McpServer`).

---

## 4. Cronologia reale (mainline)

| # | Commit | Data | Stato |
|---|---|---|---|
| 0 | `bcfc74c` | 14/05 | DDD puro, nessun MCP ne' Chat |
| 1 | `2877dfa` | 15/05 | un solo MCP server (`AI/BrewUp.AI.McpServer`) + Chat = Esperimento 1, Tool Catalog monolite |
| - | `597054f`..`0962ada` | 15-17/05 | MCP server per BC (Sales, Warehouse, MasterData) |
| - | `cc321d8` / `41e6886` | 17/05 | Mother come coordinatore agenti |
| - | `e218ece`..`6fc0f10` | 27/05-01/06 | Chat con autodiscover dei tool MCP multi-server |
| 2 | `39ada0d` | 03/06 | monolite rimosso, MCP per BC, Chat -> Mother con `McpToolsProvider` |
| 3 | `a889eb6` | 03/06 | agent-based coordination: `MotherCoordinator` + agenti per BC |
| 4 | `1cb2f33` | 04/06 | orchestratore stabile, senza Knowledge |
| 5 | `44bb9f4` | 16/06 | RAG / Knowledge Context (= `mother-with-mcp-and-rag`) |
| 6 | `4e7520d` | 17/06 | agenti/AgentCard nei moduli, A2A senza framework (= `a2a-no-framework`, tip `194fad6`) |
| 7 | `a6abcb9` | 23/07 | Aspire + A2A HTTP + Azure ServiceBus (= `feature/a2a`) |
| 8 | `e0312d1` | 25/08 | Microsoft.Agents.AI + React + observability (= `main`) |
| 9 | `a72e012` | 28/08 | LLM Wiki con provenance (= `feature/mcp-2`) = finale |

Nota: gli step 2 e 3 sono a poche ore di distanza (03/06) e possono essere uniti o
tenuti separati in base al livello di dettaglio desiderato.

---

## 5. Sequenza di checkout consigliata per la presentazione

| Momento della narrazione | Checkout | Cosa mostrare |
|---|---|---|
| Intro progetto (monolite modulare, BC, CQRS) | `bcfc74c` (oppure `DDD`) | struttura ERP, bounded contexts |
| Esperimento 1: MCP monolite + Chat | `2877dfa` | un solo `McpServer`, `BrewUpMcpTools` |
| Decisione: un MCP per BC | `39ada0d` | `BrewUp.{MasterData,Sales,Warehouse}.McpServer`, monolite rimosso |
| Orchestratore centrale (Mother) | `a889eb6` (stabile: `1cb2f33`) | `MotherCoordinator` + `McpToolsProvider` + agenti |
| Knowledge base / RAG | `mother-with-mcp-and-rag` | `Knowledge/`: embeddings, Azure AI Search, SQL Server vector store, `search_knowledge_base` |
| Un tool non e' un agente (A2A) | `a2a-no-framework` | `KnowledgeAgent` + `KnowledgeAgentCardProvider` |
| Aspire + A2A HTTP (opzionale) | `feature/a2a` | `HttpKnowledgeAgentA2aClient`, `BrewOrchestrator.Host` |
| Observability + sistema finale | `main` poi `feature/mcp-2` | telemetria semantica, React; poi LLM Wiki (`query_wiki`/`get_wiki_page`/`get_wiki_page_evidence`) |

---

## 6. Tag proposti (annotati, non ancora creati)

```
demo/00-ddd-base        bcfc74c                        # intro: ERP DDD puro
demo/01-mcp-monolith    2877dfa                        # Esperimento 1: un MCP server
demo/02-mcp-per-bc      39ada0d                        # decisione: un MCP per BC
demo/03-orchestrator    a889eb6                        # Mother + agenti
demo/04-rag             origin/mother-with-mcp-and-rag # Knowledge / RAG
demo/05-a2a             origin/a2a-no-framework        # A2A senza framework
demo/06-aspire          origin/feature/a2a             # Aspire + A2A HTTP
demo/07-observability   origin/main                    # Microsoft A2A + telemetria
demo/08-final           origin/feature/mcp-2           # LLM Wiki: finale
```

Comandi:

```bash
git tag -a demo/00-ddd-base   bcfc74c -m "Demo 00: base DDD, nessun MCP/AI"
git tag -a demo/01-mcp-monolith 2877dfa -m "Demo 01: un solo MCP server + Chat (Esperimento 1)"
git tag -a demo/02-mcp-per-bc 39ada0d -m "Demo 02: monolite rimosso, MCP per Bounded Context"
git tag -a demo/03-orchestrator a889eb6 -m "Demo 03: Mother + agenti (agent-based coordination)"
git tag -a demo/04-rag  origin/mother-with-mcp-and-rag -m "Demo 04: Knowledge Context / RAG"
git tag -a demo/05-a2a  origin/a2a-no-framework -m "Demo 05: A2A senza framework"
git tag -a demo/06-aspire origin/feature/a2a -m "Demo 06: Aspire + A2A HTTP + ServiceBus"
git tag -a demo/07-observability origin/main -m "Demo 07: Microsoft Agents.AI + observability + React"
git tag -a demo/08-final origin/feature/mcp-2 -m "Demo 08: LLM Wiki con provenance (finale)"
```

---

## 7. Stato delle decisioni

- **Ordine del racconto: deciso.** Il deck segue la timeline dei commit:
  `MCP -> per-BC -> Mother -> RAG -> A2A -> Aspire -> observability -> Wiki`. La scaletta
  (`Scaletta-InnovAI.md`) è rimasta sull'ordine precedente (Knowledge prima, Mother in fondo):
  va riallineata a questo.
- **Intro: da chiudere.** Il deck non ha un checkout per l'intro, quindi non la vincola.
  Restano le due opzioni: `bcfc74c` (DDD puro, ideale) oppure il branch `DDD` (snapshot con
  gli MCP già presenti).
- **Granularità dei tag: decisa.** Il deck tratta per-BC e Mother come slide separate, quindi
  `demo/02-mcp-per-bc` e `demo/03-orchestrator` restano separati.
- **Aspire: incluso come opzionale.** Il deck ha la slide breve "Il sistema cresce", quindi il
  tag `demo/06-aspire` resta.
- **Tag: da creare.** I comandi della sezione 6 non sono ancora stati eseguiti.
