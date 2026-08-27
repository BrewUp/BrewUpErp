---
name: journal
description: >
  Salva sul journal quando ricevi comandi come "salva sul journal",
  "ricordati questo", "chiudi sessione".
license: MIT
metadata:
  author: Ferdinando Santacroce
  version: "1.1"
  target-audience: "Sviluppatori BrewUp — sessioni di lavoro su BrewUpErp"
---

# Journal Workflow

Genera **esclusivamente** il blocco Markdown per `JOURNAL.md` nella root del repository. Niente preamboli, niente spiegazioni.
**OBBLIGATORIO: NON MODIFICARE MAI LE ENTRY GIÀ PRESENTI!**

## 1. Prepara

- Se `JOURNAL.md` ha modifiche non committate, chiedi se committarle prima di procedere
- Se trovi entry fuori ordine cronologico, riordinale senza chiedere
- Non modificare mai il frontmatter YAML

## 2. Scrivi

Ogni entry deve **SEMPRE** riportare data e titolo nella prima riga, in questo formato obbligatorio:

```
## [YYYY-MM-DD] - Titolo sessione
```

- **Data**: formato ISO `YYYY-MM-DD`, senza ora
- **Titolo**: specifico e descrittivo, mai generico (es. "Test E2E dello stack: fix pipeline eventi e currency birre", non "Lavoro su BrewUp" o "Varie")

Prepend sempre la nuova entry in **cima** a `JOURNAL.md`, subito dopo il frontmatter YAML (ordine cronologico inverso).

Template:

```
## [YYYY-MM-DD] - [Titolo sessione]

**Attività:**
- [cosa è stato fatto]

**Decisioni:**
- [decisioni con motivazione e impatto]

**Appreso:**
- [opzionale]

**Da ricordare:**
- [opzionale]

---
```

Regole:

- Ogni entry inizia SEMPRE con `## [YYYY-MM-DD] - Titolo`: senza data e titolo la entry non è valida
- `---` finale obbligatorio dopo ogni entry
- Le sezioni opzionali (`Appreso`, `Da ricordare`) si omettono se vuote
- Le sezioni obbligatorie sono `Attività` e `Decisioni`
- Non modificare il contenuto delle entry precedenti: prepend solo

## 3. Proponi commit

- Chiedi se l'entry nel JOURNAL è OK
- Proponi di committarla ma attendi esplicito consenso

## Esempio

```
## [2026-08-27] - Test E2E dello stack: fix pipeline eventi e currency birre

**Attività:**
- Verificato che tutto lo stack fosse già attivo (docker backing services, 5 MCP/agent, API :6094, React :5173)
- Ripristinata la pipeline eventi: passaggio dal message bus Azure Service Bus al RabbitMQ locale
- Azzerata la LastEventPosition in MongoDB per il replay del read model

**Decisioni:**
- Passaggio a RabbitMQ locale (`UseRMQ: true`) perché Azure Service Bus falliva con `TryAgain (ServiceCommunicationProblem)`

**Appreso:**
- In Muflone il read model è alimentato da EventDispatcher, che si sottoscrive a KurrentDB `$all` dalla posizione persistita su MongoDB

**Da ricordare:**
- API riavviata con nohup, log su /tmp/opencode/brewup-rest.log

---
```
