<!-- .slide: class="lead" -->

# Da MCP tools a **Distributed Intelligence**

<p class="subtitle">Come DDD, MCP, A2A, RAG, Wiki e observability cambiano il modo in cui progettiamo sistemi AI-native</p>

<p class="ref">Ferdinando Santacroce · Alberto Acerbis<br>InnovAI 2026 · <code>github.com/BrewUp/BrewUpErp</code></p>

Note: **Intervento Nando (0:00-1:00)** — Nando tiene questa slide. Non leggere: imparare i cinque passaggi, poi guardare il pubblico. Cinque secondi di silenzio dopo la domanda.

**1. Presentarsi.** "Siamo Ferdinando Santacroce e Alberto Acerbis."

**2. Il laboratorio.** "BrewUp è un ERP costruito con i vincoli del mondo reale: bounded context, CQRS, comunicazione a eventi. L'intelligenza artificiale è arrivata dopo, e noi abbiamo deciso di metterla dentro."

**3. La promessa.** "Non vi raccontiamo come si usa MCP o A2A. Vi raccontiamo sei problemi che li abbiamo incontrati davvero, e la decisione che abbiamo preso per ognuno."

**4. La domanda.** "Quando un agente smette di essere un agente e diventa un monolite con un LLM davanti?" Poi la spinta: "A noi è successo al primo tentativo."

**5. Il passaggio di mano.** Guardare Alberto e cedergli la parola: "Alberto, la tesi del talk in una frase." Lui prende la slide "La tesi" e prosegue da lì.

Testo integrale, da imparare a memoria:

"Siamo Ferdinando Santacroce e Alberto Acerbis.

BrewUp è un ERP costruito con i vincoli del mondo reale: bounded context, CQRS, comunicazione a eventi. L'intelligenza artificiale è arrivata dopo, e noi abbiamo deciso di metterla dentro.

Non vi raccontiamo come si usa MCP o A2A. Vi raccontiamo sei problemi che li abbiamo incontrati davvero, e la decisione che abbiamo preso per ognuno.

La domanda da cui partiamo è semplice: quando un agente smette di essere un agente e diventa un monolite con un LLM davanti?

A noi è successo al primo tentativo.

Alberto, la tesi del talk in una frase."

**Prima di salire, da concordare con Alberto**

- chi tiene il mouse e chi parla durante la demo
- il segnale per passarsi la parola: guardare il pubblico, non toccare il microfono
- tu non interrompi mai durante il codice; lui chiude ogni risposta agganciando la scaletta
- il pubblico parla con te, non con Alberto: sei tu il contraltare

---

## La **tesi**

<p class="big-statement">Progettare un sistema AI-native non significa aggiungere agenti a un'applicazione.</p>

<p class="q">Significa progettare come l'intelligenza fluisce nell'organizzazione.</p>

<ul>
<li class="fragment">Dove vive la conoscenza?</li>
<li class="fragment">Chi ha l'autorità di rispondere?</li>
<li class="fragment">Chi coordina quando servono più contesti?</li>
<li class="fragment">Come sappiamo che ha funzionato davvero?</li>
</ul>

Note: Le quattro domande sono la spina dorsale. Ogni esperimento che segue risponde a una di esse. Tenerle visibili come filo conduttore.

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

<p class="ref">Quattro contesti: <b>MasterData</b> · <b>Sales</b> · <b>Warehouse</b> · <b>Knowledge</b></p>

Note: Sottolineare: i confini esistono già prima dell'AI. Il sistema non è greenfield pensato per gli agenti: è un ERP con le sue regole. Questo è il punto: l'AI deve rispettare ciò che esiste.

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

<p class="q">Nessun contesto "sa tutto". È il vincolo, non il problema.</p>

Note: Questo è l'assunto chiave del talk. La distribuzione dell'intelligenza non è un ripiego: è la conseguenza naturale dei confini di dominio.

