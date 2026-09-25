# Slide del talk — Da MCP tools a Distributed Intelligence

Deck in **Reveal.js**. Tutto il contenuto vive in `slides.md`: l'HTML è solo il renderer.

## File

- `slides.md` — contenuto delle slide (unica fonte di verità). Separatori: `---` = slide orizzontale, `----` = verticale, `Note:` = note del relatore.
- `index.html` — renderer Reveal.js 5.2.1 (CDN), carica `slides.md` via `data-markdown`.
- `theme.css` — tema custom (light, accento blu #2980B9, sfondo sponsor).
- `assets/bg-content.png` — sfondo slide di contenuto (bianco + barra sponsor).
- `assets/bg-title.png` — sfondo slide di apertura/sezione (blu Inn0vAI 2026 + sponsor).

Le slide di apertura e di sezione usano `data-background-image="assets/bg-title.png"`; tutte le altre
mostrano `assets/bg-content.png` come sfondo dell'area slide 16:9 (`.reveal .slides`), non della
finestra: così l'immagine non viene mai ritagliata e i loghi restano interi su qualunque
proporzione di schermo. Per cambiare sfondo basta sostituire i due PNG.

## Avvio in locale

`data-markdown` carica `slides.md` via `fetch`: serve un server HTTP (il `file://` è bloccato dai browser).

```bash
cd tmp/slides
python3 -m http.server 8799
# apri http://localhost:8799/
```

Oppure:

```bash
npx serve tmp/slides
```

Tasti utili: `S` presenter/speaker view, `ESC` overview, `F` fullscreen, `B` pausa.

## Export PDF

Apri con il parametro di stampa e poi "Stampa → Salva come PDF" dal browser:

```
http://localhost:8799/?print-pdf
```

## Export verso altri formati

Il sorgente `slides.md` è Markdown quasi-standard (CommonMark + separatori `---`), quindi si può
riusare con **Marp** o **Slidev** cambiando solo il renderer. Vedi `../Copione.md`.

## Riferimenti demo

Ogni slide operativa ha una nota con il commit/branch da fare checkout durante il talk.
Sequenza completa dei tag in `../Copione.md`.
