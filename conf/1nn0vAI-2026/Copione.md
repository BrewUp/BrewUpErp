# Copione — 1nn0vAI 2026

Documento unico del talk: cosa dire, cosa mostrare e quale checkout fare, blocco per blocco. L'ordine segue il deck (`slides/slides.md`) e la timeline reale dei commit. Il talk dura 50 minuti.

Le slide stanno in `slides/` e si avviano come spiegato in `slides/README.md`.

**Ruoli**

- **Alberto Acerbis**: ideatore e autore della soluzione, protagonista del talk. Spiega l'architettura, guida ed esegue la demo. A lui vanno i meriti.
- **Ferdinando (Nando) Santacroce**: spalla nella conduzione. Apre il talk, cura il rapporto col pubblico, interviene con qualche domanda e legge la domanda del what-if nella demo finale.

## Scaletta a colpo d'occhio

| Tempo | Blocco | Idea centrale | Demo |
| ---: | --- | --- | --- |
| 0–4 | Apertura: tesi, laboratorio, quattro contesti | Progettare come fluisce l'intelligenza | branch `DDD` |
| 4–9 | Esperimento 1: diamo all'AI accesso all'ERP | MCP come superficie delle capability | `2877dfa` |
| 9–14 | Il Tool Catalog diventa un monolite | Un MCP Server per Bounded Context | `39ada0d` |
| 14–19 | Esperimento 2: chi coordina i contesti | Mother delega, non sa tutto | `a889eb6` |
| 19–24 | Il dominio non vive tutto nel database | Knowledge Context + RAG | `mother-with-mcp-and-rag` |
| 24–29 | Retrieval non significa conoscenza | Dal RAG alla LLM Wiki | — |
| 29–34 | Un tool non è un agente | Knowledge Agent, MCP vs A2A | `a2a-no-framework` |
| 34–36 | Il sistema cresce | Aspire + A2A su HTTP | `feature/a2a` (opzionale) |
| 36–39 | "Ha funzionato" non è abbastanza | Observability tecnica e semantica | `main` |
| 39–43 | LLM Wiki con provenance | Conoscenza derivata con le fonti | `feature/mcp-2` |
| 43–44 | Evaluation | Misurare la qualità, non solo l'esito | `feature/mcp-2` |
| 44–49 | Show me the system | Demo end-to-end e trace | sistema finale |
| 49–50 | Conclusione | Progettare il flusso dell'intelligenza | — |

I dettagli dei branch, dei commit e dei tag stanno nelle appendici.

---

## 0–4 — Apertura: tesi, laboratorio, quattro contesti

Apre Nando. Presentazione, poi il laboratorio e la tesi.

BrewUp ERP va introdotto solo quanto basta per capire che esistono più bounded context e che ciascuno possiede una parte diversa del dominio.

La tesi:

> Progettare un sistema AI-native non significa aggiungere agenti a un'applicazione. Significa progettare come l'intelligenza fluisce nell'organizzazione.

Le quattro domande che fanno da spina dorsale:

- Dove vive la conoscenza?
- Chi ha l'autorità di rispondere?
- Chi coordina quando servono più contesti?
- Come sappiamo che ha funzionato davvero?

Quattro contesti, quattro linguaggi: MasterData, Sales, Warehouse, Knowledge. Nessun contesto sa tutto: è il vincolo, non il problema.

Poi la rottura, prima di entrare nel merito:

> Creare un agente è facile. Progettarlo no.

E la domanda che apre la sessione:

> **Quando un agente smette di essere un agente e diventa un monolite con un LLM davanti?**

**Demo (branch `DDD`)**: introduzione concettuale al DDD e alla struttura dell'ERP. Mostrare modular monolith, bounded context, CQRS ed eventi, e i quattro contesti ancora senza AI. Serve a fissare che i confini esistono prima dell'intelligenza.

---

## 4–9 — Esperimento 1: diamo all'AI accesso all'ERP

La prima soluzione è naturale:

**MCP.**

Sales espone gli ordini, Warehouse lo stock, MasterData il catalogo.

Qui pochissimo codice. Basta mostrare che una capability applicativa può diventare una capability scoperta dinamicamente dall'AI.

Il punto non è spiegare l'SDK MCP.

Il punto è:

> **MCP trasforma capacità applicative in capacità scopribili.**

E per qualche minuto sembra davvero che il problema sia risolto.

**Demo (checkout `2877dfa`)**: mostrare il McpServer unico e il Tool Catalog. È il punto di partenza: semplice e funzionante.

---