---

<!-- .slide: class="lead" -->

## Creare un agente **è facile**.

## Progettarlo **no**.

Le demo mostrano agenti che funzionano.
La differenza non è nella demo: è in **dove vive la conoscenza**, **chi decide** e **chi coordina**.

Note: Slide di rottura. Serve a spostare l'attenzione dalla spettacolarità della demo alla qualità della progettazione.

---

<!-- .slide: class="lead" -->

## La mappa del talk

<p class="map">MCP → confini → conoscenza → agenti → coordinazione → osservabilità</p>

<p class="ref">Seguiremo l'ordine reale in cui le cose sono state costruite nel repository.</p>

Note: Mostrare che la sequenza è quella della storia di sviluppo, non quella di una presentazione a posteriori. Rende onesto il racconto e utile il paragone con i commit.

---

## Esperimento 1 — dare all'AI accesso all'ERP

Un solo **MCP Server** che espone un catalogo di tool: le capacità dell'ERP rese scopribili.

<div class="callout">
<b>MCP (Model Context Protocol)</b> è la superficie scopribile: l'agente vede <em>cosa può fare</em>, senza conoscere come è fatto l'ERP dentro.
</div>

<ul>
<li class="fragment">Un server, tanti tool, tante responsabilità</li>
<li class="fragment">Si parte dal caso più semplice: rispondere a domande sui dati</li>
</ul>

<p class="ref">commit <code>2877dfa</code> — "Add Chat and MCP" · <code>.../AI/BrewUp.AI.McpServer/Tools/BrewUpMcpTools.cs:9</code></p>

Note: DEMO checkout 2877dfa. Mostrare il McpServer unico e il tool catalog. È il punto di partenza: semplice e funzionante.

---

## Problema: il Tool Catalog diventa un **monolite**

<ul>
<li class="fragment">30+ tool, 4 domini, una sola superficie</li>
<li class="fragment">Il confine del dominio si perde nel catalogo</li>
<li class="fragment">Ownership e manutenzione non sono più chiari</li>
<li class="fragment">Un cambiamento in Sales tocca il server che espone tutto</li>
</ul>

<p class="q">Abbiamo ricreato in AI il monolite che il DDD ci aveva fatto evitare.</p>

Note: Il problema non è tecnico ma di confini. Far vedere il catalogo cresciuto: è la stessa lezione del monolite, applicata all'AI.

**Intervento Nando (1 minuto, 9:00)**

Domanda chiusa al pubblico: «Chi di voi ha un agente in produzione con più di venti tool?» Far alzare le mani, poi: «Che cosa lo chiamate, quel pezzo di codice?». Due-tre risposte e poi passi ad Alberto.

Due regole per non fallire: fai la domanda, aspetta tre secondi in silenzio, poi chiama. Chi risponde per primo parla di nuovo per tutta la sessione.

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

<p class="ref">commit <code>39ada0d</code> — monolite rimosso, un MCP per contesto · <code>.../MasterData/BrewUp.MasterData.McpServer/Tools/MasterDataTools.cs:7</code></p>

Note: DEMO checkout 39ada0d. Mostrare i tre McpServer separati. Messaggio: l'AI eredita gli stessi confini del dominio. Nessuna condivisione di DB o servizi, solo messaggi espliciti.

**Intervento Nando (30 secondi, 12:30)**

La domanda che porta la tesi: «Il DDD l'avevate già prima degli agenti. Sono stati gli agenti a farvi scoprire che il confine serviva, o il DDD che ha reso possibile l'AI?»

Se risponde «il DDD»: «E allora perché il primo tentativo è stato un monolite?»

Chiudi facendo dire ad Alberto la frase, non mostrandola: «Bounded contexts don't just separate code, they separate knowledge».

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

Note: Sottolineare l'ownership: ogni tool ha un proprietario chiaro. Questo abilita anche la scoperta selettiva: un agente vede solo i contesti che gli servono.

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

