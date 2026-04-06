# CLAUDE.md — Mark In DeDark (Versione Web)

Stiamo lavorando sul porting dalla versione C# alla versione web con Typescript/PixiJs.

- La versione legacy C# è in `/Csharp`
- La nuova versione è in `/Web`

---

## Regole per task di bugfixing

- Il funzionamento preciso della vecchia pipeline di rendering è qui, leggila prima di fare qualunque cosa: `docs/rendering/RENDERING_PIPELINE_CSHARP.md`.
- Il funzionamento preciso della nuova pipeline di rendering è qui, leggila prima di fare qualunque cosa: `docs/rendering/RENDERING_PIPELINE_PIXIJS.md`.
- Per iterare in autonomia senza disturbarmi puoi inserire da solo dei debug console, e lanciare il test automatico `npx playwright test tests/debug-game.spec.ts`, questo fa girare il gioco sul livello 3 per 10 secondi e scrive tutto l'output della console in `debug/game-debug-output.txt`, così puoi fare tutto da solo e iterare a volontà. (attualmente punta al livello 8)

---

## Regole Invarianti (Hard Rules) — solo per task di codice

- **Shader:** Ogni shader deve avere sia WGSL (WebGPU) che GLSL (WebGL). Mai GLSL-only.
- **TypeScript:** Nessun tipo `any`. I campi opzionali nelle classi vanno dichiarati come `T | undefined` (non `?: T`) per via di `exactOptionalPropertyTypes`.
- **Game Lifecycle:** Richiama `destroyGame()` prima di ogni `initGame()`.
- **Routing:** Navigazione tra pagine tramite `window.location.hash = '#/route'` (evita import circolari con `router.ts`).
- **Rendering Debug:** Per problemi visivi, controlla sempre l'ordine di z-index di PixiJS prima di alterare la logica delle classi visive.
- **Build:** `npm run build` deve passare senza errori dopo la risoluzione di ogni singolo task di codice.

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
- **Documentazione:** `docs/` — pipeline di rendering, AI nemici, formato livelli, linee guida di design
  - `docs/rendering/RENDERING_PIPELINE_CSHARP.md` — pipeline originale C# (reference per debugging comparativo)
  - `docs/rendering/RENDERING_PIPELINE_PIXIJS.md` — pipeline corrente PixiJS (fondamentale)
  - `docs/ENEMY_AI.md` — comportamento AI di tutti i nemici
  - `docs/LEVEL_FORMAT.md` — struttura tecnica del file `.mark`
  - `docs/LEVEL_DESIGN_GUIDELINES.md` — linee guida di design, narrativa, progressione difficoltà
- **Assets pubblici:** `Web/public/assets/` (spritesheets, sounds, levels)
- **Livelli:** `.mark` (testo pipe-delimited, parsati da `grid/GridLoader.ts`). Validatore: `Web/scripts/validate-level.ts`
- **progetto-riferimento**: `/progetto-riferimento` contiene un altro gioco molto più semplice di questo che ho convertito manualmente da C# a Typescript/PixiJs (trovi entrambe le versioni). Usalo come riferimento per le parti di conversione a PixiJs, ma considera che non c'è nulla, in quel progetto, di relativo a rendering avanzato come shader, normal map, ecc.
- **Level Editor Migration**: `LEVEL_EDITOR_MIGRATION.md` — analisi completa della conversione del Level Editor WPF → TypeScript/Vite (inventario C#, funzionalità, complessità per area, risorse riusabili)
- **Level Editor Web**: `Web/src/pages/leveleditor/` — editor visuale integrato per creare e modificare i file `.mark` direttamente nel browser

---

## Workflow — task di codice (bugfix, feature, refactor)

- Per ogni bug: analizza prima il C# originale in `/Csharp`. Usa `progetto-riferimento/` se proprio hai dei dubbi per passare da CSharp a TS per le parti a certe conversioni in PixiJs. Non modificare senza aver letto la parte `/Csharp` originale.
- Risolvi autonomamente: analizza log, esegui il test Playwright, itera senza chiedere guida passo passo.
- Non segnare un task come completato senza che `npm run build` passi e il test Playwright confermi il comportamento.
- Dopo correzioni dell'utente: aggiorna `tasks/lessons.md` con il pattern per prevenire la ricorrenza.
- Impatto minimo: modifica solo ciò che serve. Niente refactor non richiesti, niente astrazioni speculative.

---

## Workflow — generazione livelli

Quando il task riguarda la creazione o modifica di livelli di gioco, **non eseguire build né test Playwright** — non hanno senso in questo contesto.

Il punto di partenza obbligatorio è **`docs/LEVEL_TASKS.md`**: contiene la lista dei livelli, lo stato di avanzamento, le specifiche di ogni livello, il workflow da seguire e la checklist di completamento.

Documentazione di supporto:

- `docs/LEVEL_FORMAT.md` — struttura tecnica del formato `.mark`
- `docs/LEVEL_DESIGN_GUIDELINES.md` — linee guida di design, narrativa, progressione difficoltà

Workflow per ogni livello:

1. Leggi le specifiche del livello in `LEVEL_TASKS.md`.
2. Leggi `LEVEL_FORMAT.md` e `LEVEL_DESIGN_GUIDELINES.md` se non li hai già in contesto. **Inizia sempre dai VINCOLI INVIOLABILI** in cima a `LEVEL_DESIGN_GUIDELINES.md`.
3. Produci il grafo delle stanze in testo (Spawn → A → B → … → Uscita) e verifica i vincoli (bounding box, trigger nei corridoi 1-cella, linearità). Segui il workflow a fasi nella sezione 6 di `LEVEL_DESIGN_GUIDELINES.md`.
4. Crea o modifica `Web/public/assets/levels/folder/N.mark` direttamente nel formato pipe-delimited secondo `LEVEL_FORMAT.md`.
5. Valida con `npx tsx Web/scripts/validate-level.ts Web/public/assets/levels/folder/N.mark`.
6. Se ci sono errori, correggi il `.mark` direttamente.
7. Segna il livello come ✅ in `LEVEL_TASKS.md`.