## 9–14 — Il Tool Catalog diventa un monolite

Se tutte le capability finiscono nello stesso MCP Server, abbiamo solo spostato il problema.

Il server AI comincia a conoscere tutto:

* ordini;
* stock;
* clienti;
* catalogo;
* policy;
* produzione.

Ed ecco il ritorno del DDD.

**Un MCP Server per Bounded Context.**

Non principalmente per motivi infrastrutturali, ma per ownership.

Sales parla il linguaggio di Sales. Warehouse quello del Warehouse. MasterData protegge il proprio modello.

La frase centrale:

> **Bounded Contexts don't just separate code. They separate knowledge.**

È probabilmente uno dei messaggi principali dell'intero talk.

**Demo (checkout `39ada0d`)**: mostrare i tre McpServer separati. L'AI eredita gli stessi confini del dominio: nessuna condivisione di DB o servizi, solo messaggi espliciti.

---

## 14–19 — Esperimento 2: chi coordina i contesti

Con i confini chiari, entra Mother: il coordinatore, non un super-agente che sa tutto.

Domanda:

> **"What if we sell 500 bottles of IPA?"**

Non appartiene a nessun bounded context.

Serve collaborazione:

`MasterData → Sales → Warehouse → Knowledge`

MasterData risolve il prodotto.

Sales interpreta la domanda.

Warehouse valuta l'impatto.

Knowledge recupera policy e regole.

Mother non possiede nessuna di queste verità.

Possiede invece:

* coordinamento;
* execution flow;
* correlation;
* system-level reasoning.

La frase chiave:

> **Bounded contexts own truth. Mother owns coordination.**

E qui emerge la vera evoluzione:

non stiamo più distribuendo soltanto software.

Stiamo distribuendo **conoscenza e reasoning**.

**Demo (checkout `a889eb6`)**: presentare il what-if come collaborazione tra contesti. La coordinazione è esplicita e tracciabile, non emergente.

---

## 19–24 — Il dominio non vive tutto nel database

A questo punto fate una domanda che nessun MCP operativo può risolvere bene:

> "Qual è la nostra politica di riordino per una IPA?"

Warehouse conosce stock e soglie.

Ma le regole aziendali possono vivere in:

* procedure;
* documentazione;
* manuali;
* policy;
* knowledge aziendale.

Nasce quindi il **Knowledge Context**.

Qui entra il RAG:

`documents → chunks → embeddings → vector search → evidence`

Il punto architetturale importante è che anche questa conoscenza viene classificata per scope:

* General
* Sales
* Warehouse
* MasterData
* Production

Quindi persino la documentazione continua a rispettare i confini del dominio.

**Demo (checkout `mother-with-mcp-and-rag`)**: qui nasce il Knowledge Context. Mostrare RAG con confini di dominio, non un indice unico su tutto.

---

## 24–29 — Retrieval non significa conoscenza

Il RAG sa recuperare fonti rilevanti.

Ma la domanda successiva è:

> **Vogliamo cercare documenti ogni volta, o vogliamo costruire una rappresentazione più stabile di ciò che l'organizzazione sa?**

Un chunk recuperato non è una risposta giustificata.

Il retrieval risponde a "cosa è simile". Non risponde a "qual è la fonte". E non dice se il risultato è ancora valido.

> **Derived knowledge is not operational truth.**

Stock, ordini e disponibilità rimangono responsabilità dei rispettivi bounded context. La soluzione a questa domanda arriva più avanti, dopo l'observability.

(nessuna demo: è un blocco concettuale che prepara la Wiki in coda)

---

## 29–34 — Un tool non è un agente

A questo punto il Knowledge Context non espone più soltanto una ricerca.

Ha:

* capability proprie;
* discovery;
* regole;
* gestione degli errori;
* responsabilità;
* autonomia evolutiva.

Quindi la domanda diventa:

> **Stiamo ancora chiamando uno strumento oppure stiamo delegando un compito?**

Qui entra A2A.

Il passaggio nel repo è molto chiaro:

`Mother → A2A → Knowledge Agent → MCP → Knowledge`

Ed è qui che entra Microsoft Agent Framework.

La distinzione concettuale che userei è:

> **MCP: What can you do?**
>
> **A2A: Can you take responsibility for this task?**

Non come definizione normativa, ma come modello mentale per l'architettura.

**Demo (checkout `a2a-no-framework`)**: qui A2A è implementato a mano per capire il protocollo. Il framework arriva dopo, quando il concetto è chiaro.

---

## 34–36 — Il sistema cresce

