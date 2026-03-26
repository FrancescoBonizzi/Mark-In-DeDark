# CLAUDE.md — Mark In DeDark

Letto all'inizio di ogni sessione prima di scrivere codice. Tu sei l'ingegnere software incaricato della migrazione da C# a TypeScript/PixiJS.

---

## Stato del progetto & Piano di Attacco

Tasks 0-10 completati (Flusso completo: menu → livello → gameover. Audio ok. Build passa).
Siamo nella fase **Bug-fix + Feature**.

> **Regola operativa:** Affronta i task rigorosamente nell'ordine delle Fasi sottostanti per minimizzare il context-switching. Aggiorna lo status (✅/🔄/❌) a fine sessione.

### Fase 1: UI, Routing e Layout

| Task    | Descrizione                                                                            | Status     |
| ------- | -------------------------------------------------------------------------------------- | ---------- |
| Task 1  | Bug: alla morte del player non si naviga alla schermata gameover.                      | ✅ Done    |
| Task 2  | UI: level select — griglia responsive (gli 8 livelli non devono uscire dallo schermo). | ✅ Done    |
| Task 3b | I livelli sono già tutti sbloccati, non ha senso il pulsante per sbloccarli            | ✅ Done    |

### Fase 2: Interazioni, HUD e Scena Static

| Task   | Descrizione                                                                                                        | Status     |
| ------ | ------------------------------------------------------------------------------------------------------------------ | ---------- |
| Task 3 | Bug: porte interattive non si aprono con Space. **Leggere `migration-specs/INTERACTION_AND_OBJECTS.md`**.          | ✅ Done    |
| Task 4 | HUD: popup testuali contestuali. **Leggere `migration-specs/INTERACTION_AND_OBJECTS.md`**.                         | ✅ Done    |
| Task 5 | Oggetti mancanti: braceri (BrazierLit) non renderizzati. **Leggere `migration-specs/INTERACTION_AND_OBJECTS.md`**. | ✅ Done    |

### Fase 3: Entità e Animazioni

| Task   | Descrizione                                                                                                     | Status     |
| ------ | --------------------------------------------------------------------------------------------------------------- | ---------- |
| Task 6  | Bug: animazione morte mancante. **Leggere `migration-specs/ENTITY_ANIMATION_LOGIC.md`** — risolve anche Task 1.       | ⬜ Pending |
| Task 7  | Bug: i nemici non attaccano visivamente. **Leggere `migration-specs/ENTITY_ANIMATION_LOGIC.md`**.                     | ⬜ Pending |
| Task 7b | Bug: i nemici non inseguono/spengono le candele del player. Verificare `KillCandleAI.ts` e il wiring in gamebootstrap. | ⬜ Pending |

### Fase 4: Rendering Avanzato

| Task   | Descrizione                                                                                                                       | Status     |
| ------ | --------------------------------------------------------------------------------------------------------------------------------- | ---------- |
| Task 8 | Normal map rotta: luci fioche, nessun effetto 3D. **Leggere `migration-specs/RENDERING_PIPELINE.md`** prima di toccare il codice. | ⬜ Pending |

---

## Protocollo di Risoluzione Task (Leggere prima di agire)

Per task complessi (specialmente **Task 11** e logiche core), applica questo workflow rigoroso:

1. **Analisi:** Leggi i file C# originali responsabili del comportamento atteso (in `progetto-riferimento/`).
2. **Ispezione:** Leggi l'attuale implementazione TypeScript/Shader.
3. **Documentazione (NO CODICE):** Aggiorna prima i file `migration-specs/PATTERN_COOKBOOK.md` o `ARCHITECTURE_MAP.md` spiegando il problema (es. differenze matematiche, coordinate invertite).
4. **Approvazione:** Attendi la conferma dell'utente prima di scrivere il codice effettivo.
5. **Implementazione:** Inserisci sempre un breve commento in cima al file modificato spiegando _cosa_ e _perché_ stai cambiando rispetto al C#.

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

- **Sorgente C# originale:** `progetto-riferimento/`
- **Migration Specs:** `migration-specs/ARCHITECTURE_MAP.md` (enum tables, oggetti non ancora portati), `PATTERN_COOKBOOK.md`.
- **Assets pubblici:** `Web/public/assets/` (spritesheets, sounds, levels)
- **Livelli:** `.mark` (testo pipe-delimited, parsati da `grid/GridLoader.ts`)
