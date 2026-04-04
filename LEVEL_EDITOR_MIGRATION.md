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
Web/src/grid/GridSerializer.ts              (NEW - serializeMark estratto)
Web/src/pages/leveleditor.ts                (NEW - page renderer + routing)
Web/src/pages/leveleditor/                  (NEW - directory)
  editorTypes.ts                            (NEW - lookup maps runtime, colori, labels)
  editorState.ts                            (NEW - stato mutabile + operazioni)
  editorGrid.ts                             (NEW - rendering griglia CSS + update celle)
  editorSidebar.ts                          (NEW - 4 tab con radio button)
  editorInteraction.ts                      (NEW - mouse, keyboard, painting)
  editorFileIo.ts                           (NEW - open/save/new/dirty tracking)
  leveleditor.css                           (NEW - stili editor)
Web/src/pages/router.ts                     (MODIFY - aggiungere route)
Web/src/pages/menu.ts                       (MODIFY - aggiungere link)
Web/scripts/ascii-to-mark.ts               (MODIFY - importare da GridSerializer)
```

### Task 1 — Estrarre `serializeMark` in modulo condiviso `[ ]`

**File**: `Web/src/grid/GridSerializer.ts`

Spostare la logica da `Web/scripts/ascii-to-mark.ts:378-408` in un modulo che lavori con `GridBundle`/`CellData`.

- Nuova firma: `serializeMark(bundle: GridBundle): string`
- Iterare `row` 0..24, `col` 0..44, leggere `bundle.grid[col]![row]`, saltare celle null o con `groundType === 0`
- Adattare lo script CLI per importare e usare la nuova funzione
- ~40 righe nuovo modulo + ~20 righe refactor CLI

### Task 2 — Lookup maps runtime `[ ]`

**File**: `Web/src/pages/leveleditor/editorTypes.ts`

I `const enum` di `enums.ts` sono cancellati a compile time — servono array runtime per iterare nei radio button.

- `EditorAction` enum: Ground=0, Wall=1, Object=2, Text=3
- `GROUND_ENTRIES`: array `{ value, label, color }` per i 29 GroundTypes. Colori da `App.xaml`: NonCella=#000, Buio=#808080, Terra1-10=sfumature rosso/marrone, Traps=fuchsia/aqua/lime, Riempimento=#000
- `WALL_ENTRIES`: 19 WallTypes con colore (green/red/blue/aqua/violet)
- `OBJECT_ENTRIES` + `ENEMY_ENTRIES`: separati per tab come nel C#
- `GROUND_COLOR_MAP`, `WALL_COLOR_MAP`: `Record<number, string>` per lookup O(1) nel rendering
- `WALL_CODE_MAP`: tipo → codice corto (M0, MB, P, I, U, D1-D8, M1-M5)
- `OBJECT_LABEL_MAP`: tipo → emoji unicode (🧍 🗝 🚪 🕯 🧟 💀 👺 🐂 👻 👾 🔥)
- `SINGLETON_OBJECTS`: `Set<number>` = {1, 2, 3, 7} (Giocatore, Chiave, Uscita, Goblin)
- `isRiempimento(groundType: number): boolean`
- ~120 righe

### Task 3 — State management `[ ]`

**File**: `Web/src/pages/leveleditor/editorState.ts`

Stato mutabile plain con funzioni di manipolazione. Nessun framework, nessun observable.

```typescript
interface EditorState {
    grid: GridBundle;
    currentAction: EditorAction;
    selectedGround: number;
    selectedWall: number;
    selectedObject: number;
    hoveredCell: { col: number; row: number } | null;
    lastSavedSnapshot: string;
    currentFilename: string;
    singletonPlaced: Set<number>;
}
```

Funzioni:
- `createEmptyState()` — griglia 25x45 vuota
- `createStateFromBundle(bundle, filename)` — scansiona singletons esistenti
- `setGroundType(state, col, row, type)` — se riempimento, cancella anche oggetto
- `setWallType(state, col, row, type, position)` — con validazione (L/R no Ingresso/Uscita, Bottom no Ingresso/Uscita)
- `setObjectType(state, col, row, type, additionalData)` — tracking singletons, auto-set Terra1 se NonCella
- `setStringCode(state, col, row, code)`
- `isDirty(state)` — confronto `serializeMark(state.grid)` vs `lastSavedSnapshot`
- `ensureCell(grid, col, row)` — crea CellData default se null
- ~130 righe

### Task 4 — Rendering griglia CSS `[ ]`

**Files**: `Web/src/pages/leveleditor/editorGrid.ts`, `Web/src/pages/leveleditor/leveleditor.css`

- `renderGrid(container, state)` → crea div wrapper con `display: grid; grid-template-columns: repeat(45, 30px); grid-template-rows: repeat(25, 30px)`. Ritorna `cellElements[col][row]`
- Ogni cella div: `data-col`, `data-row`, sfondo da `GROUND_COLOR_MAP`, bordi 4px colorati per muri, span interni per label oggetto/muro/testo
- `updateCell(cellEl, cellData)` — aggiorna aspetto singola cella
- `setHoverHighlight` / `clearHoverHighlight` — schiarisce/ripristina colore sfondo
- CSS: `.editor-grid`, `.editor-cell` (30x30, position relative), `.cell-object`, `.cell-wall-label`, `.cell-text-marker`
- ~180 righe TS + ~120 righe CSS

### Task 5 — Sidebar tool palette `[ ]`

**File**: `Web/src/pages/leveleditor/editorSidebar.ts`

- 4 tab: Terreno, Muri, Oggetti, Nemici
- Radio button generati da GROUND/WALL/OBJECT/ENEMY_ENTRIES, con sfondo colorato
- Tutti radio condividono `name="editor-tool"` (mutuamente esclusivi come nel C#)
- Tab switching via class toggle
- `updateSingletonButtons(state)` — disabilita radio se singleton gia' piazzato
- ~150 righe

### Task 6 — Interazione mouse e keyboard `[ ]`

**File**: `Web/src/pages/leveleditor/editorInteraction.ts`

- `attachInteraction(gridEl, cellElements, state, onCellChanged)` → ritorna cleanup function
- Hit-testing via `event.target.dataset.col/row` (triviale con div, non serve divisione pixel)
- **Ground mode**: mousedown+mousemove con bottone premuto → drag painting
- **Wall mode**: hover highlights cella, arrow keys piazzano muro sul lato corrispondente (comportamento identico al C#)
- **Object mode**: click. `prompt()` per candele (quantita') e nemici (path ID). Singleton auto-switch a Ground dopo piazzamento
- **Text mode**: click → `prompt()` per string code
- **Shortcuts** (guarded da `!isInputFocused`): F=Terra1, W=Occludente, D=Niente, C=Cancella, R=Riempimento1, Arrows=muro
- ~170 righe

### Task 7 — File I/O `[ ]`

**File**: `Web/src/pages/leveleditor/editorFileIo.ts`

- `openFile()` — `<input type="file" accept=".mark">` + FileReader + `parseLevel()`
- `saveFile(state)` — `serializeMark(state.grid)` → Blob → download link
- `confirmIfDirty(state)` — `confirm()` se ci sono modifiche non salvate
- ~80 righe

### Task 8 — Wiring finale e routing `[ ]`

**Files**: `Web/src/pages/leveleditor.ts`, `Web/src/pages/router.ts`, `Web/src/pages/menu.ts`

- `renderLevelEditorPage(container)` — layout: toolbar (Nuovo/Apri/Salva + dirty% + texture pack) + sidebar + canvas scrollabile
- **Blocco mobile**: all'avvio della pagina, rilevare se il dispositivo e' touch-only o schermo piccolo. Se si', mostrare un messaggio di blocco al posto dell'editor (es. "Il Level Editor richiede un mouse e uno schermo desktop"). Criteri di rilevamento: `window.matchMedia('(pointer: coarse)')` (input primario touch) oppure `window.innerWidth < 1024`. Nessun editor viene inizializzato in questo caso.
- Crea stato iniziale, renderizza sidebar e griglia, attacca interazione e toolbar
- Route `#/level-editor` con `leave` hook (cleanup + dirty check)
- Link nel menu principale
- ~100 righe

### Dipendenze

```
Task 1 ─┐
         ├─ Task 3 ─┬─ Task 4 ──┐
Task 2 ──┘           │           ├─ Task 6 ─┐
                     └─ Task 5 ──┘           ├─ Task 8
                     Task 7 ─────────────────┘
```

Task 1+2 paralleli. Task 4+5 paralleli dopo Task 3. Task 6+7 paralleli dopo Task 4+5. Task 8 finale.

### Checklist di verifica finale

- [ ] `npm run build` passa
- [ ] `#/level-editor` carica griglia 25x45 vuota
- [ ] Aprire un `.mark` esistente → griglia si popola con colori/muri/oggetti corretti
- [ ] Drag painting terreno funziona
- [ ] Muri con hover+frecce funzionano
- [ ] Piazzamento oggetti funziona (incluso prompt candele/nemici)
- [ ] Salvataggio produce `.mark` valido (verificabile con `validate-level.ts`)
- [ ] Dirty tracking avvisa prima di perdere modifiche
- [ ] Keyboard shortcuts funzionano (F, W, D, C, R)
