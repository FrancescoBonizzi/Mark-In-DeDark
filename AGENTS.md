# AGENTS.md

Questo repository contiene due versioni dello stesso videgioco:

- La versione legacy C# è in `/Csharp`
- La nuova versione (web) è in `/Web`

Lavoriamo solo sulla versione web.

## Informazioni

- Il funzionamento preciso della pipeline di rendering è qui: `docs/rendering/RENDERING_PIPELINE_PIXIJS.md`.
- Per iterare in autonomia senza disturbarmi puoi inserire da solo dei debug console, e lanciare il test automatico `npx playwright test tests/debug-game.spec.ts`, questo fa girare il gioco sul livello 3 per 10 secondi e scrive tutto l'output della console in `debug/game-debug-output.txt`, così puoi fare tutto da solo e iterare a volontà. (attualmente punta al livello 8)
- Ogni build genera anche un manifest per i livelli per popolare le pagine HTML

## Tech Stack

- Vite
- Typescript
- PixiJs
- Howler
- Navigo

## Setup commands

- `npm run dev` per eseguire il gioco
- `npm run build` per creare un bundle
- Per iterare in autonomia senza disturbarmi puoi inserire da solo dei debug console, e lanciare il test automatico `npx playwright test tests/debug-game.spec.ts`, questo fa girare il gioco su un dato livello per 10 secondi e scrive tutto l'output della console in `debug/game-debug-output.txt`, così puoi fare tutto da solo e iterare a volontà.

## Struttura dei file

- **Assets pubblici:** `web/public/assets/` (spritesheets, sounds, levels)
- **Livelli:** `.mark` (testo pipe-delimited, parsati da `grid/GridLoader.ts`)
- **Level Editor Web**: `Web/src/pages/leveleditor/` — editor visuale integrato per creare e modificare i file `.mark` direttamente nel browser
