<!-- .slide: class="lead" -->

# Da MCP tools a **Distributed Intelligence**

<p class="subtitle">Come DDD, MCP, A2A, RAG, Wiki e observability cambiano il modo in cui progettiamo sistemi AI-native</p>

<p class="ref">Ferdinando Santacroce · Alberto Acerbis<br>InnovAI 2026 · <code>github.com/BrewUp/BrewUpErp</code></p>

Note: [Nando 0:00-1:00] Apertura.
- Laboratorio: ERP coi vincoli del mondo reale (BC, CQRS, eventi); l'AI è arrivata dopo e l'abbiamo messa dentro.
- Promessa: 6 problemi incontrati davvero + 1 decisione per ognuno.
- Domanda: «Quando un agente smette di essere un agente e diventa un monolite con un LLM davanti?» → «A noi è successo al primo tentativo.»
- Handoff: «Alberto, la tesi del talk in una frase.» (lui prende "La tesi")
Da concordare con Alberto: chi tiene il mouse; segnale di passaggio = guardare il pubblico; Nando non interrompe sul codice; il pubblico parla con Nando.

---

## La **tesi**

<p class="big-statement">Progettare un sistema AI-native <br> non significa aggiungere agenti a un'applicazione.</p>

<p class="q">Significa progettare <br> come l'intelligenza fluisce nell'organizzazione.</p>

<ul>
<li class="fragment">Dove vive la conoscenza?</li>
<li class="fragment">Chi ha l'autorità di rispondere?</li>
<li class="fragment">Chi coordina quando servono più contesti?</li>
<li class="fragment">Come sappiamo che ha funzionato davvero?</li>
</ul>

Note: Le 4 domande sono la spina dorsale: ogni esperimento risponde a una. Tenerle come filo conduttore.

---

## Il laboratorio: **BrewUp ERP**

Un ERP didattico costruito con i vincoli del mondo reale: confini espliciti, niente scorciatoie.

<div class="cards">
<div class="card">
<h4>Modular monolith</h4>
<p>Bounded context isolati nello stesso deployable, nessuna dipendenza diretta tra moduli</p>
</div>
<div class="card">
<h4>DDD + CQRS</h4>
<p>Aggregati, eventi di dominio, saghe e read model separati dalle scritture</p>
</div>
<div class="card">
<h4>Event-driven</h4>
<p>Comunicazione solo tramite messaggi espliciti, mai accesso condiviso ai dati</p>
</div>
<div class="card">
<h4>Stack .NET 10</h4>
<p>Muflone, KurrentDB (EventStore), RabbitMQ, MongoDB</p>
</div>
</div>

<p class="ref">Quattro contesti: <b>MasterData</b> · <b>Sales</b> · <b>Warehouse</b> · <b>Knowledge</b><br>tag <code>demo/00-ddd-base</code></p>

Note: I confini esistono PRIMA dell'AI. Non è greenfield per agenti: è un ERP con le sue regole, e l'AI deve rispettarle.

---

## Quattro contesti, quattro linguaggi

<div class="cards">
<div class="card">
<h4>MasterData</h4>
<p>Prodotti, listini, anagrafiche. È la fonte di verità del catalogo.</p>
</div>
<div class="card">
<h4>Sales</h4>
<p>Ordini di vendita, prezzi, sconti, valutazioni commerciali.</p>
</div>
<div class="card">
<h4>Warehouse</h4>
<p>Giacenze, movimenti, disponibilità e impatti sulla produzione.</p>
</div>
<div class="card">
<h4>Knowledge</h4>
<p>Contratti, procedure, documenti: la conoscenza che non sta in una tabella.</p>
</div>
</div>

<p class="q">Nessun contesto "sa tutto". <br> È il vincolo, non il problema.</p>

Note: Assunto chiave: la distribuzione dell'intelligenza non è un ripiego, è la conseguenza naturale dei confini di dominio.

