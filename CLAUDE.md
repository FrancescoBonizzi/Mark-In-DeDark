# CLAUDE.md — Mark In DeDark (Versione Web)

Stiamo lavorando sul porting dalla versione C# alla versione web con Typescript/PixiJs.

- La versione legacy C# è in `/Csharp`
- La nuova versione è in `/Web`

---

## Stato del progetto & Piano di Attacco

Siamo in una fase di bugfixing, la maggior parte del progetto è migrata.

Regole di bugfixing:

- Il funzionamento preciso della vecchia pipeline di rendering è qui, leggila prima di fare qualunque cosa: `migration-specs/RENDERING_PIPELINE_CSHARP.md`.
- Il funzionamento preciso della nuova pipeline di rendering è qui, , leggila prima di fare qualunque cosa: `migration-specs/RENDERING_PIPELINE_PIXIJS.md`.
- Per iterare in autonomia senza disturbarmi puoi inserire da solo dei debug console, e lanciare il test automatico `npx playwright test tests/debug-game.spec.ts`, questo fa girare il gioco sul livello 3 per 10 secondi e scrive tutto l'output della console in `debug/game-debug-output.txt`, così puoi fare tutto da solo e iterare a volontà. (attualmente punta al livello 8)

---

## Regole Invarianti (Hard Rules)

- **Shader:** Ogni shader deve avere sia WGSL (WebGPU) che GLSL (WebGL). Mai GLSL-only.
- **TypeScript:** Nessun tipo `any`. I campi opzionali nelle classi vanno dichiarati come `T | undefined` (non `?: T`) per via di `exactOptionalPropertyTypes`.
- **Game Lifecycle:** Richiama `destroyGame()` prima di ogni `initGame()`.
- **Routing:** Navigazione tra pagine tramite `window.location.hash = '#/route'` (evita import circolari con `router.ts`).
- **Rendering Debug:** Per problemi visivi, controlla sempre l'ordine di z-index di PixiJS prima di alterare la logica delle classi visive.
- **Build:** `npm run build` deve passare senza errori dopo la risoluzione di ogni singolo task.

---

## Tech Stack & Riferimenti

| Tool       | Version    | Note                                    |
| ---------- | ---------- | --------------------------------------- |
| Vite       | 7.x        | `base` = `/mark-in-dedark/`             |
| TypeScript | 5.7 strict | `exactOptionalPropertyTypes: true`      |
| PixiJS     | 8.6.x      | WebGPU primario, WebGL fallback         |
| Howler     | 2.2.x      | Audio; singleton `SoundManagerInstance` |
| Navigo     | 8.x        | Hash router (`{ hash: true }`)          |

- **Sorgente C# originale:** `Csharp/`
- **Migration Specs:** `migration-specs/ARCHITECTURE_MAP.md` (enum tables, oggetti non ancora portati), `PATTERN_COOKBOOK.md`.
- **Assets pubblici:** `Web/public/assets/` (spritesheets, sounds, levels)
- **Livelli:** `.mark` (testo pipe-delimited, parsati da `grid/GridLoader.ts`)
- **Level Design:** `level-design/LEVEL_FORMAT.md` — documentazione completa del formato `.mark` per creare nuovi livelli; `level-design/ENEMY_AI.md` — copia dell'AI doc per il level design
- **progetto-riferimento**: `/progetto-riferimento` contiene un altro gioco molto più semplice di questo che ho convertito manualmente da C# a Typescript/PixiJs (trovi entrambe le versioni). Usalo come riferimento per le parti di conversione a PixiJs, ma considera che non c'è nulla, in quel progetto, di relativo a rendering avanzato come shader, normal map, ecc.

---

## Workflow

- Per ogni bug: analizza prima il C# originale in `/Csharp`. Usa `progetto-riferimento/` se proprio hai dei dubbi per passare da CSharp a TS per le parti a certe conversioni in PixiJs. Non modificare senza aver letto la parte `/Csharp` originale.
- Risolvi autonomamente: analizza log, esegui il test Playwright, itera senza chiedere guida passo passo.
- Non segnare un task come completato senza che `npm run build` passi e il test Playwright confermi il comportamento.
- Dopo correzioni dell'utente: aggiorna `tasks/lessons.md` con il pattern per prevenire la ricorrenza.
- Impatto minimo: modifica solo ciò che serve. Niente refactor non richiesti, niente astrazioni speculative.