<p class="ref">commit <code>a889eb6</code> — "Introduce agent-based coordination for what-if analysis" · <code>.../Mother.Facade/Agents/MotherCoordinator.cs:63</code></p>

Note: DEMO checkout a889eb6. Presentare il what-if come collaborazione tra contesti. La coordinazione è esplicita e tracciabile, non emergente.

**Intervento Nando (30 secondi, 29:30)**

«Mother ha un prompt. Chi lo scrive? Se lo scrive un umano, Mother non è un agente: è un product manager con un LLM.»

Se Alberto risponde bene, chiedi il seguito: «E quando quel prompt sbaglia, di chi è il bug?».

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

<p class="q">Nessun agente risponde da solo: il risultato è una composizione.</p>

Note: Questo è il cuore dimostrativo. Mostrare che ogni contesto contribuisce con la propria autorità e che la risposta finale nasce dalla somma dei contributi, non da un unico modello onnisciente.

**Intervento Nando (1 minuto, 30:30)**

Chiedi al pubblico: «Secondo voi, quale contesto dovrebbe rispondere a "what if we sell 500 bottles of IPA"?». Fai alzare le mani e senti due risposte diverse, tipicamente Sales e Warehouse.

Poi ad Alberto: «Perché nessuno dei due può rispondere da solo?». È la risposta che apre Mother.

---

## Il dominio non vive tutto nel **database**

Contratti, procedure, PDF, wiki operative: conoscenza che non sta in una tabella.

<div class="cards">
<div class="card"><h4>Knowledge Context</h4><p>Un bounded context dedicato alla conoscenza</p></div>
<div class="card"><h4>Ingestion</h4><p>PDF e Markdown, estrazione, chunking</p></div>
<div class="card"><h4>Embedding</h4><p>Azure OpenAI per i vettori</p></div>
<div class="card"><h4>Retrieval</h4><p>Vector store: InMemory, SQL Server, Azure AI Search</p></div>
</div>

<p class="ref">commit <code>324be2f</code> — "RAG completed" · tool <code>search_knowledge_base</code> · <code>.../Knowledge.McpServer/Tools/KnowledgeTools.cs:10</code></p>

Note: DEMO checkout mother-with-mcp-and-rag. Qui nasce il Knowledge Context. RAG con confini di dominio, non un indice unico su tutto.

**Intervento Nando (30 secondi, 14:30)**

Leggi al pubblico la domanda: «Qual è la nostra politica di riordino per una IPA?». Lasciala in aria.

Poi ad Alberto: «Cosa succede quando la risposta non esiste in nessun documento?». Sposta l'attenzione dal "come funziona" al "quando non funziona".

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

Note: Spiegare il funzionamento senza scendere nei dettagli implementativi. Enfatizzare che il retrieval è similarity, non verità.

---

## Retrieval **non** significa conoscenza

Un chunk recuperato non è una risposta **giustificata**.

<ul>
<li class="fragment">Il retrieval risponde a "cosa è simile"</li>
<li class="fragment">Non risponde a "qual è la fonte"</li>
<li class="fragment">E non dice se il risultato è ancora valido</li>
</ul>

<p class="q">Serve conoscenza derivata, con provenance.</p>

Note: Ponte concettuale verso l'LLM Wiki. Far atterrare il problema prima della soluzione.

---

## Un tool non è un agente

<div class="cards">
<div class="card"><h4>MCP</h4><p>"What can you do?" — espone capacità, non si assume obiettivi</p></div>
<div class="card"><h4>A2A</h4><p>"Can you take responsibility?" — assume un obiettivo e lo porta a termine</p></div>
</div>

Il **Knowledge Agent**: dal tool al collaboratore.

<p class="ref">commit <code>a2a-no-framework</code> — A2A senza framework · <code>.../BrewUp.Shared/Agents/AgentCard.cs:3</code> · <code>.../Knowledge.Facade/Agents/KnowledgeAgentCardProvider.cs:7</code></p>