---

<!-- .slide: class="lead" -->

## Creare un agente **è facile**.

## Progettarlo **no**.

Le demo mostrano agenti che funzionano.
La differenza non è nella demo: è in **dove vive la conoscenza**, **chi decide** e **chi coordina**.

Note: Rottura: dalla spettacolarità della demo alla qualità della progettazione.

---

<!-- .slide: class="lead" -->

## La mappa del talk

<p class="map">MCP → confini → conoscenza → agenti → coordinazione → osservabilità</p>

Note: La sequenza è la storia di sviluppo, non una ricostruzione a posteriori → utile il paragone coi commit.

---

## Esperimento 1 — dare all'AI accesso all'ERP

Un solo **MCP Server** che espone un catalogo di tool: le capacità dell'ERP rese scopribili.

<div class="callout">
<b>MCP (Model Context Protocol)</b> è la superficie scopribile: l'agente vede <em>cosa può fare</em>, senza conoscere come è fatto l'ERP dentro.
</div>

<ul>
<li class="fragment">Un server, tanti tool, tante responsabilità</li>
<li class="fragment">Si parte dal caso più semplice: <br> rispondere a domande sui dati</li>
</ul>

<p class="ref">tag <code>demo/01-mcp-monolith</code> — "Add Chat and MCP" · <code>.../AI/BrewUp.AI.McpServer/Tools/BrewUpMcpTools.cs:9</code></p>

Note: DEMO checkout demo/01-mcp-monolith → McpServer unico + tool catalog. Punto di partenza: semplice e funzionante.

---

## Problema: il Tool Catalog diventa un **monolite**

<ul>
<li class="fragment">30+ tool, 4 domini, una sola superficie</li>
<li class="fragment">Il confine del dominio si perde nel catalogo</li>
<li class="fragment">Ownership e manutenzione non sono più chiari</li>
<li class="fragment">Un cambiamento in Sales tocca il server che espone tutto</li>
</ul>

<p class="q">Abbiamo ricreato in AI il monolite <br> che il DDD ci aveva fatto evitare.</p>

Note: Problema di confini, non tecnico. Mostrare il catalogo cresciuto: è la stessa lezione del monolite, applicata all'AI.

[Nando 9:00] Al pubblico: «Chi ha un agente in produzione con più di 20 tool?» (alzare le mani) → «E come lo chiamate quel pezzo di codice?» 2-3 risposte → passo ad Alberto.
Regole: fai la domanda, 3s di silenzio, poi chiama; chi risponde per primo parlerà di nuovo.

---

## Decisione: un **MCP Server per Bounded Context**

<div class="flow">
<span class="node accent">Sales</span>
<span class="arrow">→</span>
<span class="node">Sales MCP</span>
</div>
<div class="flow">
<span class="node accent">Warehouse</span>
<span class="arrow">→</span>
<span class="node">Warehouse MCP</span>
</div>
<div class="flow">
<span class="node accent">MasterData</span>
<span class="arrow">→</span>
<span class="node">MasterData MCP</span>
</div>

Il catalogo torna a coincidere con i confini del **dominio**.

<p class="ref">tag <code>demo/02-mcp-per-bc</code> — monolite rimosso, un MCP per contesto · <code>.../MasterData/BrewUp.MasterData.McpServer/Tools/MasterDataTools.cs:7</code></p>

Note: DEMO checkout demo/02-mcp-per-bc → 3 McpServer separati. L'AI eredita gli stessi confini: nessun DB/servizio condiviso, solo messaggi espliciti.

[Nando 12:30] «Il DDD c'era già: gli agenti vi hanno fatto scoprire il confine, o il DDD ha reso possibile l'AI?» se dice DDD: «E allora perché il primo tentativo è stato un monolite?»
→ chiudi facendo DIRE ad Alberto: «Bounded contexts don't just separate code, they separate knowledge».

---

