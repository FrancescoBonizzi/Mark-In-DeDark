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

### Task 1

L'area della luce del player è troppo ampia. Se il player è al centro di una griglia di 9 celle, deve illuminare solo le 9 celle (un 3x3) adesso sta illuminando un 5x5!

### Task 2

Problema di colori del PAVIMENTO e saturazione.

1. La texture dei pavimenti hanno saturazione solo nei punti in cui c'è l'origine della luce e poi decresce con l'allontanarsi della luce da quel punto, invece nei punti in cui il pavimento è illuminato anche di poco, la saturazione dovrebbe essere totalmente uniforme
2. I colori originali erano molto molto più vividi anche al centro della luce (diciamo dove è posizionato il player, attualmente, sebbene presenti i colori, anche lì sono poco saturi)

Ho il sospetto, ma magari mi sbaglio, che magari il rendering iniziale del pavimento sia corretto, ma che sopra alla texture originale del pavimento ci sia disegnata la texture della specular map, perché vedo degli strani artefatti che hanno poco senso e che nel gioco originale non c'erano.

- Ho fatto uno screenshot del vecchio gioco come appare appena fai partire il livello 4: `Csharp-version-screenshot-level-4.jpg`. Guarda così ti rendi conto. Noterai che i rossi e i verdi sono molto molto più accesi nel punto in cui c'è il player.
- Ho fatto uno screenshot anche del livello 7 come appare nel vecchio gioco: `Csharp-version-screenshot-level-7.jpg`. Qua volevo farti notare la differenza di texture del pavimento, qua è molto evidente che sopra alla texture ci sia renderizzato qualcos altro che gli va sopra quasi a mo' di decal. Infatti nella versione vecchia la texture usata in questo livello era molto liscia e marroncina, adesso in pixijs è totalmente grigia e piena di artefatti sopra.

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
- **progetto-riferimento**: `/progetto-riferimento` contiene un altro gioco molto più semplice di questo che ho convertito manualmente da C# a Typescript/PixiJs (trovi entrambe le versioni). Usalo come riferimento per le parti di conversione a PixiJs, ma considera che non c'è nulla, in quel progetto, di relativo a rendering avanzato come shader, normal map, ecc.

---

## Workflow

- Per ogni bug: analizza prima il C# originale in `/Csharp`. Usa `progetto-riferimento/` se proprio hai dei dubbi per passare da CSharp a TS per le parti a certe conversioni in PixiJs. Non modificare senza aver letto la parte `/Csharp` originale.
- Risolvi autonomamente: analizza log, esegui il test Playwright, itera senza chiedere guida passo passo.
- Non segnare un task come completato senza che `npm run build` passi e il test Playwright confermi il comportamento.
- Dopo correzioni dell'utente: aggiorna `tasks/lessons.md` con il pattern per prevenire la ricorrenza.
- Impatto minimo: modifica solo ciò che serve. Niente refactor non richiesti, niente astrazioni speculative.
