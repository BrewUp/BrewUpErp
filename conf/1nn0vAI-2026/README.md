# 1nn0vAI 2026 · Da MCP tools a Distributed Intelligence

Questa cartella raccoglie il materiale del talk che presentiamo a **1nn0vAI 2026** (Pordenone, Aula S3). Il talk è di **Alberto Acerbis**, ideatore e autore della soluzione BrewUp; con lui sale sul palco Ferdinando Santacroce, che lo affianca nella conduzione. Il repository di riferimento è `github.com/BrewUp/BrewUpErp`.

Sito della conferenza: https://www.1nn0vai.it/

In programma il titolo è "Dall'MCP ai Sistemi Multi-Agente: progettare organizzazioni digitali con l'AI". Il deck che proiettiamo si intitola invece "Da MCP tools a Distributed Intelligence": è lo stesso talk, con un taglio più da sala.

## Obiettivo

Il talk non spiega come si usa MCP o A2A. Racconta sei problemi che Alberto ha incontrato davvero costruendo il layer AI dell'ERP BrewUp, e la decisione presa per ognuno.

La tesi è una sola:

> Progettare un sistema AI-native non significa aggiungere agenti a un'applicazione. Significa progettare come l'intelligenza fluisce nell'organizzazione.

Da qui partono quattro domande che fanno da spina dorsale:

- Dove vive la conoscenza?
- Chi ha l'autorità di rispondere?
- Chi coordina quando servono più contesti?
- Come sappiamo che ha funzionato davvero?

Il fine è pratico: far dire al pubblico "quello che ci hanno raccontato esiste davvero nella codebase". Per questo ogni passo del racconto ha un commit reale e un checkout da fare dal vivo.

## La storia in sei problemi

1. **Stiamo costruendo un nuovo monolite.** Il primo MCP Server espone tutto: diventa un Tool Catalog unico e perde i confini del dominio. Decisione: un MCP Server per Bounded Context.
2. **Il dominio non vive tutto nel database.** Procedure, policy e manuali non stanno in una tabella. Nasce il Knowledge Context, con RAG dentro i confini del dominio.
3. **Retrieval non significa conoscenza.** Un chunk simile non è una risposta giustificata. Arriva la LLM Wiki, con la provenance delle fonti.
4. **Un tool non è un agente.** MCP espone capacità, A2A delega responsabilità. Nasce il Knowledge Agent.
5. **Chi coordina tutto questo?** Una domanda, il what-if, non appartiene a nessun contesto. Entra Mother, che coordina senza possedere verità.
6. **"Ha funzionato" non è abbastanza.** L'observability tecnica dice cosa è successo, quella semantica dice perché. L'evaluation dice se bastava.

La sequenza dei problemi è anche l'ordine reale in cui il codice è stato scritto. Non è un ordine ricostruito a posteriori.

## La scaletta

Il talk dura 50 minuti. La scaletta blocca i tempi e i contenuti per ogni blocco.

| Tempo | Blocco | Idea centrale |
| ---: | --- | --- |
| 0–4 | Creare un agente è facile. Progettarlo no. | L'anti-pattern del chatbot con troppi tool |
| 4–9 | Esperimento 1: diamo all'AI accesso all'ERP | MCP come superficie delle capability |
| 9–14 | Problema 1: stiamo costruendo un nuovo monolite | Un MCP Server per Bounded Context |
| 14–19 | Problema 2: il dominio non vive tutto nel database | Knowledge Context + RAG |
| 19–24 | Problema 3: retrieval non significa conoscenza | Dal RAG alla LLM Wiki |
| 24–29 | Problema 4: un tool non è un agente | Knowledge Agent, Agent Card, MCP vs A2A |
| 29–34 | Problema 5: chi coordina tutto questo? | Mother e la collaborazione tra contesti |
| 34–39 | Problema 6: "ha funzionato" non è abbastanza | Observability tecnica e semantica, evaluation |
| 39–49 | Show me the system | Codice finale, demo end-to-end e trace |
| 49–50 | Conclusione | Progettare il flusso dell'intelligenza |

Il copione completo, con i contenuti estesi di ogni blocco, sta in `Copione.md`.