Note: DEMO checkout a2a-no-framework. Sottolineare: qui A2A è implementato a mano per capire il protocollo. Il framework arriverà dopo, quando il concetto è chiaro.

**Intervento Nando (30 secondi, 25:30)**

«Avete fatto A2A a mano quando il framework Microsoft esisteva già. Perché?»

La risposta vera è nello storico del repository: prima il protocollo, poi lo strumento. Falla dire ad Alberto, non dirla tu.

Poi: «Che cosa ha fatto il framework che il vostro codice non faceva?». Serve a mostrare che la scelta era deliberata.

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

Note: Chiarire i due protocolli complementari: MCP è verticale (agente→capacità), A2A è orizzontale (agente→agente). Questo è il passaggio da tool a collaboratore.

---

<!-- .slide: class="lead" -->

## Il sistema cresce

Orchestrazione e trasporto: **Aspire** + A2A su HTTP + Service Bus.

<div class="callout">
Il protocollo stesso si evolve: l'aggiornamento a <b>MCP 2.0</b> arriva dopo, insieme alla parte finale del sistema. Nell'ordine narrativo resta un <b>aggiornamento</b>, non un nuovo passo.
</div>

<p class="ref"><code>a2a-no-framework</code> → <code>feature/a2a</code> → MCP 2.0 (<code>1694b58</code>, <code>feature/mcp-2</code>) · <code>.../Mother.Facade/Mcp/McpToolsProvider.cs:52</code> · <code>.../BrewUp.Shared/Agents/McpToolClient.cs:25</code></p>

Note: Slide breve e opzionale. Serve a mostrare che il sistema diventa distribuito davvero: processi separati, comunicazione esplicita, orchestrazione dei servizi. Se qualcuno chiede della versione di MCP: l'introduzione è di maggio (2877dfa), l'update a 2.0 è di agosto (1694b58), quasi alla fine.

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

<p class="ref">commit <code>88ba7ae</code> — telemetria semantica nel sistema finale · <code>.../Mother.Facade/Telemetry/MotherTelemetry.cs:28</code></p>

Note: DEMO checkout main. Mostrare il trace: non solo i tempi, ma chi ha parlato con chi, quale capability è stata usata, quale handoff è avvenuto, con quale esito.

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

Note: Qui si chiude il cerchio con la tesi: se l'intelligenza fluisce tra confini, dobbiamo poter vedere il flusso. La telemetria semantica è ciò che rende il sistema progettabile e migliorabile.

**Intervento Nando (20 secondi, 35:00)**

Per chi non ha mai aperto un trace agentico: «Qui non vedete solo servizi che si chiamano. Vedete chi ha parlato con chi, quale capacità ha usato e con quale esito.».

---

## LLM Wiki: conoscenza derivata con **provenance**

Documenti → Wiki generata → risposta con **riferimenti alla fonte**.

<div class="flow">
<span class="node know">search_knowledge_base</span>
<span class="node know">query_wiki</span>
<span class="node know">get_wiki_page</span>
<span class="node know">get_wiki_page_evidence</span>
</div>

<p class="ref">commit <code>3e3e2f9</code> — il finale · KnowledgeTools · <code>.../Knowledge.McpServer/Tools/KnowledgeTools.cs:35,36</code></p>

Note: DEMO checkout feature/mcp-2. Mostrare i quattro tool del Knowledge. Differenza chiave: ogni pagina wiki porta con sé l'evidenza delle fonti da cui è derivata.

---

## Perché una *Wiki* e non solo RAG

<ul>
<li class="fragment">La conoscenza è <b>derivata</b>, non solo recuperata</li>
<li class="fragment">Ogni pagina mantiene i <b>riferimenti</b> alle fonti (provenance)</li>
<li class="fragment">Si aggiorna in modo controllato, non a ogni query</li>
<li class="fragment">L'agente può <b>citare</b> la fonte, non solo rispondere</li>
</ul>