Il sistema diventa distribuito davvero: processi separati, comunicazione esplicita, orchestrazione.

**Aspire** per l'orchestrazione e il trasporto: Aspire + A2A su HTTP + Service Bus.

Il protocollo stesso si evolve: l'aggiornamento a **MCP 2.0** arriva dopo, insieme alla parte finale del sistema. Nell'ordine narrativo resta un aggiornamento, non un nuovo passo.

**Demo (checkout `feature/a2a`, opzionale)**: `HttpKnowledgeAgentA2aClient`, `BrewOrchestrator.Host`.

---

## 36–39 — "Ha funzionato" non è abbastanza

Qui dividiamo l'observability nei due livelli implementati.

#### Technical observability

Risponde a:

> **What happened?**

HTTP call, latency, SQL, MCP invocation, model call, errori, token, retrieval.

È l'observability tradizionale applicata a un sistema agentico.

#### Semantic observability

Risponde invece a:

> **Why did the system do that?**

Qui entrano:

`AgentRun`

`invoke_workflow`

`invoke_agent`

`capability`

`handoff`

`outcome`

Non state più osservando solo servizi. State osservando **decisioni e collaborazione**.

**Demo (checkout `main`)**: mostrare il trace. Non solo i tempi, ma chi ha parlato con chi, quale capability è stata usata, quale handoff è avvenuto, con quale esito.

---

## 39–43 — LLM Wiki con provenance

Riprendiamo il problema lasciato aperto: il retrieval risponde a "cosa è simile", non a "qual è la fonte".

Da qui la LLM Wiki, la rappresentazione più stabile di ciò che l'organizzazione sa:

`Document → Chunk → Wiki Page → Claim → Evidence`

Una Wiki Page non è "vera perché l'ha generata l'LLM". Ogni claim conserva il collegamento alle fonti.

I quattro tool del Knowledge:

```text
search_knowledge_base
query_wiki
get_wiki_page
get_wiki_page_evidence
```

Perché una Wiki e non solo RAG:

- la conoscenza è derivata, non solo recuperata;
- ogni pagina mantiene i riferimenti alle fonti (provenance);
- si aggiorna in modo controllato, non a ogni query;
- l'agente può citare la fonte, non solo rispondere.

> Una risposta senza fonte è un'opinione.

**Demo (checkout `feature/mcp-2`)**: mostrare i quattro tool. Differenza chiave: ogni pagina wiki porta con sé l'evidenza delle fonti da cui è derivata.

---

## 43–44 — Evaluation

Non basta che il sistema risponda: deve rispondere bene.

Non basta sapere che quattro agenti hanno risposto. Bisogna sapere se:

* il prodotto è stato risolto;
* Sales ha prodotto evidenza;
* Warehouse ha prodotto evidenza;
* Knowledge ha prodotto evidenza;
* il workflow può essere considerato completo.

- **KnowledgeRetrievalEvaluator**: misura la qualità del retrieval.
- Valutazione continua, non una demo una tantum.
- La qualità della conoscenza è un requisito, non un extra.

> **Observability tells us what the system did. Evaluation tells us whether what it did was enough.**

(nessuna demo dedicata: si chiude nel trace del blocco precedente)

---

## 44–49 — Show me the system

Questa deve essere una parte vera del talk, non un'appendice.

Dopo 44 minuti di evoluzione:

> **Enough diagrams. Let's see what we actually built.**

### 44–46 — Il codice finale

Pochissimi file, non un tour della solution. Quattro punti.

**1. Struttura dei bounded context**

Sales, Warehouse, MasterData, Knowledge con i rispettivi MCP Server.

**2. `KnowledgeTools`**

Per mostrare che ormai ci sono capability diverse:

```text
search_knowledge_base
query_wiki
get_wiki_page
get_wiki_page_evidence
```

**3. Knowledge Agent**

Per rendere concreta la catena:

```text
Mother
  ↓ A2A
Knowledge Agent
  ↓ MCP
Knowledge Context
```

**4. MotherCoordinator**

Solo il tratto significativo:

```text
MasterDataAgent
→ SalesAgent
→ WarehouseAgent
→ KnowledgeAgent
→ Evaluation
```

Il codice deve servire a far dire al pubblico:

> "Ah, quindi quello che ci hanno raccontato esiste davvero nella codebase."

### 46–48 — Demo end-to-end

Una sola domanda, non quattro demo:

> **"What if we sell 500 bottles of IPA?"**

La domanda attraversa tutta l'architettura. Il pubblico vede:

**MasterData** risolvere "IPA" nel prodotto corretto.