Il talk alterna racconto e demo. La demo non è una sola: ogni problema ha il suo checkout. La sequenza di checkout e i tag da creare sono in `Copione.md`.

## I ruoli

**Alberto Acerbis** è l'ideatore e l'autore della soluzione. La conosce fin nei dettagli ed è il protagonista del talk: tiene la slide della tesi, spiega l'architettura, guida ed esegue la demo, risponde alle domande tecniche e chiude ogni risposta riagganciando la scaletta. A lui vanno i meriti della soluzione e della presentazione.

**Ferdinando (Nando) Santacroce** lo affianca nella conduzione. È una spalla: conosce la soluzione e i contenuti, ma non è il protagonista. Apre il talk e cura il rapporto col pubblico, interviene nei momenti segnati nella scaletta con qualche domanda, legge la domanda del what-if durante la demo finale e aiuta a chiudere. Il merito resta di Alberto.

Da concordare prima di salire: chi tiene il mouse e chi parla durante la demo, e il segnale per passarsi la parola. Il segnale è guardarsi, non toccare il microfono.

## Come sono nate le slide

1. Siamo partiti dall'abstract della conferenza e dalla scaletta dei 50 minuti, che fissa i sei problemi e i tempi.
2. Abbiamo verificato sul repository la sequenza reale dei commit, branch per branch. Il deck segue quell'ordine, non una versione edulcorata.
3. Abbiamo scritto il deck in Reveal.js: il contenuto vive in un solo file Markdown, l'HTML è solo il renderer, il tema è custom.
4. Abbiamo messo il copione nelle note del relatore. Ogni nota porta i tempi, gli interventi di Nando con il minuto preciso e le domande al pubblico.

Il deck è pensato per essere una cosa sola con la presentazione: le note sono parte del copione, non un extra.

## Il file delle slide

Tutto vive in `slides/`.

- `slides.md`: il contenuto. È l'unica fonte di verità. I separatori `---` aprono una slide orizzontale, `----` una verticale, `Note:` introduce le note del relatore.
- `index.html`: il renderer. Reveal.js 5.2.1 da CDN, carica `slides.md` con `data-markdown`.
- `theme.css`: il tema custom. Chiaro, accento blu `#2980B9`, sfondo con gli sponsor.
- `assets/bg-title.png`: sfondo delle slide di apertura e di sezione.
- `assets/bg-content.png`: sfondo delle slide di contenuto.

Per cambiare sfondo basta sostituire i due PNG. I dettagli stanno anche in `slides/README.md`.

## Avviare le slide in locale

`data-markdown` carica `slides.md` via `fetch`, quindi serve un server HTTP: aprire il file con `file://` non funziona.

```bash
cd conf/1nn0vAI-2026/slides
python3 -m http.server 8799
# apri http://localhost:8799/
```

In alternativa:

```bash
npx serve conf/1nn0vAI-2026/slides
```

Tasti utili: `S` apre la presenter view, `ESC` l'overview, `F` il fullscreen, `B` mette in pausa.

## Esportare in PDF

Apri con il parametro di stampa e poi usa "Stampa → Salva come PDF" dal browser:

```
http://localhost:8799/?print-pdf
```

Il file `1nn0vAI_MCP-A2A.pdf` in questa cartella è l'export del deck. Il sorgente `slides.md` è Markdown quasi standard, quindi si può riusare con Marp o Slidev cambiando solo il renderer.

## Materiale correlato

- `Copione.md`: il copione del talk, blocco per blocco, con demo, branch, commit e tag.
- `1nn0vAI_MCP-A2A.pdf`: l'export PDF delle slide.

## Stato delle decisioni

Quasi tutto è deciso; il dettaglio sta in `Copione.md`.

- L'ordine del racconto segue la timeline dei commit (il deck è già allineato); la scaletta va riallineata allo stesso ordine.
- `demo/02-mcp-per-bc` e `demo/03-orchestrator` restano separati.
- Aspire resta come slide breve opzionale, con il tag `demo/06-aspire`.

Resta da chiudere solo l'intro: `bcfc74c` (DDD puro) oppure il branch `DDD` (snapshot con gli MCP già presenti). I tag della demo sono proposti ma non ancora creati.
