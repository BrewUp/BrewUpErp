---
name: journal
description: >
  Salva sul journal quando ricevi comandi come "salva sul journal",
  "ricordati questo", "chiudi sessione".
---

# Journal Workflow

## 1. Prepara

Se `JOURNAL.md` ha modifiche non committate, chiedi se committarle prima di procedere. 
Se trovi entry fuori ordine cronologico, riordinale senza chiedere.

## 2. Scrivi

Aggiungi la nuova entry in **cima** a `JOURNAL.md`, subito dopo
il frontmatter YAML (ordine cronologico inverso). Non modificare
il frontmatter.

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

## 3. Proponi commit
 - Chiedi se l'entry nel JOURNAL è OK
 - Proponi di committarla ma attendi esplicito consenso

