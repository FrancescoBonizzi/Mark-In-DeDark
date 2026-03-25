# CLAUDE.md — Mark In DeDark

Letto all'inizio di ogni sessione prima di scrivere codice.

---

## Stato del progetto

Tasks 0-10 completati. Il gioco gira. Ora siamo in fase bug-fix + feature.

> **Aggiorna questa tabella a fine sessione.**

| Task    | Descrizione                                                                                                            | Status     |
| ------- | ---------------------------------------------------------------------------------------------------------------------- | ---------- |
| Task 12 | Bug: porte interattive non si aprono con Space. Premendo Space davanti a una porta apribile deve aprirsi.              | ⬜ Pending |
| Task 13 | Bug: animazione morte mancante. Effetto "esplosione parti del corpo" + schermo che diventa nero tranne il player.      | ⬜ Pending |
| Task 14 | Bug: i nemici non attaccano visivamente il player (animazione attacco mancante).                                       | ⬜ Pending |
| Task 15 | Bug: alla morte del player non si naviga alla schermata gameover.                                                      | ⬜ Pending |
| Task 16 | HUD: popup testuali contestuali (tutorial/aiuto: "premi Space", "chiave raccolta", "candele insufficienti", etc.).     | ⬜ Pending |
| Task 17 | Oggetti mancanti: braceri (BrazierLit) non renderizzati nelle mappe.                                                   | ⬜ Pending |
| Task 18 | UI: level select — griglia responsive (gli 8 livelli non devono uscire dallo schermo).                                 | ⬜ Pending |
| Task 11 | Normal map rotta: luci fioche, nessun effetto 3D. Prima analisi MD del rendering C# originale, poi bugfix sistematico. | ⬜ Pending |

**Legend:** ⬜ Pending · 🔄 In Progress · ✅ Done · ❌ Blocked

### Note ultima sessione (2026-03-25)

Task 9 e 10 completati. Flusso completo: menu → livello → gameover. Audio funziona (Howler, graceful failure su file mancanti). Camera shake (sinusoidale con decay). Pause/resume su visibilitychange. Death overlay con CSS fade-in. 4 debug console.log rimossi. `npm run build` passa.

---

## Tech Stack

| Tool       | Version    | Note                                    |
| ---------- | ---------- | --------------------------------------- |
| Vite       | 7.x        | `base` = `/mark-in-dedark/`             |
| TypeScript | 5.7 strict | `exactOptionalPropertyTypes: true`      |
| PixiJS     | 8.6.x      | WebGPU primario, WebGL fallback         |
| Howler     | 2.2.x      | Audio; singleton `SoundManagerInstance` |
| Navigo     | 8.x        | Hash router (`{ hash: true }`)          |

---

## Regole invarianti

- Ogni shader deve avere sia WGSL (WebGPU) che GLSL (WebGL). Mai GLSL-only.
- Nessun tipo `any` TypeScript.
- `destroyGame()` prima di ogni `initGame()`.
- I campi opzionali nelle classi vanno dichiarati come `T | undefined` (non `?: T`) a causa di `exactOptionalPropertyTypes`.
- Navigazione tra pagine: `window.location.hash = '#/route'` (evita import cicolari con router.ts).
- `npm run build` deve passare senza errori dopo ogni task.

---

## Riferimenti utili

- Sorgente C# originale: `progetto-riferimento/` (per capire il comportamento atteso)
- **`migration-specs/ARCHITECTURE_MAP.md`** — enum tables (`ObjectTypes`, `GroundTypes`, `WallTypes` con valori interi), lista classi C# ancora da portare (BrazierLit, particle systems, etc.). Leggi quando lavori su oggetti o trappole mancanti.
- Assets pubblici: `Web/public/assets/` (spritesheets, sounds, levels)
- Livelli `.mark`: testo pipe-delimited, parsati da `grid/GridLoader.ts`
- Prima di modificare un file esistente, aggiungi un commento in cima spiegando brevemente cosa stai per cambiare e perché, basandoti sul sorgente C#.
- Per problemi visivi (come il Task 17 - braceri non renderizzati), controlla sempre l'ordine di z-index/rendering di PixiJS e la presenza corretta dell'asset nel GridLoader.ts prima di modificare le classi visive.

## Note per la risoluzione del task 11

Procediamo rigorosamente in questo ordine:

1. Leggi i file C# originali responsabili del rendering delle normal map e dell'illuminazione (cerca in progetto-riferimento/).
2. Leggi la nostra attuale implementazione TypeScript e gli shader attuali.
3. Non scrivere ancora codice. Aggiorna prima il file migration-specs/PATTERN_COOKBOOK.md aggiungendo una nuova sezione intitolata 'C# to PixiJS 8 Shader Translation (WebGPU/WebGL)'. Spiega in quella sezione le differenze matematiche o architetturali che stanno causando il bug (es. coordinate invertite, calcolo della luce nello spazio sbagliato, etc.).
4. Solo dopo che avrò approvato le tue modifiche al Cookbook, procederemo a scrivere il codice rispettando la regola invariante: 'Ogni shader deve avere sia WGSL che GLSL'."