## Ogni contesto espone il **proprio** catalogo

<div class="cards">
<div class="card">
<h4>Sales MCP</h4>
<p>Ordini, prezzi, valutazioni commerciali. Paginazione per efficienza.</p>
</div>
<div class="card">
<h4>Warehouse MCP</h4>
<p>Giacenze, disponibilità, impatti sulla produzione.</p>
</div>
<div class="card">
<h4>MasterData MCP</h4>
<p>Prodotti e listini, la fonte di verità del catalogo.</p>
</div>
</div>

<p class="q">Un tool appartiene a un contesto, e a uno solo.</p>

Note: Ownership: ogni tool ha un proprietario chiaro → abilita la scoperta selettiva (l'agente vede solo i contesti che gli servono).

---

## Esperimento 2 — chi **coordina** i contesti?

<div class="flow">
<span class="node">Utente</span>
<span class="arrow">→</span>
<span class="node accent">MotherCoordinator</span>
<span class="arrow">→</span>
<span class="node">MasterData</span>
<span class="arrow">·</span>
<span class="node">Sales</span>
<span class="arrow">·</span>
<span class="node">Warehouse</span>
</div>

Un coordinatore che **delega**, non un super-agente che sa tutto.

<p class="ref">tag <code>demo/03-orchestrator</code> — "Introduce agent-based coordination for what-if analysis" · <code>.../Mother.Facade/Agents/MotherCoordinator.cs:63</code></p>

Note: DEMO checkout demo/03-orchestrator → what-if come collaborazione tra contesti. Coordinazione esplicita e tracciabile, non emergente.

[Nando 29:30] «Mother ha un prompt: chi lo scrive? Se lo scrive un umano, Mother non è un agente, è un PM con un LLM.» se risponde bene: «E quando quel prompt sbaglia, di chi è il bug?»

---

## Il *what-if* passo per passo

<div class="flow">
<span class="node">"What if we sell 500 bottles of IPA?"</span>
</div>
<div class="flow">
<span class="node accent">Sales</span><span class="arrow">→</span>
<span class="node">impatto sui ricavi</span>
</div>
<div class="flow">
<span class="node accent">Warehouse</span><span class="arrow">→</span>
<span class="node">disponibilità e produzione</span>
</div>
<div class="flow">
<span class="node accent">Knowledge</span><span class="arrow">→</span>
<span class="node">vincoli e procedure</span>
</div>

<p class="q">Nessun agente risponde da solo: <br> il risultato è una composizione.</p>

Note: Cuore dimostrativo: ogni contesto contribuisce con la propria autorità; la risposta nasce dalla composizione, non da un modello onnisciente.

[Nando 30:30] Al pubblico: «Quale contesto dovrebbe rispondere a "what if we sell 500 bottles of IPA"?» (tipicamente Sales e Warehouse) → ad Alberto: «Perché nessuno dei due può rispondere da solo?» (apre Mother).

---

## Il dominio non vive tutto nel **database**

Contratti, procedure, PDF, wiki operative: conoscenza che non sta in una tabella.

<div class="cards">
<div class="card"><h4>Knowledge Context</h4><p>Un bounded context dedicato alla conoscenza</p></div>
<div class="card"><h4>Ingestion</h4><p>PDF e Markdown, estrazione, chunking</p></div>
<div class="card"><h4>Embedding</h4><p>Azure OpenAI per i vettori</p></div>
<div class="card"><h4>Retrieval</h4><p>Vector store: InMemory, SQL Server, Azure AI Search</p></div>
</div>

<p class="ref">tag <code>demo/04-rag</code> — "RAG completed" · tool <code>search_knowledge_base</code> · <code>.../Knowledge.McpServer/Tools/KnowledgeTools.cs:10</code></p>

Note: DEMO checkout demo/04-rag → nasce il Knowledge Context. RAG con confini di dominio, non un indice unico.

[Nando 14:30] Al pubblico: «Qual è la nostra politica di riordino per una IPA?» (lascia in aria) → ad Alberto: «E quando la risposta non esiste in nessun documento?»

---

## RAG: dalla fonte alla risposta

<div class="flow">
<span class="node">PDF / Markdown</span><span class="arrow">→</span>
<span class="node">chunking</span><span class="arrow">→</span>
<span class="node">embeddings</span><span class="arrow">→</span>
<span class="node accent">vector store</span><span class="arrow">→</span>
<span class="node know">retrieval</span>
</div>

<ul>
<li class="fragment">L'ingestion trasforma documenti in vettori ricercabili</li>
<li class="fragment">Il retrieval restituisce i chunk più <b>simili</b> alla domanda</li>
<li class="fragment">Tutto dentro i confini del Knowledge Context</li>
</ul>

Note: Niente dettagli implementativi. Chiave: il retrieval è similarity, non verità.

---

## Retrieval **non** significa conoscenza

Un chunk recuperato non è una risposta **giustificata**.

<ul>
<li class="fragment">Il retrieval risponde a "cosa è simile"</li>
<li class="fragment">Non risponde a "qual è la fonte"</li>
<li class="fragment">E non dice se il risultato è ancora valido</li>
</ul>

<p class="q">Serve conoscenza derivata, con provenance.</p>

Note: Ponte verso l'LLM Wiki: prima far atterrare il problema, poi la soluzione.

---

## Un tool non è un agente

<div class="cards">
<div class="card"><h4>MCP</h4><p>"What can you do?" — espone capacità, non si assume obiettivi</p></div>
<div class="card"><h4>A2A</h4><p>"Can you take responsibility?" — assume un obiettivo e lo porta a termine</p></div>
</div>

Il **Knowledge Agent**: dal tool al collaboratore.

<p class="ref">tag <code>demo/05-a2a</code> — A2A senza framework · <code>.../BrewUp.Shared/Agents/AgentCard.cs:3</code> · <code>.../Knowledge.Facade/Agents/KnowledgeAgentCardProvider.cs:7</code></p>

Note: DEMO checkout demo/05-a2a → A2A fatto a mano per capire il protocollo; il framework Microsoft arriva dopo.

[Nando 25:30] «Avete fatto A2A a mano col framework MS già disponibile: perché?» (la risposta è nello storico: prima il protocollo, poi lo strumento) → poi: «Cosa ha fatto il framework che il vostro codice non faceva?»

---

## A2A: agenti che si parlano

<ul>
<li class="fragment">Ogni agente pubblica una <b>AgentCard</b>: identità e capacità</li>
<li class="fragment">Un agente scopre un altro agente e gli <b>delega</b> un obiettivo</li>
<li class="fragment">Il confine resta esplicito: è un <b>handoff</b>, non una chiamata di funzione</li>
</ul>

<div class="callout">
Il Knowledge Agent espone se stesso via A2A e a sua volta usa MCP per parlare col Knowledge Context.
</div>

Note: MCP è verticale (agente→capacità), A2A è orizzontale (agente→agente). Passaggio da tool a collaboratore.

---

<!-- .slide: class="lead" -->

## Il sistema cresce

Orchestrazione e trasporto: **Aspire** + A2A su HTTP + Service Bus.

<div class="callout">
Il protocollo stesso si evolve: l'aggiornamento a <b>MCP 2.0</b> arriva dopo, insieme alla parte finale del sistema. Nell'ordine narrativo resta un <b>aggiornamento</b>, non un nuovo passo.
</div>

<p class="ref"><code>demo/05-a2a</code> → <code>demo/06-aspire</code> → <code>demo/08-llm-wiki</code> · <code>.../Mother.Facade/Mcp/McpToolsProvider.cs:52</code> · <code>.../BrewUp.Shared/Agents/McpToolClient.cs:25</code></p>

Note: Opzionale. Il sistema diventa distribuito: processi separati, comunicazione esplicita, orchestrazione dei servizi. Se chiedono di MCP: intro a maggio (demo/01-mcp-monolith), update 2.0 ad agosto (demo/08-llm-wiki).

---

## Ha funzionato **non è abbastanza**

<div class="cards">
<div class="card">
<h4>Technical observability</h4>
<p>Latenza, span, errori. Sappiamo <em>che</em> è successo.</p>
</div>
<div class="card">
<h4>Semantic observability</h4>
<p>AgentRun, workflow, capability, handoff, outcome. Sappiamo <em>cosa</em> e <em>perché</em>.</p>
</div>
</div>

<p class="q">Apriamo il trace.</p>

<p class="ref">tag <code>demo/07-observability</code> — telemetria semantica nel sistema finale · <code>.../Mother.Facade/Telemetry/MotherTelemetry.cs:28</code></p>

Note: DEMO checkout demo/07-observability → apri il trace: non solo i tempi, ma chi ha parlato con chi, quale capability, quale handoff, con quale esito.

---

## Cosa tracciamo

<ul>
<li class="fragment"><b>AgentRun</b>: l'esecuzione di un agente dall'inizio alla fine</li>
<li class="fragment"><b>Workflow</b>: la sequenza di passi che compongono un what-if</li>
<li class="fragment"><b>Capability</b>: quale capacità è stata invocata e con quale esito</li>
<li class="fragment"><b>Handoff</b>: il passaggio di responsabilità tra agenti</li>
<li class="fragment"><b>Outcome</b>: il risultato finale, valutabile</li>
</ul>

<p class="q">Osservabilità semantica = capire, non solo misurare.</p>

Note: Chiude il cerchio con la tesi: se l'intelligenza fluisce tra confini, devi poter vedere il flusso.

[Nando 35:00] Per chi non ha mai aperto un trace: «Non vedete solo servizi che si chiamano: vedete chi ha parlato con chi, quale capacità ha usato, con quale esito.»

---

## LLM Wiki: conoscenza derivata con **provenance**

Documenti → Wiki generata → risposta con **riferimenti alla fonte**.

<div class="flow">
<span class="node know">search_knowledge_base</span>
<span class="node know">query_wiki</span>
<span class="node know">get_wiki_page</span>
<span class="node know">get_wiki_page_evidence</span>
</div>

<p class="ref">tag <code>demo/08-llm-wiki</code> — il finale · KnowledgeTools · <code>.../Knowledge.McpServer/Tools/KnowledgeTools.cs:35,36</code></p>

Note: DEMO checkout demo/08-llm-wiki → i 4 tool del Knowledge. Chiave: ogni pagina wiki porta l'evidenza delle fonti (provenance).

---

## Perché una *Wiki* e non solo RAG

<ul>
<li class="fragment">La conoscenza è <b>derivata</b>, non solo recuperata</li>
<li class="fragment">Ogni pagina mantiene i <b>riferimenti</b> alle fonti (provenance)</li>
<li class="fragment">Si aggiorna in modo controllato, non a ogni query</li>
<li class="fragment">L'agente può <b>citare</b> la fonte, non solo rispondere</li>
</ul>

<p class="q">Una risposta senza fonte è un'opinione.</p>

Note: Pattern LLM Wiki (Karpathy): conoscenza strutturata e verificabile dai documenti, non solo similarity. Salto di qualità sul RAG puro.

[Nando 23:00] «La wiki la genera l'LLM: chi approva una pagina? Se nessun umano firma, come distingui una pagina affidabile da una inventata?» → «Derived knowledge is not operational truth: e se una pagina wiki contraddice lo stock?» → al pubblico: «Chi vorrebbe un sistema che risponde con una procedura che nessuno approva?»

---

## Evaluation

Non basta che il sistema risponda: deve rispondere **bene**.

<ul>
<li class="fragment"><b>KnowledgeRetrievalEvaluator</b>: misura la qualità del retrieval</li>
<li class="fragment">Valutazione continua, non una demo una tantum</li>
<li class="fragment">La qualità della conoscenza è un requisito, non un extra</li>
</ul>

<div class="callout">
L'osservabilità dice <b>cosa</b> ha fatto il sistema. L'evaluation dice se <b>era sufficiente</b>.
</div>

<p class="ref">tag <code>demo/09-evaluation</code> · <code>.../Knowledge.Facade/Evaluation/KnowledgeRetrievalEvaluator.cs:10</code> · <code>.../Mother.Facade/Agents/WhatIfWorkflowEvaluator.cs:45</code></p>

Note: Come sappiamo che è migliorato? L'evaluation chiude il ciclo con l'osservabilità.

[Nando 37:30] «L'evaluation la scrivete voi o la genera il modello? Se la genera il modello, non è un bias che valuta sé stesso?» → «Osservability dice cosa ha fatto il sistema. Evaluation dice se bastava. Chi decide se bastava?»

---

## Il sistema finale: **intelligence flow**

<div class="flow">
<span class="node accent">MotherCoordinator</span>
</div>
<div class="flow">
<span class="node">MasterData</span><span class="arrow">→</span>
<span class="node">Sales</span><span class="arrow">→</span>
<span class="node">Warehouse</span><span class="arrow">→</span>
<span class="node know">Knowledge</span><span class="arrow">→</span>
<span class="node">Evaluation</span>
</div>

- **Bounded Contexts** con MCP Server separati
- **KnowledgeTools** con provenance
- **Knowledge Agent**: Mother → A2A → Agent → MCP → Knowledge

Note: Sintesi: la tesi realizzata — l'intelligenza fluisce tra confini espliciti, non dentro un unico agente.

---

<!-- .slide: class="lead" -->

## Show me the system

Demo end-to-end: **"What if we sell 500 bottles of IPA?"**

<div class="flow">
<span class="node">Mother</span><span class="arrow">→</span>
<span class="node">Sales</span><span class="arrow">→</span>
<span class="node">Warehouse</span><span class="arrow">→</span>
<span class="node know">Knowledge</span><span class="arrow">→</span>
<span class="node">Evaluation</span>
</div>

Note: DEMO finale: lancia il what-if, poi apri il trace. Ogni passo ha un confine, un'autorità, una traccia.

[Nando 10 min] Leggi tu la domanda: «What if we sell 500 bottles of IPA?» (Alberto esegue, tu non tocchi il mouse).
- Commenta ad alta voce l'atteso: «ora MasterData risolve "IPA"... ora Warehouse dice se basta lo stock...» (tiene il pubblico anche se qualcosa va storto).
- Se si rompe: «vediamo come risponde il sistema» (momento onesto, non errore).
- Alla fine apri tu il trace: «Quale delle quattro risposte vi ha sorpreso?».

---

<!-- .slide: class="lead" -->

# Grazie

<p class="ref">
alberto.acerbis@intre.it · ferdinando.santacroce@gmail.com<br>
<code>https://github.com/BrewUp/BrewUpErp</code>
</p>

Note: Q&A. Ringraziare. Repo per i dettagli.

Riserva:
- «Cosa definite sperimentale che in un'azienda vera non passerebbe la review?»
- «Se ricominciassi oggi, cosa rifaresti diversamente?»
- «Quanto è costato rifare l'AI layer col passaggio a MCP 2.0?»
- «Cosa avete costruito che non userete mai in produzione?»
- «Chi ha un sistema agentico in produzione? Cosa è costato di più: costruirlo o farlo restare?»

[Nando 49:00] Chiusura: torna a `User → LLM → Tools` vs schema finale → tesi a memoria: "The real challenge isn't connecting an LLM to some APIs. It's designing how intelligence flows through the organization." → una frase e silenzio.
