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

## Task completati

### Task 9 — Bug: Piazzamento oggetti e testi ✅

**Risolto:** fix `label.htmlFor` dopo override `radio.id` in `editorSidebar.ts`.
Il radio Cancella (value=0) ora è checked di default nel tab Oggetti.
Testo mode funziona: prompt chiede il testo direttamente (embedded nel .mark).

### Task 10 — Bug: Piazzamento nemici con waypoint automatico ✅

**Risolto:** `ENEMY_PREFIX_MAP` in `editorTypes.ts`, `currentWaypointGroup` in `EditorState`.
Auto-generazione `PREFIX-GROUP-INDEX` (es. `Z-1-1`) con prompt pre-compilato modificabile.
UI selettore gruppo nel pannello Nemici (−/N/+).

### Task 11 — UX: Keyboard shortcuts → switch tab sidebar ✅

**Risolto:** `switchToTab()` esportata da `editorSidebar.ts`, passata a `attachInteraction`.
F→Terra, W/D→Muri, C→Oggetti, R→Terra.

### Task 12 — Feature: Testi embedded nel formato `.mark` ✅

**Risolto:** `getStringCodeText` ora fa fallback al codice stesso (`code || undefined`).
I file `.mark` nuovi contengono il testo direttamente nel campo `STRING_CODE`. Backward compat con ID numerici storici.

### Task 13 — Test Playwright per oggetti, nemici, testo ✅

**Risolto:** 5 test suite in `debug-leveleditor.spec.ts` — tutti passano:

1. Griglia, colori, drag, tab switch
2. Oggetti: Giocatore singleton + cancella
3. Shortcut tastiera → switch tab
4. Testo: marker "A" in cella
5. Waypoint nemici auto-generati

### Task 14 — Feature: Pulsante "Testa in gioco" ✅

**Risolto:** pulsante in toolbar serializza il livello in `sessionStorage['editor-preview']` e naviga a `#/game/preview/0`. `LevelLoader.ts` intercetta `folder==='preview'` e legge da sessionStorage.

### Task 15 — Feature: Autosave in sessionStorage ✅

**Risolto:** `setInterval` da 30s salva `editor-autosave` + `editor-autosave-filename` + `editor-autosave-timestamp`. Al mount dell'editor: `window.confirm` offre ripristino se trovato. Span "Autosalvataggio: HH:MM:SS" in toolbar.

### Task 16 — Feature: Validazione al salvataggio file ✅

**Risolto:** `Web/src/grid/LevelValidator.ts` (browser-compatible, no Node.js deps). `saveFile()` chiama `validateLevel()` prima di scrivere: se errori mostra `confirm` con lista, se solo warning mostra `confirm` con lista.

---

## Task opzionale (fare solo su indicazione esplicita)

### Task 17 — Texture catalog panel: mostra le tile del pack selezionato

**Obiettivo:** mostrare all'interno dell'editor un catalogo visivo delle texture disponibili nel pack correntemente selezionato, cos' chi disegna il livello sa cosa sta per usare prima di iniziare. **Non si tratta di renderizzare le texture sulla griglia** — solo un pannello/strip di anteprima statica.

**Perimetro:** solo le tile "ground" visive per il pack selezionato: celle pavimento (cell1–cell10), riempimento (riempimento_1–13), muri (wall_horizontal_1, wall_vertical_1 — solo il primo campione), decals (decal_1–decal_8), porte (door_1 — solo primo frame). Nessuna tile \_normal o \_spec. Nessuna animazione.

---

**Analisi spritesheet completata** (dati concreti):