**Sales** interpretare il demand signal.

**Warehouse** verificare disponibilità e soglie.

**Knowledge** recuperare le regole aziendali rilevanti.

**Mother** correlare il risultato e produrre la risposta.

Questa singola demo riassume praticamente tutto il talk.

### 48–49 — Guardiamo cosa è successo davvero

Subito dopo la risposta, senza cambiare scenario, aprire la Aspire dashboard o il trace.

```text
AgentRun
 └─ invoke_workflow brewup.what-if
     ├─ invoke_agent MasterDataAgent
     ├─ invoke_agent SalesAgent
     ├─ invoke_agent WarehouseAgent
     ├─ invoke_agent KnowledgeAgent
     └─ evaluation
```

Questo chiude il cerchio. Prima abbiamo visto la risposta. Adesso vediamo come è stata costruita. E infine se avevamo abbastanza evidenza per fidarci del risultato.

---

## 49–50 — Conclusione

Tornerei all'immagine iniziale.

All'inizio avevamo:

```text
User → LLM → Tools
```

Alla fine abbiamo:

```text
User
  ↓
Coordinator
  ↓
Agents
  ↓
Bounded Contexts
  ↓
Operational + Derived Knowledge
```

E chiuderei con la tesi dell'abstract:

> **The real challenge isn't connecting an LLM to some APIs.**
>
> **It's designing how intelligence flows through the organization.**

---

## Appendice A — Cosa contengono i branch

| Branch / commit | Moduli | Chat/Mother | Knowledge/RAG | Agent/A2A | Altro |
|---|---|---|---|---|---|
| `DDD` | Dashboards, MasterData, Purchases, Sagas, Sales, Warehouse | no | no | no | MCP server per MasterData/Sales/Warehouse |
| `mother-with-mcp-and-rag` | + Chat, Knowledge | si (Mother + agenti per BC) | si | no | RAG: embeddings Azure OpenAI, Azure AI Search, vector store SQL Server, PDF ingestion |
| `a2a-no-framework` | come mother | si | si | `KnowledgeAgent` + `KnowledgeAgentCardProvider` manuali | agenti e AgentCard spostati nei moduli |
| `main` | come sopra + BrewOrchestrator | si | si | `BrewUp.Knowledge.Agent` standalone con `Microsoft.Agents.AI` 1.19.0 | Aspire, React, telemetria semantica, infra/Bicep |
| `feature/mcp-2` | come main | si | si + Wiki | come main | LLM Wiki (Karpathy) + provenance, integration test MCP, prompt fuori dall'assembly |

`feature/aspire` e `feature/chat-without-mcp` sono esperimenti iniziali con un **unico** MCP server monolitico (`BrewUp.Mcp.McpServer` / `AI/BrewUp.AI.McpServer`).

`DDD` è uno snapshot derivato da `1cb2f33`, ripulito dall'AI layer con `0be5b7d "Just DDD"`. Contiene già gli MCP server per BC. Non è un antenato del resto.

---

## Appendice B — Cronologia reale dei commit

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

---

## Appendice C — Ordine reale dei branch

```
base ... 1cb2f33 ... mother-with-mcp-and-rag --+-- a2a-no-framework (ramo laterale)
                                               |
                                               +-- feature/a2a -- main -- feature/mcp-2

DDD = ramo staccato da 1cb2f33 con l'AI layer rimosso
```

Relazioni verificate con `git rev-list --left-only --count`:

- `mother-with-mcp-and-rag` è ancestor di `a2a-no-framework`, `feature/a2a`, `main`, `feature/mcp-2`;
- `feature/a2a` è ancestor di `main` e `feature/mcp-2`;
- `main` è ancestor di `feature/mcp-2`;
- `a2a-no-framework` non è antenato di `main` (differisce solo per il commit `194fad6 Update gitignore`);
- `DDD` non è antenato di nulla.

Branch più vecchi, non in mainline: `feature/aspire` (15/05), `feature/chat-without-mcp` (28/05).

---

## Appendice D — Tag della demo

Non ancora creati.

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

## Appendice E — Stato delle decisioni

- **Ordine del racconto: deciso.** Segue la timeline dei commit e il deck.
- **Intro: decisa.** Concetti generali del DDD, con checkout del branch `DDD`.
- **Granularità dei tag: decisa.** `demo/02-mcp-per-bc` e `demo/03-orchestrator` restano separati.
- **Aspire: incluso come opzionale.** Slide breve, tag `demo/06-aspire`.
- **Tag: da creare.** Vedi Appendice D.
