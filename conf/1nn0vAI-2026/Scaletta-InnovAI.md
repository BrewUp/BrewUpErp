## Scaletta — 50 minuti

| Tempo | Sezione                                                   | Idea centrale                                                   |
| ----: | --------------------------------------------------------- | --------------------------------------------------------------- |
|   0–4 | **Creare un agente è facile. Progettarlo no.**            | L’anti-pattern del chatbot con una collezione crescente di tool |
|   4–9 | **Esperimento #1 — Diamo all’AI accesso all’ERP**        | MCP come superficie delle capability                            |
|  9–14 | **Problema #1 — Stiamo costruendo un nuovo monolite**    | Un MCP per Bounded Context                                      |
| 14–19 | **Problema #2 — Il dominio non vive tutto nel database** | Knowledge Context + RAG                                         |
| 19–24 | **Problema #3 — Retrieval non significa conoscenza**     | Dal RAG alla LLM Wiki                                           |
| 24–29 | **Problema #4 — Un tool non è un agente**                | Knowledge Agent, Agent Card, MCP vs A2A                         |
| 29–34 | **Problema #5 — Chi coordina tutto questo?**             | Mother e la collaborazione tra contesti                         |
| 34–39 | **Problema #6 — “Ha funzionato” non è abbastanza**       | Technical observability, semantic observability, evaluation     |
| 39–49 | **Show me the system**                                    | Codice finale + demo end-to-end + trace                         |
| 49–50 | **Conclusione**                                           | Progettare il flusso dell’intelligenza                          |

---

### 0–4 — Creare un agente è facile. Progettarlo no.

Partenza volutamente semplice.

Un LLM, una chat, qualche tool.

Poi Sales, Warehouse, MasterData.

Finché le capacità sono poche, tutto sembra funzionare. Ma il punto della sessione è proprio mostrare che la complessità arriva dopo.

La domanda iniziale può essere:

> **Quando un agente smette di essere un agente e diventa un monolite con un LLM davanti?**

BrewUp ERP va introdotto solo quanto basta per capire che esistono più bounded context e che ciascuno possiede una parte diversa del dominio.

Prima idea da fissare:

> **Business truth belongs to a Bounded Context.**

---

### 4–9 — Esperimento #1: diamo all’AI accesso all’ERP

La prima soluzione è naturale:

**MCP.**

Sales espone gli ordini, Warehouse lo stock, MasterData il catalogo.

Qui pochissimo codice. Basta mostrare che una capability applicativa può diventare una capability scoperta dinamicamente dall’AI.

Il punto non è spiegare l’SDK MCP.

Il punto è:

> **MCP trasforma capacità applicative in capacità scopribili.**

E per qualche minuto sembra davvero che il problema sia risolto.

---

### 9–14 — Problema #1: stiamo costruendo un nuovo monolite

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

È probabilmente uno dei messaggi principali dell’intero talk.

---

### 14–19 — Problema #2: il dominio non vive tutto nel database

A questo punto fate una domanda che nessun MCP operativo può risolvere bene:

> “Qual è la nostra politica di riordino per una IPA?”

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

---

### 19–24 — Problema #3: retrieval non significa conoscenza

Il RAG sa recuperare fonti rilevanti.

Ma la domanda successiva è:

> **Vogliamo cercare documenti ogni volta, o vogliamo costruire una rappresentazione più stabile di ciò che l’organizzazione sa?**

Da qui la LLM Wiki.

Qui mostrerei soprattutto l’evoluzione concettuale:

**RAG**

> Find me evidence that may answer this question.

**LLM Wiki**

> Build and maintain an interpretable body of domain knowledge.

Nel vostro caso:

`Document → Chunk → Wiki Page → Claim → Evidence`

La parte più importante è la provenance.

Una Wiki Page non è “vera perché l’ha generata l’LLM”.

Ogni claim conserva il collegamento alle fonti.

E soprattutto:

> **Derived knowledge is not operational truth.**

Stock, ordini, disponibilità rimangono responsabilità dei rispettivi bounded context.

---

### 24–29 — Problema #4: un tool non è un agente

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
> **A2A: Can you take responsibility for this task?**

Non come definizione normativa, ma come modello mentale per l’architettura.

---

### 29–34 — Problema #5: chi coordina tutto questo?

Ora finalmente entra Mother.

Non all’inizio.

Soltanto quando il pubblico ha già capito perché serve.

Domanda:

> **“What if we sell 500 bottles of IPA?”**

Non appartiene a nessun bounded context.

Serve collaborazione:

`MasterData → Sales → Warehouse → Knowledge`

MasterData risolve il prodotto.

Sales interpreta la domanda.

Warehouse valuta l’impatto.

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

---

### 34–39 — Problema #6: “ha funzionato” non è abbastanza

Qui dividerei chiaramente l’observability nei due livelli che avete implementato.

#### Technical observability

Risponde a:

> **What happened?**

HTTP call, latency, SQL, MCP invocation, model call, errori, token, retrieval.

È l’observability tradizionale applicata a un sistema agentico.

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

Non state più osservando solo servizi.

State osservando **decisioni e collaborazione**.

E poi arriva l’evaluation.

Non basta sapere che quattro agenti hanno risposto.

Bisogna sapere se:

* il prodotto è stato risolto;
* Sales ha prodotto evidenza;
* Warehouse ha prodotto evidenza;
* Knowledge ha prodotto evidenza;
* il workflow può essere considerato completo.

Frase forte:

> **Observability tells us what the system did. Evaluation tells us whether what it did was enough.**

---

# 39–49 — Show me the system

Questa deve essere una parte vera del talk, non un’appendice.

Dopo 39 minuti di evoluzione:

> **Enough diagrams. Let's see what we actually built.**

## 39–42 — Il codice finale

Tre minuti, pochissimi file.

Non un tour della solution.

Mostrerei soltanto quattro punti.

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

> “Ah, quindi quello che ci hanno raccontato esiste davvero nella codebase.”

---

## 42–47 — Demo end-to-end

Una sola domanda.

Non quattro demo.

Per esempio:

> **“What if we sell 500 bottles of IPA?”**

La domanda deve attraversare tutta l’architettura.

Il pubblico vede:

**MasterData**

risolvere “IPA” nel prodotto corretto.

**Sales**

interpretare il demand signal.

**Warehouse**

verificare disponibilità e soglie.

**Knowledge**

recuperare le regole aziendali rilevanti.

**Mother**

correlare il risultato e produrre la risposta.

Questa singola demo riassume praticamente tutto il talk.

---

## 47–49 — Guardiamo cosa è successo davvero

Subito dopo la risposta, senza cambiare scenario:

**Aspire dashboard / trace.**

Aprire la stessa richiesta.

Mostrare qualcosa come:

```text
AgentRun
 └─ invoke_workflow brewup.what-if
     ├─ invoke_agent MasterDataAgent
     ├─ invoke_agent SalesAgent
     ├─ invoke_agent WarehouseAgent
     ├─ invoke_agent KnowledgeAgent
     └─ evaluation
```

Questo chiude perfettamente il cerchio.

Prima abbiamo visto:

**la risposta.**

Adesso vediamo:

**come è stata costruita.**

E infine:

**se avevamo abbastanza evidenza per fidarci del risultato.**

---

### 49–50 — Conclusione

Tornerei all’immagine iniziale.

All’inizio avevamo:

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

E chiuderei con la tesi dell’abstract:

> **The real challenge isn't connecting an LLM to some APIs.**
>
> **It's designing how intelligence flows through the organization.**
