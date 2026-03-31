# CLAUDE.md — Mark In DeDark

Letto all'inizio di ogni sessione prima di scrivere codice. Tu sei l'ingegnere software incaricato della migrazione da C# a TypeScript/PixiJS.

---

## Stato del progetto & Piano di Attacco

Tasks 0-10 completati (Flusso completo: menu → livello → gameover. Audio ok. Build passa).
Siamo nella fase **Bug-fix + Feature**.

> **Regola operativa:** Affronta i task rigorosamente nell'ordine delle Fasi sottostanti per minimizzare il context-switching. Aggiorna lo status (✅/🔄/❌) a fine sessione.

### Fase 1: UI, Routing e Layout

| Task    | Descrizione                                                                            | Status  |
| ------- | -------------------------------------------------------------------------------------- | ------- |
| Task 1  | Bug: alla morte del player non si naviga alla schermata gameover.                      | ✅ Done |
| Task 2  | UI: level select — griglia responsive (gli 8 livelli non devono uscire dallo schermo). | ✅ Done |
| Task 3b | I livelli sono già tutti sbloccati, non ha senso il pulsante per sbloccarli            | ✅ Done |

### Fase 2: Interazioni, HUD e Scena Static

| Task   | Descrizione                                                                                                        | Status  |
| ------ | ------------------------------------------------------------------------------------------------------------------ | ------- |
| Task 3 | Bug: porte interattive non si aprono con Space. **Leggere `migration-specs/INTERACTION_AND_OBJECTS.md`**.          | ✅ Done |
| Task 4 | HUD: popup testuali contestuali. **Leggere `migration-specs/INTERACTION_AND_OBJECTS.md`**.                         | ✅ Done |
| Task 5 | Oggetti mancanti: braceri (BrazierLit) non renderizzati. **Leggere `migration-specs/INTERACTION_AND_OBJECTS.md`**. | ✅ Done |

### Fase 3: Entità e Animazioni

| Task    | Descrizione                                                                                                            | Status  |
| ------- | ---------------------------------------------------------------------------------------------------------------------- | ------- |
| Task 6  | Bug: animazione morte mancante. **Leggere `migration-specs/ENTITY_ANIMATION_LOGIC.md`** — risolve anche Task 1.        | ✅ Done |
| Task 7  | Bug: i nemici non attaccano visivamente. **Leggere `migration-specs/ENTITY_ANIMATION_LOGIC.md`**.                      | ✅ Done |
| Task 7b | Bug: i nemici non inseguono/spengono le candele del player. Verificare `KillCandleAI.ts` e il wiring in gamebootstrap. | ✅ Done |

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

---

## Orchestrazione del Workflow

### 1. Modalità Pianificazione (Predefinita)

- Entra in modalità pianificazione per QUALSIASI attività non banale (3+ passaggi o decisioni architetturali)
- Se qualcosa va storto, FERMATI e pianifica di nuovo immediatamente
- Usa la modalità pianificazione anche per i passaggi di verifica, non solo per la costruzione
- Scrivi specifiche dettagliate in anticipo per ridurre l'ambiguità

### 2. Strategia dei Subagenti

- Usa i subagenti liberamente per mantenere pulita la finestra di contesto principale
- Delega ricerca, esplorazione e analisi parallela ai subagenti
- Per problemi complessi, usa più risorse computazionali tramite subagenti
- Un task per subagente per un'esecuzione focalizzata

### 3. Ciclo di Auto-Miglioramento

- Dopo QUALSIASI correzione da parte dell'utente: aggiorna tasks/lessons.md con il pattern
- Scrivi regole per te stesso che prevengano lo stesso errore
- Itera senza pietà su queste lezioni finché il tasso di errore diminuisce
- Rivedi le lezioni all'inizio della sessione per il progetto rilevante

### 4. Verifica Prima di Considerare Completato

- Non segnare mai un task come completato senza dimostrare che funziona
- Confronta il comportamento tra la versione principale e le tue modifiche quando rilevante
- Chiediti: "Un ingegnere senior approverebbe questo?"
- Esegui test, controlla i log, dimostra la correttezza

### 5. Pretendi Eleganza (Bilanciata)

- Per modifiche non banali: fermati e chiedi "esiste un modo più elegante?"
- Se una soluzione sembra un hack: "Sapendo tutto ciò che so ora, implementa la soluzione elegante"
- Salta questo passaggio per fix semplici e ovvi - evita l'over-engineering
- Metti in discussione il tuo lavoro prima di presentarlo

### 6. Risoluzione Autonoma dei Bug

- Quando ricevi un bug report: risolvilo e basta. Non chiedere guida passo passo
- Analizza log, errori, test falliti - poi risolvili
- Nessun bisogno di far cambiare contesto all'utente
- Risolvi i test CI falliti senza che ti venga detto come

## Gestione dei Task

1. Pianifica Prima: Scrivi il piano in tasks/todo.md con elementi verificabili
2. Verifica il Piano: Fai un check prima di iniziare l'implementazione
3. Traccia i Progressi: Segna gli elementi come completati man mano
4. Spiega le Modifiche: Fornisci un riepilogo ad alto livello a ogni step
5. Documenta i Risultati: Aggiungi una sezione di revisione in tasks/todo.md
6. Registra le Lezioni: Aggiorna tasks/lessons.md dopo le correzioni

## Principi Fondamentali

- Prima la Semplicità: Rendi ogni modifica il più semplice possibile. Impatta il minimo codice.
- Zero Pigrizia: Trova le cause radice. Niente soluzioni temporanee. Standard da sviluppatore senior.
- Impatto Minimo: Modifica solo ciò che è necessario. Nessun effetto collaterale o nuovi bug.