<p class="q">Una risposta senza fonte è un'opinione.</p>

Note: Collegare al pattern LLM Wiki (Karpathy): costruire conoscenza strutturata e verificabile dai documenti, invece di affidarsi solo alla similarity. È il salto di qualità rispetto al RAG puro.

**Intervento Nando (30 secondi, 23:00)** — la domanda più ficcante del talk

«La wiki la genera l'LLM. Chi approva una pagina? Se non c'è un umano che firma, come si distingue una pagina affidabile da una pagina inventata?»

Proseguimento: «Derived knowledge is not operational truth. E se una pagina wiki contraddice lo stock?»

Chiudi con la domanda al pubblico: «Chi vorrebbe trovarsi un sistema che risponde con una procedura che nessuno approva?».

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

<p class="ref"><code>.../Knowledge.Facade/Evaluation/KnowledgeRetrievalEvaluator.cs:10</code> · <code>.../Mother.Facade/Agents/WhatIfWorkflowEvaluator.cs:45</code></p>

Note: Portare il discorso su come sappiamo che il sistema è migliorato. L'evaluation chiude il ciclo con l'osservabilità.

**Intervento Nando (30 secondi, 37:30)**

«L'evaluation la scrivete voi o la genera il modello? Se la genera il modello, non è un bias che il modello valuta sé stesso?»

Chiudi con: «Osservability dice cosa ha fatto il sistema. Evaluation dice se bastava. Chi decide se bastava?».

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

Note: Slide di sintesi dell'architettura. Qui si vede la tesi realizzata: l'intelligenza fluisce tra confini espliciti, non dentro un unico agente.

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

Note: DEMO finale. Lanciare il what-if, poi aprire il trace. Chiudere il cerchio con la tesi: ogni passo ha un confine, un'autorità e una traccia.

**Intervento Nando (10 minuti, la parte tua)**

Leggi tu la domanda: «What if we sell 500 bottles of IPA?». Alberto esegue, tu non tocchi il mouse.

Mentre gira, commenta ad alta voce cosa dovrebbe succedere: «adesso MasterData deve risolvere "IPA"... adesso Warehouse deve dirci se basta lo stock...». Serve a tenere il pubblico concentrato anche se qualcosa va storto.

Se qualcosa si rompe, non nasconderlo: «vediamo come risponde il sistema» diventa un momento onesto, non un errore.

Alla fine apri tu il trace: «Quale delle quattro risposte vi ha sorpreso?».

---

<!-- .slide: class="lead" -->

# Grazie

<p class="ref">
alberto.acerbis@intre.it · ferdinando.santacroce@gmail.com<br>
<code>https://github.com/BrewUp/BrewUpErp</code>
</p>

Note: Domande. Ringraziare. Indicare il repo per i dettagli.

**Domande di riserva**, se avanza tempo o c'è un vuoto:

- «Qual è la parte che oggi definite sperimentale e che in un'azienda vera non passerebbe la review?»
- «Se ricominciassi oggi, cosa rifaresti diversamente?»
- «Quanto vi è costato rifare l'AI layer quando il protocollo è passato a MCP 2.0?»
- «Che cosa avete costruito che non userete mai in produzione?»
- «Chi di voi ha un sistema agentico in produzione? Che cosa vi ha costato di più, costruirlo o convincerlo a restare?».

**Intervento Nando — chiusura (49:00-50:00)**

Torni all'immagine iniziale: `User → LLM → Tools`, e la confronti con lo schema finale. Poi la tesi, detta a memoria:

"The real challenge isn't connecting an LLM to some APIs. It's designing how intelligence flows through the organization."

Chiudi con una sola frase e tacere. Il silenzio finale vale più di un riassunto.
