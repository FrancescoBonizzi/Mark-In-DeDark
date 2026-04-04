# Level Editor: Migrazione WPF → TypeScript/Vite

## Context

Il Level Editor attuale e' un'applicazione WPF desktop (~2.200 righe C# + ~2.300 righe XAML) in `Csharp/MarkInDeDark.LevelEditor/`. L'obiettivo e' convertirlo in una pagina web routabile (`#/level-editor`) all'interno di `/Web`, che lavori esclusivamente con file `.mark` (no formato ASCII).

---

## Inventario completo del C#

### File sorgente (20 file, ~2.233 righe C#)

| Componente C#                       | Righe            | Complessita' | Note conversione                                             |
| ----------------------------------- | ---------------- | ------------ | ------------------------------------------------------------ |
| **MainWindow.xaml/.cs**             | 585 + ~1640 XAML | ALTA         | Core: sidebar 4 tab, canvas, menu, shortcuts                 |
| **WpfCanvasEditorPresentation.cs**  | 482              | MEDIA-ALTA   | Rendering griglia 25x45: celle, muri, oggetti, hover         |
| **EditorTypesObjectsConverter.cs**  | 434              | BASSA        | Mapping tipo->icona/colore: puro dato, conversione meccanica |
| **EditorTypesColorConverter.cs**    | 101              | BASSA        | Mapping tipo->colore: puro dato                              |
| **EditorCellTextConverter.cs**      | 60               | BASSA        | Mapping tipo->label testo                                    |
| **TraslationsWindow.xaml/.cs**      | 86 + ~105 XAML   | MEDIA        | Editor traduzioni chiave/valore per lingua                   |
| **InGameParametersWindow.xaml/.cs** | 75 + ~194 XAML   | BASSA-MEDIA  | Slider parametri illuminazione                               |
| **GameConfigEditorWindow.xaml/.cs** | 75 + ~130 XAML   | BASSA-MEDIA  | PropertyGrid per GameConfig                                  |
| **InputDialog.xaml/.cs**            | 42 + ~76 XAML    | BASSA        | Modal con TextBox -> `prompt()` o modal HTML                 |
| **FileSettingsRepository.cs**       | 81               | ELIMINABILE  | Salvava path exe in AppData                                  |
| **Converters (XnaColors, Vector2)** | ~88 tot          | ELIMINABILE  | Bridge XNA<->WPF                                             |

### XAML files (6 file, ~2.320 righe totali)

- `MainWindow.xaml` (~1640 righe) - Layout principale
- `InGameParametersWindow.xaml` (~194 righe)
- `App.xaml` (~175 righe)
- `GameConfigEditorWindow.xaml` (~130 righe)
- `TraslationsWindow.xaml` (~105 righe)
- `InputDialog.xaml` (~76 righe)

### Dipendenze C#

- .NET 9.0 WPF (`UseWPF`)
- Extended.Wpf.Toolkit 4.0.1 (IntegerUpDown, ColorPicker, PropertyGrid)
- MonoGame.Framework.WindowsDX 3.8.1 (tipi Color, Vector2, Vector3)
- Newtonsoft.Json 13.0.3
- SharpDX (trascinato da MonoGame)
- Progetti interni: `MarkInDeDark.Common`, `MarkInDeDark.GameGridCommon`

---

## Funzionalita' dell'editor C# (completo)

### UI principale (MainWindow)

- **Layout**: due colonne — sidebar 200px (TabControl) + canvas editor
- **Menu bar**: File (Nuovo, Apri, Salva, Salva copia, Path exe) + Strumenti (Test in-game, Parametri illuminazione, Config, Traduzioni)
- **Barra proprieta'**: IntegerUpDown per dirty probability (0-100), ComboBox per texture pack (1-8), nome livello

### Sidebar — 4 tab con radio button (tutti mutuamente esclusivi)

1. **Terreno**: NonCella, Buio, Terra1-10 (sfumature rosso/marrone), TrappolaMorteIstantanea, TrappolaTemporizzata, TriggerGoblin, Riempimento1-13 (con preview immagine)
2. **Muri**: No muro, M0-M5 (verde), MB/Fragile (rosso), P/Porta (blu), I/Ingresso (aqua), U/Uscita (aqua), D1-D8/Decal (viola)
3. **Oggetti**: Cancella, Giocatore, Chiave, Uscita, Candele, Braciere, Testo, Colonna larga, Colonna stretta, Vaso
4. **Nemici**: Zombie, Scheletro, Goblin, Taurus, Ghost, Bubu

### Interazione mouse

- **Terreno**: click/drag dipinge il tipo selezionato. Se Riempimento, cancella anche l'oggetto sulla cella.
- **Muri**: hover evidenzia la cella (colore piu' chiaro). Frecce direzionali piazzano il muro sul lato corrispondente. Regole di validazione: Left/Right non ammettono Ingresso/Uscita; Bottom non ammette Ingresso/Uscita.
- **Oggetti**: click piazza l'oggetto. Singleton (Giocatore, Chiave, Uscita, Goblin) disabilitano il bottone dopo il piazzamento. Candele chiede quantita' via InputDialog. Nemici chiedono path identifier.
- **Testo**: click apre InputDialog per string code, mostra "A" in corsivo sulla cella, tooltip con testo.

### Keyboard shortcuts

| Tasto  | Azione                                                       |
| ------ | ------------------------------------------------------------ |
| F      | Seleziona Terra1                                             |
| W      | Seleziona Muro Occludente                                    |
| D      | Seleziona Niente (no muro)                                   |
| C      | Seleziona Cancella oggetto                                   |
| R      | Seleziona Riempimento1                                       |
| Frecce | Piazza muro sul lato corrispondente (solo in modalita' Muri) |

### File I/O (.mark)

- **Apertura**: `OpenFileDialog` -> deserializzazione asincrona con `GameGridDeserializerManager` (auto-detect V2/V3/V4/Excel)
- **Salvataggio**: serializzazione con `GameGridSerializerV4` -> file .mark
- **Dirty tracking**: confronto stringa serializzata vs ultimo salvataggio
- **Warning unsaved changes**: su New, Open, e chiusura finestra

### Finestre secondarie

1. **InputDialog**: modal con TextBox + Ok/Cancel. Usato per testo celle, quantita' candele, path nemici.
2. **TraslationsWindow**: TabControl per lingua, ListView con colonne Key/Value editabili. Save su JSON.
3. **InGameParametersWindow**: slider per parametri illuminazione (direzione luce, distanza, opacita', colori). Scrive JSON temp e lancia exe.
4. **GameConfigEditorWindow**: PropertyGrid (Xceed) per GameConfig con editor custom per Color e Vector2.

### Rendering canvas (WpfCanvasEditorPresentation)

- Griglia 25 righe x 45 colonne di `Border` WPF posizionati manualmente
- Dimensione cella: 30px
- Ogni cella: `Grid` con `Image` (icona oggetto) + `TextBlock` (quantita'/codici muro) + `Label` ("A" per testo)
- Z-ordering manuale con `ReorderControls()`
- Hover: traccia `_lastHoveredCell`, schiarisce/ripristina colore background
- Muri: `BorderThickness` 4px sul lato corrispondente, `TextBlock` etichetta posizionata al bordo
- Colori muri: verde (standard), rosso (fragile), blu (porta), aqua (ingresso/uscita), viola (decal)

### Funzionalita' assenti nel C#

- Nessun undo/redo
- Nessun copy/paste
- Nessun drag-and-drop
- Nessun resize griglia
- Nessun zoom/scroll canvas

---

## Risorse gia' esistenti nel Web

| Risorsa                      | File                                   | Cosa fornisce                                                       |
| ---------------------------- | -------------------------------------- | ------------------------------------------------------------------- |
| Enums completi               | `Web/src/grid/enums.ts`                | GroundTypes, ObjectTypes, WallTypes, WallPositions — identici al C# |
| Tipi dati griglia            | `Web/src/grid/GridTypes.ts`            | `CellData`, `GridBundle` — struttura completa cella                 |
| Parser .mark (lettura)       | `Web/src/grid/GridLoader.ts`           | `parseLevel()` — deserializzazione V4 funzionante                   |
| Serializer .mark (scrittura) | `Web/scripts/ascii-to-mark.ts:378-408` | `serializeMark()` — da estrarre in modulo condiviso                 |
| Pattern routing              | `Web/src/pages/router.ts`              | Navigo hash router con `leave` hook                                 |
| Pattern pagina DOM           | `Web/src/pages/menu.ts`                | Template per pagine DOM-only                                        |
| CSS                          | `Web/src/ui/styles.css`                | Stili riusabili                                                     |
| Formato livelli              | `docs/LEVEL_FORMAT.md`                 | Specifica completa .mark                                            |

---

## Stima di complessita' per area

### 1. Griglia interattiva — ALTA

La parte piu' complessa. 25x45 = 1.125 celle da renderizzare con colori, bordi muri, icone oggetti, etichette testo.

**Approccio raccomandato**: Canvas HTML5 puro (no PixiJS — l'editor non ha bisogno di shader/normal map). Performance ottima, rendering semplice.

~300-400 righe TS.

### 2. Sidebar tool palette — MEDIA

4 tab con radio button color-coded. HTML/CSS e' molto piu' snello di XAML.

~150-200 righe TS + CSS.

### 3. Mouse interaction e painting — MEDIA

Click/drag painting, hover highlight, frecce direzionali, conversione coordinate mouse -> griglia. Logica quasi 1:1 dal C#.

~150-200 righe.

### 4. Serializzazione/deserializzazione .mark — BASSA

Gia' implementato. Serve estrarre `serializeMark` dal CLI script in un modulo condiviso e adattare I/O per il browser.

~50 righe.

### 5. Gestione singleton — BASSA

Player, Chiave, Uscita, Goblin: tracking unicita' + enable/disable bottoni.

~50-80 righe.

### 6. Validazione muri — BASSA

Guard su combinazioni invalide (Left/Right/Bottom vs Ingresso/Uscita).

~20-30 righe.

### 7. Keyboard shortcuts — BASSA

Singolo `keydown` listener.

~30-40 righe.

### 8. Dialog modali — BASSA

`prompt()` nativo o modal HTML custom.

~0-60 righe.

### 9. File I/O browser — BASSA-MEDIA

- **Open**: `<input type="file" accept=".mark">` + `FileReader`
- **Save**: `Blob` + `URL.createObjectURL` + download link
- **Opzionale**: File System Access API per save-in-place (solo Chrome)
- Dirty tracking: confronto stringa serializzata

~60-80 righe.

### 10. Finestre secondarie (opzionali V1) — MEDIA

- TraslationsWindow: editor traduzioni. Utile ma non bloccante.
- InGameParametersWindow: va ripensato per web (nel C# lanciava exe).
- GameConfigEditorWindow: puo' diventare un JSON editor.

Se incluse: ~200-300 righe extra. Consiglio di ometterle nella V1.

### 11. Routing e integrazione — BASSA

Nuova route in `router.ts`, link nel menu.

~20-30 righe.

---

## Riepilogo quantitativo

| Scenario                                                               | Righe TS stimate |
| ---------------------------------------------------------------------- | ---------------- |
| **V1 Core** (griglia + palette + paint + muri + save/load + shortcuts) | ~800-1.100       |
| **V1 + dialog custom + dirty tracking**                                | ~1.000-1.300     |
| **V2 completo** (+ traduzioni + config editor + test in-game)          | ~1.300-1.600     |

Confronto: ~2.200 C# + ~2.300 XAML -> ~1.000-1.300 TS. Il web e' piu' conciso perche':

- Niente XAML boilerplate
- Niente bridge XNA<->WPF (converters eliminati)
- Niente settings filesystem desktop
- Parser/serializer .mark gia' esistenti

---

## Rischi e punti di attenzione

1. **Performance canvas 1.125 celle**: nessun rischio reale, Canvas 2D gestisce senza problemi.
2. **File I/O cross-browser**: File System Access API solo Chrome. Fallback download/upload per Firefox/Safari.
3. **Undo/redo**: il C# non ce l'ha. Potrebbe valere la pena aggiungerlo nel web (pattern Command, ~100-150 righe extra).
4. **Test in-game**: nel C# lanciava l'exe. Nel web si puo' fare un link `#/game/preview` che carica il livello da sessionStorage.

---

## Verdetto

**Complessita': MEDIA.** Ben definito e delimitato. Il grosso del lavoro e' la griglia interattiva (rendering + mouse + keyboard). Il resto e' mapping meccanico di dati e UI HTML/CSS. Molto codice C# (converters, XAML, bridge XNA) non ha equivalente nel web.

Enums, tipi, parser e serializer **gia' presenti** nella codebase riducono significativamente lo sforzo.

**Decisione tecnica**: CSS Grid con div (NON Canvas HTML5). Mapping 1:1 dal WPF (Border posizionati = div). Hit-testing triviale via `event.target.dataset`, CSS borders per i muri, hover nativo. 1.125 celle (25x45) gestibilissime dal DOM moderno.

**Stack UI**: TypeScript puro + CSS vanilla. Niente React, niente Tailwind. Motivazioni: il resto dell'app e' zero-framework; il pattern di drag-painting (mutazione cella per cella su mousemove) contrasta con il modello di re-render di React; bundle size. Il CSS dell'editor va in un file dedicato `leveleditor.css` separato da `styles.css` — nessuna modifica al CSS globale esistente.

---

## Task List di implementazione

### Struttura file

```
Web/src/grid/GridSerializer.ts              ✅ DONE
Web/src/grid/LevelValidator.ts              ✅ DONE  (Task 16)
Web/src/pages/leveleditor.ts                ✅ DONE
Web/src/pages/leveleditor/                  ✅ DONE
  editorTypes.ts                            ✅ DONE
  editorState.ts                            ✅ DONE
  editorGrid.ts                             ✅ DONE
  editorSidebar.ts                          ✅ DONE
  editorInteraction.ts                      ✅ DONE
  editorFileIo.ts                           ✅ DONE
  leveleditor.css                           ✅ DONE
Web/src/pages/router.ts                     ✅ DONE
Web/src/pages/menu.ts                       ✅ DONE
Web/src/assets/LevelLoader.ts               ✅ DONE  (Task 14 - preview)
Web/src/ui/GameTexts.ts                     ✅ DONE  (Task 12 - testi embedded)
Web/tests/debug-leveleditor.spec.ts         ✅ DONE  (5 test suite)
```

### Task 1 — Estrarre `serializeMark` ✅

### Task 2 — Lookup maps runtime ✅

### Task 3 — State management ✅

### Task 4 — Rendering griglia CSS ✅

### Task 5 — Sidebar tool palette ✅

### Task 6 — Interazione mouse e keyboard ✅

### Task 7 — File I/O ✅

### Task 8 — Wiring finale e routing ✅

### Test Playwright ✅

### Checklist di verifica

- [x] `npm run build` passa
- [x] `#/level-editor` carica griglia 25x45 vuota
- [x] Drag painting terreno funziona (verificato con RGB assert Playwright)
- [x] Dirty tracking avvisa prima di perdere modifiche
- [x] Blocco mobile attivo
- [ ] Aprire un `.mark` esistente → griglia si popola con colori/muri/oggetti corretti (non testabile automaticamente — file dialog)
- [x] Piazzamento oggetti funziona (Task 9 risolto — Playwright test 2 OK)
- [x] Piazzamento nemici con waypoint auto-generati (Task 10 risolto — Playwright test 5 OK)
- [ ] Muri con hover+frecce funzionano — da verificare manualmente
- [x] Salvataggio valida prima di produrre `.mark` (Task 16 — LevelValidator.ts integrato)
- [x] Keyboard shortcuts funzionano e cambiano tab sidebar (Task 11 risolto — Playwright test 3 OK)
- [x] Testi embedded nel .mark (Task 12 — getStringCodeText con fallback)
- [x] "Testa in gioco" button funziona via sessionStorage + route `/game/preview/0` (Task 14)
- [x] Autosave ogni 30s in sessionStorage + ripristino al riavvio (Task 15)

---

## Task da fare

### Task 17 — Texture catalog panel + filtro sidebar per spritepack

**Priorità: ALTA** (non più opzionale)

**Files**: `Web/src/pages/leveleditor/packCatalog.ts` (nuovo), `editorSidebar.ts`, `leveleditor.ts`, `leveleditor.css`

**Obiettivo A — Catalog panel:**
Mostrare nell'editor un catalogo visivo delle texture del pack selezionato. Strip fissa ~70px sotto la toolbar (o popover se la strip schiaccia troppo la griglia), scrollabile su asse X, miniature 48×48 raggruppate per sezione (Celle, Muri, Decal, Riempimento, Porte). Si aggiorna al cambio pack.

**Obiettivo B — Filtro sidebar in base al pack (aggiunta):**
Attualmente la sidebar mostra tutti i tipi di Ground/Muro/Decal indipendentemente da cosa esiste realmente nel pack selezionato (es. il pacco 1 potrebbe non avere tutte le 10 varianti Terra). Al cambio pack, l'editor deve:
1. Leggere il JSON dello spritesheet per il pack corrente
2. Determinare quali tipi ground (cell1–cell10), muri (wall_horizontal/vertical_N), decal (decal_1–decal_8) esistono davvero
3. Disabilitare visivamente (opacity + `pointer-events: none`) i radio button della sidebar per i tipi non presenti nel pack

Questo si integra naturalmente con il loader del catalog: la stessa `FrameMap` usata per il catalog viene usata per decidere quali radio disabilitare.

**Analisi spritesheet:**
- 6 spritesheet SS0–SS5, 2048×2048px, tile 192×192px
- Solo SS0–SS3 contengono frame `Grounds/`
- Frame cross-spritesheet: i frame di un pack possono essere su sheet diversi
- Naming: `Grounds/paccoN/cell1`, `Grounds/paccoN/Riempimento/riempimento_1`, `Grounds/paccoN/RandomWalls/wall_horizontal_1`, `Grounds/paccoN/Decals/decal_1`, `Grounds/paccoN/Door/door_1`

**Implementazione:**

1. `packCatalog.ts`: fetch parallelo SS0–SS3 JSON → `FrameMap` unificata → cachata in memoria
2. `getCatalogFrames(packId, frameMap)` → `CatalogSection[]` filtrate per `Grounds/pacco{packId}/`
3. Strip di canvas 48×48 (`ctx.drawImage`) per ogni tile trovata
4. `getPackAvailableGrounds(packId, frameMap): Set<number>` → ground type values disponibili → passata alla sidebar per disabilitare i radio assenti
5. La sidebar espone `updatePackFilter(availableGrounds: Set<number>)` che aggiorna disabled/opacity dei radio

**Non fa:**
- Non renderizza le texture sulla griglia (coperto da Task 14)
- Non gestisce varianti random/dirty (solo prima variante)
- Non mostra normal/spec map

---

### Task 18 — Preview "Testa in gioco" in nuovo tab

**Priorità: ALTA**

**File**: `Web/src/pages/leveleditor.ts`

**Problema:** il pulsante "Testa in gioco" naviga nello stesso tab, perdendo lo stato dell'editor in corso.

**Fix:** sostituire `window.location.hash = '#/game/preview/0'` con apertura in nuovo tab:

```typescript
window.open(
    window.location.origin + window.location.pathname + '#/game/preview/0',
    '_blank'
);
```

La `sessionStorage` è condivisa all'interno della stessa origin/tab ma NON tra tab diversi. Occorre usare `localStorage` al posto di `sessionStorage` per la chiave `editor-preview`, oppure passare i dati via URL (base64 encode del contenuto .mark, ma potrebbe essere troppo lungo).

**Soluzione consigliata:** usare `localStorage` per la chiave `editor-preview` (non `sessionStorage`), con una flag `editor-preview-ready: 'true'` che il nuovo tab legge e poi cancella. Il nuovo tab legge da localStorage, carica il livello, poi rimuove la chiave.

**File da modificare:**
- `leveleditor.ts`: cambia `sessionStorage` → `localStorage` per `PREVIEW_SESSION_KEY`, usa `window.open(..., '_blank')`
- `LevelLoader.ts`: cambia `sessionStorage.getItem` → `localStorage.getItem` per folder `'preview'`

---

### Task 19 — Regola muri doppi (editor + validatore)

**Priorità: ALTA**

**Files**: `editorState.ts`, `LevelValidator.ts`

**Problema:** è possibile piazzare un muro Top su una cella e anche un muro Bottom sulla cella adiacente sopra, creando un "muro doppio" ridondante e potenzialmente confusionario per il renderer.

**Regola:** se la cella `(col, row)` ha un muro su lato X, la cella adiacente sul lato X non può avere un muro sul lato opposto (perché sarebbe lo stesso confine fisico).

Mapping lato → cella adiacente → lato opposto:
| Lato su (col,row) | Cella adiacente | Lato da bloccare |
|---|---|---|
| Top (WP_TOP) | (col, row-1) | Bottom (WP_BOTTOM) |
| Bottom (WP_BOTTOM) | (col, row+1) | Top (WP_TOP) |
| Left (WP_LEFT) | (col-1, row) | Right (WP_RIGHT) |
| Right (WP_RIGHT) | (col+1, row) | Left (WP_LEFT) |

**In `setWallType` (editorState.ts):** prima di applicare il muro, controllare se la cella adiacente ha già un muro sul lato opposto. Se sì, bloccare e restituire un messaggio di errore (attualmente ritorna solo `boolean`).

Cambiare la firma:
```typescript
// Prima: boolean (ok/ko)
// Dopo: { ok: boolean; reason?: string }
export function setWallType(...): { ok: boolean; reason?: string }
```

**In `validateLevel` (LevelValidator.ts):** aggiungere una validazione semantica che scansiona tutte le celle e segnala come warning ogni coppia di muri doppi trovata.

**In `editorInteraction.ts`:** gestire il nuovo return type e mostrare il feedback (vedi Task 20).

---

### Task 20 — Feedback utente per azioni bloccate

**Priorità: ALTA**

**Files**: `editorInteraction.ts`, `leveleditor.ts`, `leveleditor.css`

**Problema:** quando l'editor blocca silenziosamente un'azione (singleton già piazzato, muro in posizione invalida, muro doppio, ecc.) l'utente clicca e non capisce perché non succede nulla.

**Lista completa delle azioni attualmente bloccate silenziosamente:**
1. Piazzamento singleton già presente (Giocatore, Chiave, Uscita, Goblin)
2. Muro Left/Right/Bottom con tipo Ingresso o Uscita (validazione posizione)
3. Muro doppio su cella adiacente (dopo Task 19)
4. Futuro: ground type non disponibile nel pack corrente (dopo Task 17)

**Fix:** aggiungere un sistema di toast non bloccante nell'editor:

```typescript
// In leveleditor.ts
function showEditorToast(message: string, type: 'error' | 'warning' = 'error'): void {
    // Crea/aggiorna uno span .editor-toast posizionato in overlay sulla griglia
    // Auto-scompare dopo 2.5 secondi
}
```

Il toast deve essere:
- Posizionato in overlay sulla griglia (non un `alert()` bloccante)
- Auto-dismiss dopo ~2.5s
- Colore rosso per errori, giallo per warning
- Non interrompe il flusso di lavoro

**Integrazione:**
- `attachInteraction` riceve una callback `onBlocked: (reason: string) => void`
- `applyObject` e `applyWall` chiamano `onBlocked(reason)` invece di fare `return` silenzioso
- `leveleditor.ts` passa `showEditorToast` come `onBlocked`

CSS: `.editor-toast` posizionato `position: fixed; bottom: 20px; left: 50%; transform: translateX(-50%); z-index: 9999`

---

### Task 21 — Bug: prima apertura file non funziona

**Priorità: ALTA**

**File**: `editorFileIo.ts` (funzione `openFile`)

**Sintomo:** alla prima apertura del level editor, premere "Apri" e selezionare un `.mark` non carica nulla. Se si annulla e si riprova (o si cambia file), il caricamento funziona.

**Probabile causa:** il `<input type="file">` creato dinamicamente e cliccato programmaticamente viene garbage-collected prima che il browser riesca a processare la selezione del file, oppure l'evento `onchange` non si attiva perché l'input viene rimosso dal DOM prima del completamento. Un altro candidato: la `Promise` viene resolved con `null` per via dell'evento `oncancel` che scatta prima di `onchange` su alcuni browser/OS.

**Fix da investigare:**
1. Appendere l'input al `document.body` prima del click e rimuoverlo solo dopo la risoluzione
2. Rimuovere il handler `oncancel` (non standard in tutti i browser, potrebbe interferire) e gestire solo `onchange`
3. In alternativa: aggiungere un timeout di sicurezza per risolvere la Promise

**Fix consigliato:**
```typescript
document.body.appendChild(input);
input.onchange = () => {
    // ... gestione file
    document.body.removeChild(input);
};
// Rimuovere oncancel (non affidabile cross-browser)
input.click();
```