- **6 spritesheet**: `Spritesheet-0.png` … `Spritesheet-5.png`, ognuno 2048×2048px
- **Solo SS0–SS3 contengono frame `Grounds/`** — SS4 e SS5 sono solo nemici/effetti
- **Ogni tile e' 192×192px** — tutti i ground frame hanno `w=192, h=192` nel JSON
- **Frame format**: `{ "frame": { "x": N, "y": N, "w": 192, "h": 192 } }` — nessuna rotazione, nessun trim
- **Naming**: `Grounds/paccoN/cell1`, `Grounds/paccoN/Riempimento/riempimento_1`, `Grounds/paccoN/RandomWalls/wall_horizontal_1`, ecc.
- **Cross-spritesheet per pack**: i frame di un singolo pack sono sparpagliati su piu' sheet. Esempio pacco2: `cell1–cell4` → SS0, `cell5–cell10` → SS1, alcuni `RandomWalls` → SS2/SS3. Non si puo' assumere che tutti i frame di un pack stiano in un unico PNG.

**Mappa frame → sheet** (campione per pacco2):
| Frame | Sheet |
|---|---|
| cell1–cell4 | SS0 |
| cell5–cell10 | SS1 |
| Riempimento/riempimento_1–13 | SS0 |
| RandomWalls/wall_horizontal_1 | SS1 |
| RandomWalls/wall_vertical_1 | SS3 |
| Decals/decal_1–8 | SS0 |
| Door/door_1 | SS0 |

---

**Piano di implementazione:**

**1. Loader unificato `Web/src/pages/leveleditor/packCatalog.ts`** (nuovo file):

```typescript
interface FrameRef {
  ssIndex: number; // 0–3
  x: number;
  y: number;
  w: number;
  h: number;
}
type FrameMap = Map<string, FrameRef>; // "Grounds/pacco1/cell1" → FrameRef

async function loadFrameMap(): Promise<FrameMap>;
```

- Fa `fetch` parallelo dei 4 JSON (SS0–SS3)
- Costruisce una `Map<string, FrameRef>` unificata: `frameName → {ssIndex, x, y, w, h}`
- Carica i 4 PNG come `HTMLImageElement` (via `new Image()` + `src`) per poterli usare nei canvas
- Risultato cachato in memoria — chiamare una volta sola al mount dell'editor

**2. Funzione `getCatalogFrames(packId: number, frameMap: FrameMap): CatalogSection[]`**:

```typescript
interface CatalogTile {
  name: string;
  ref: FrameRef;
}
interface CatalogSection {
  title: string;
  tiles: CatalogTile[];
}
```

- Filtra `frameMap` per `Grounds/pacco{packId}/` escludendo `_normal` e `_spec`
- Raggruppa in sezioni: `Celle` (cell*), `Riempimento` (Riempimento/*), `Muri` (RandomWalls/_ — solo prima variante orizzontale e verticale), `Decals` (Decals/_), `Porte` (Door/door_1)
- Ritorna le sezioni ordinate

**3. Rendering del panel**: ogni tile viene disegnata su un `<canvas>` 48×48 usando `ctx.drawImage(img, srcX, srcY, 192, 192, 0, 0, 48, 48)` dove `img` è il PNG del sheet corretto.

**4. Posizionamento del panel**:

- **Prima scelta — strip fissa sotto la toolbar**: banda orizzontale ~70px di altezza, scrollabile su asse X, mostra le tile come miniature 48×48 raggruppate per sezione con separatori verticali e label. Si aggiorna al cambio pack. Occupa spazio verticale fisso ma il catalogo rimane sempre visibile.
- **Fallback — popover**: se durante l'implementazione risulta che la strip schiaccia troppo la griglia (l'editor ha gia' toolbar + body), si passa a un popover attivato da un pulsante `🎨` accanto al selettore pack nella toolbar.

**5. Trigger aggiornamento**: il catalog si ridisegna ogni volta che `state.grid.envTexturePackId` cambia (gia' intercettabile nel listener `change` dell'input pack nella toolbar).

---

**Cosa NON fa questo task:**

- Non renderizza le texture sulla griglia dell'editor (quel requisito e' coperto dal Task 14 "Testa in game" con hot reload)
- Non gestisce le varianti random/dirty (mostra solo la prima variante di ogni tile)
- Non mostra normal map o spec map

**Dipendenze:** nessuna dipendenza dagli altri task — si puo' fare in qualsiasi momento dopo il Task 8 (wiring base).
