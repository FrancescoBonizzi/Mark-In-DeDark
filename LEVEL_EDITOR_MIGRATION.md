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
Web/tests/debug-leveleditor.spec.ts         ✅ DONE
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
- [ ] Piazzamento oggetti funziona — **BUG NOTO** (vedi Task 9)
- [ ] Piazzamento nemici con waypoint — **BUG NOTO** (vedi Task 10)
- [ ] Muri con hover+frecce funzionano — da verificare manualmente
- [ ] Salvataggio produce `.mark` valido (verificabile con `validate-level.ts`)
- [ ] Keyboard shortcuts funzionano (F, W, D, C, R) — parzialmente: funzionano ma il tab sidebar non cambia (vedi Task 11)

---

## Prossimi task (da fare)

### Task 9 — Bug: Piazzamento oggetti e testi non funziona

**Priorità: ALTA**

**Files**: `editorInteraction.ts`, `editorSidebar.ts`, `leveleditor.ts`

**Sintomi osservati:**

- Selezionare un oggetto dal tab Oggetti e cliccare sulla griglia non piazza nulla
- Il testo (modalita' "A") non funziona

**Root causes da investigare:**

1. **Default selectedObject=0 (Cancella) non visibile:** quando si entra nel tab Oggetti, nessun radio e' pre-selezionato visivamente, ma `state.selectedObject` e' 0 (Cancella). Qualsiasi click sulla griglia risulta in una cancellazione di oggetto — che su cella vuota e' invisibile. Soluzione: al click sul tab Oggetti, auto-selezionare il primo radio non-Cancella (o comunque mostrare Cancella come checked di default).

2. **Possibile problema di event propagation:** gli span interni alla cella (`.cell-object`, `.cell-wall-label`) potrebbero intercettare il click prima del `mousedown` sull'`.editor-grid`. `getCellFromEvent` risale il DOM con `parentElement`, dovrebbe funzionare, ma va verificato con un `console.log` nel test Playwright.

3. **Text mode:** il radio "A Testo" usa `value="-1"` (sentinel). Assicurarsi che `onObjectSelected(-1)` imposti `state.currentAction = EditorAction.Text` e che il click sulla griglia in Text mode chiami `applyText` che apre il `window.prompt`.

**Fix plan:**

- Auto-selezionare il radio Cancella (value=0) nel tab Oggetti al caricamento
- Aggiungere `console.log` temporaneo nell'`onMouseDown` dell'interaction per tracciare col/row e action nel test Playwright
- Verificare che `state.currentAction` sia corretto prima del click sulla griglia
- Per il tab Nemici: ogni radio nemico deve auto-impostare `state.selectedObject` correttamente

### Task 10 — Bug: Piazzamento nemici con waypoint automatico

**Priorità: ALTA**

**Files**: `editorInteraction.ts`, `editorState.ts`, `editorSidebar.ts`

**Problema attuale:** quando si piazza un nemico, il codice mostra un `window.prompt` generico per il "path ID". L'utente deve sapere il formato (`Z-1-1`, `S-2-3`, ecc.) e gestire manualmente i gruppi, il che e' error-prone.

**Comportamento desiderato:** l'editor deve gestire automaticamente i waypoint:

- Ogni tipo di nemico ha un prefisso fisso: `Z`=Zombie, `S`=Scheletro, `G`=Goblin, `T`=Taurus, `H`=Ghost, `B`=Bubu
- L'editor tiene un contatore per gruppo attivo per tipo nemico nell'`EditorState`
- **Flusso di piazzamento:**
  1. Utente seleziona nemico (es. Zombie) dalla sidebar
  2. Prima cella clickata → piazza il nemico con `Z-N-1` (nuovo gruppo N, waypoint 1)
  3. Seconda cella → piazza con `Z-N-2` (stesso gruppo N, waypoint 2)
  4. ... continua a piazzare waypoint nello stesso gruppo finche' l'utente non schiaccia "Chiudi gruppo" o seleziona un'altra tool
  5. Selezionando di nuovo la stessa tool nemico → inizia gruppo N+1

**Aggiunte a EditorState:**

```typescript
enemyWaypointState: {
    activeEnemyType: number | null;
    activeGroup: number;        // numero gruppo corrente
    nextWaypointIndex: number;  // prossimo indice da assegnare
    groupCounters: Record<number, number>; // enemyType → max gruppo usato
} | null;
```

**UI sidebar per nemici:** mostrare il gruppo attivo e il prossimo indice waypoint ("Gruppo Z-3, waypoint 4") con pulsante "Nuovo gruppo" per forzare un nuovo gruppo senza dover cambiare tool.

**Prefissi per tipo:**
| ObjectType | Prefisso |
| ---------- | -------- |
| 5 Zombie | Z |
| 6 Scheletro | S |
| 7 Goblin | G |
| 8 Taurus | T |
| 10 Ghost | H |
| 11 Bubu | B |

**Scansione dello stato esistente:** `createStateFromBundle` deve parsare i waypoint esistenti nel file .mark per inizializzare `groupCounters` ai valori corretti (evitando conflitti di gruppo con livelli preesistenti).

### Task 11 — UX: Keyboard shortcuts → switch tab sidebar

**Priorità: MEDIA**

**File**: `editorInteraction.ts`, `editorSidebar.ts`

**Problema:** quando si preme `F` o `R` (Terra), `W` o `D` (Muro), `C` (Cancella/Oggetti), il tab della sidebar non cambia visivamente. L'utente vede il radio aggiornato ma il pannello e' quello sbagliato, e sembra che lo shortcut non funzioni.

**Fix:** esporre da `editorSidebar.ts` una funzione `switchToTab(tab: 'ground' | 'wall' | 'obj')` e chiamarla dall'`onKeyDown` dell'interaction:

```
F → switchToTab('ground'), syncGroundRadio(1)
R → switchToTab('ground'), syncGroundRadio(8)
W → switchToTab('wall'),   syncWallRadio(1)
D → switchToTab('wall'),   syncWallRadio(0)
C → switchToTab('obj'),    syncObjRadio(0)
```

La funzione `switchToTab` deve riprodurre il tab-click: aggiornare le classi `.active` su tab button e pannello, e chiamare `onActionChanged` con l'`EditorAction` corrispondente.

### Task 12 — Feature: Testi embedded nel formato `.mark`

**Priorità: MEDIA**

**Problema attuale:** il campo `STRING_CODE` nel file `.mark` contiene un ID numerico (es. `637284382845827427`) che viene risolto a runtime cercando in `GameTexts.ts:STRING_CODE_MAP`. Questo significa che i file `.mark` non sono auto-sufficienti: senza il codice sorgente del gioco i testi sono illeggibili.

**Obiettivo:** far si' che il file `.mark` contenga il testo direttamente, eliminando la dipendenza da `STRING_CODE_MAP`.

**Proposta di formato:** il campo `STRING_CODE` diventa il testo stesso (stringa libera UTF-8). Vincoli:

- Non puo' contenere `|` (gia' proibito essendo separatore di campo) → sostituire `|` con `\|` se necessario, oppure usare un altro encoding
- Non puo' contenere newline → `\n` escaped
- Limite pratico: il testo e' breve (una riga di pensiero/dialogo)

**Impatto:**

- `GridLoader.ts`: nessuna modifica (il campo STRING_CODE e' gia' letto come stringa)
- `GridSerializer.ts`: nessuna modifica (il campo viene serializzato as-is)
- `GameTexts.ts`: rimuovere `STRING_CODE_MAP`; la funzione `getStringCodeText(code)` puo' essere eliminata o semplificata a `return code` (il code E' gia' il testo)
- Il codice di gioco che chiama `getStringCodeText()` deve essere aggiornato per usare direttamente `cell.stringCode`
- I file `.mark` esistenti: i vecchi ID numerici diventerebbero testi letterali non tradotti — migrare manualmente o con script di conversione
- `validate-level.ts`: nessuna modifica critica
- Level Editor: il Text mode del livello editor diventa triviale: `prompt()` chiede il testo direttamente, senza lookup

**Dipendenza con Task 9:** conviene fare prima il Task 9 (fix piazzamento oggetti/testo) perche' cambia il flusso del Text mode. Il Task 12 poi semplifica ulteriormente quel flusso.

**Piano di migrazione livelli esistenti:** creare script `Web/scripts/migrate-stringcodes.ts` che legge i `.mark` esistenti, risolve i vecchi ID via `STRING_CODE_MAP`, e riscrive i file con il testo diretto. Poi eliminare `STRING_CODE_MAP` da `GameTexts.ts`.

### Task 13 — Test Playwright per piazzamento oggetti, nemici e testo

**Priorità: MEDIA** — fare dopo Task 9 e Task 10 (i bug devono essere risolti prima)

**File**: `Web/tests/debug-leveleditor.spec.ts`

Estendere il test esistente con scenari che verificano automaticamente le funzionalita' risolte:

**Scenari da aggiungere:**

1. **Piazzamento oggetto singleton (Giocatore):**
   - Selezionare il radio "Giocatore" nel tab Oggetti
   - Cliccare su una cella
   - Verificare che la cella contenga lo span `.cell-object` con emoji `🧍`
   - Verificare che il radio "Giocatore" sia disabilitato dopo il piazzamento
   - Verificare che cliccare di nuovo su un'altra cella non piazzi un secondo Giocatore

2. **Piazzamento candele con quantita':**
   - Intercettare `window.prompt` via `page.on('dialog', ...)` e rispondere `'3'`
   - Selezionare il radio "Candele", cliccare su una cella
   - Verificare che la cella contenga `.cell-quantity` con testo `'3'`

3. **Piazzamento nemico con waypoint automatico (dopo Task 10):**
   - Selezionare Zombie dal tab Nemici
   - Cliccare su cella (col=10, row=10) → deve apparire emoji `🧟`
   - Cliccare su cella (col=12, row=10) → deve apparire emoji `🧟`
   - Verificare che `data-waypoint` o il contenuto del `.cell-quantity` mostri i codici `Z-1-1` e `Z-1-2`

4. **Text mode:**
   - Intercettare `window.prompt` e rispondere `'Testo di test'`
   - Selezionare "A Testo", cliccare su una cella
   - Verificare che la cella contenga `.cell-text-marker` con testo `'A'`
   - Verificare `title` dell'elemento = `'Testo di test'`

5. **Cancellazione oggetto:**
   - Selezionare Cancella, cliccare sulla cella con il Giocatore
   - Verificare che la cella non contenga piu' `.cell-object`
   - Verificare che il radio "Giocatore" sia di nuovo abilitato

**Tecnica per intercettare prompt:** `page.on('dialog', dialog => { dialog.accept('valore'); })`

### Task 14 — Feature: Pulsante "Testa in game"

**Priorità: MEDIA**

**Files**: `Web/src/pages/leveleditor.ts`, `Web/src/pages/leveleditor/editorFileIo.ts`, `Web/src/pages/router.ts`, `Web/src/pages/gamebootstrap.ts`

**Obiettivo:** pulsante nella toolbar "Test in game" che salva il livello corrente in `sessionStorage` e naviga direttamente a una route `/preview` che lo carica, senza dover salvare su file.

**Implementazione:**

1. **Salvataggio in sessionStorage:**

   ```typescript
   // In editorFileIo.ts o leveleditor.ts
   export function saveToSessionStorageForPreview(state: EditorState): void {
     const content = serializeMark(state.grid);
     sessionStorage.setItem("leveleditor_preview", content);
   }
   ```

2. **Route `/preview`** in `router.ts`:
   - Legge `sessionStorage.getItem('leveleditor_preview')`
   - Se presente, chiama `initGame` con il contenuto deserializzato al posto di un livello da file
   - Se assente, reindirizza al menu con un messaggio

3. **Modifica a `gamebootstrap.ts` / `GridLoader.ts`:**
   - `initGame` deve supportare un parametro opzionale `levelContent: string` che bypassa il caricamento da file
   - In alternativa: scrivere il livello in sessionStorage e modificare `loadAndParseLevel` per controllare prima la session storage se il livello richiesto e' `'preview'`

4. **Toolbar:** aggiungere dopo il pulsante "Salva" un pulsante "Test in game" (shortcut consigliato: `T`):

   ```
   [Nuovo] [Apri] [Salva] [Test in game] ● ...
   ```

   Al click: `saveToSessionStorageForPreview(state)` → `window.location.hash = '#/preview'`

5. **Leave hook del preview:** al ritorno dalla route preview al level editor, pulire `sessionStorage.removeItem('leveleditor_preview')`.

**Nota:** usare `window.location.hash` invece di `router.navigate()` per rispettare la regola CLAUDE.md sull'evitare import circolari con `router.ts`.

### Task 15 — Feature: Autosave in sessionStorage

**Priorità: MEDIA**

**Files**: `Web/src/pages/leveleditor.ts`, `Web/src/pages/leveleditor/editorFileIo.ts`

**Obiettivo:** salvare automaticamente ogni 30 secondi il livello corrente in `sessionStorage`, cosi' se l'utente chiude accidentalmente il browser non perde il lavoro. Al caricamento dell'editor, se esiste un autosave piu' recente del file aperto, offrire il ripristino.

**Chiave sessionStorage:** `'leveleditor_autosave'`
**Metadati autosave** (oggetto JSON separato in `'leveleditor_autosave_meta'`):

```typescript
interface AutosaveMeta {
  filename: string;
  timestamp: number; // Date.now()
  isDirty: boolean;
}
```

**Implementazione:**

1. **`startAutosave(state, onSave)` in `editorFileIo.ts`:**
   - Ritorna un `intervalId` (da passare a `stopAutosave` nel cleanup)
   - Ogni 30 secondi: serializza il livello e salva in sessionStorage
   - Non salva se `!isDirty(state)` (ottimizzazione: evita IO inutili)

2. **`stopAutosave(intervalId)` in `editorFileIo.ts`**

3. **`loadAutosave()` in `editorFileIo.ts`:**
   - Legge `sessionStorage.getItem('leveleditor_autosave')`
   - Ritorna `{ content, meta } | null`

4. **Al mount della pagina editor** (`renderLevelEditorPage`):
   - Controllare se esiste un autosave
   - Se si', mostrare un banner: `"Trovato autosave di '{filename}' del {data ora}. Vuoi ripristinarlo?"` con pulsanti "Ripristina" / "Ignora"
   - Dopo il ripristino o l'ignora, avviare `startAutosave`

5. **Indicatore ultimo salvataggio** nella toolbar:
   - Span `#autosave-status` accanto al dirty indicator
   - Testo: `"Autosave: 14:32:05"` (ora dell'ultimo autosave automatico) oppure `"Autosave: mai"` all'avvio
   - Aggiornato ad ogni salvataggio automatico

6. **Cleanup:** chiamare `stopAutosave` in `destroyLevelEditor()`.

7. **Conflitto con "Testa in game" (Task 14):** la chiave `'leveleditor_preview'` e' separata da `'leveleditor_autosave'` — nessun conflitto.

### Task 16 — Feature: Validazione al salvataggio file

**Priorità: MEDIA**

**Files**: `Web/src/pages/leveleditor/editorFileIo.ts`, `Web/scripts/validate-level.ts`

**Obiettivo:** eseguire le stesse validazioni di `validate-level.ts` direttamente nel browser al momento del salvataggio su file, mostrando eventuali errori in un banner nella pagina invece di richiedere all'utente di lanciare uno script CLI separato.

**Problema tecnico:** `validate-level.ts` e' uno script CLI Node.js che legge file con `fs`. Non puo' essere importato direttamente nel browser. Soluzione: estrarne la logica di validazione in un modulo condiviso browser-compatibile.

**Piano:**

1. **Creare `Web/src/grid/LevelValidator.ts`** (nuovo, browser-compatible):
   - Estrarre la logica di validazione da `validate-level.ts` (rimuovere i file I/O Node.js)
   - Firma: `validateLevel(bundle: GridBundle): ValidationResult`

   ```typescript
   interface ValidationError {
     severity: "error" | "warning";
     message: string;
   }
   interface ValidationResult {
     valid: boolean;
     errors: ValidationError[];
     warnings: ValidationError[];
   }
   ```

   - Regole da validare (da `validate-level.ts`):
     - Esattamente 1 Giocatore (ObjectType=1)
     - Esattamente 1 Chiave (ObjectType=2)
     - Almeno 1 Uscita (ObjectType=3)
     - Waypoint nemici: ogni gruppo deve avere almeno 2 waypoint; indici consecutivi senza buchi
     - Nessuna cella con oggetto nemico su NonCella (groundType=0)
     - Coerenza muri ingresso/uscita (Left/Right/Bottom non ammettono Ingresso/Uscita)

2. **Modificare `validate-level.ts`** per importare da `LevelValidator.ts` invece di duplicare la logica.

3. **Chiamare la validazione in `saveFile(state)`** (in `editorFileIo.ts`):

   ```typescript
   export function saveFile(state: EditorState, container: HTMLElement): void {
       const result = validateLevel(state.grid);
       if (result.errors.length > 0) {
           // Mostrare banner rosso con lista errori
           // Chiedere conferma prima di salvare comunque
           if (!window.confirm(`Il livello ha ${result.errors.length} errori:\n${...}\n\nSalvare comunque?`)) return;
       }
       if (result.warnings.length > 0) {
           // Mostrare banner giallo con avvisi (non bloccante)
           showValidationBanner(container, result.warnings, 'warning');
       }
       // ... salvataggio normale
   }
   ```

4. **Banner di validazione** nella pagina editor: div `.validation-banner` posizionato sopra la toolbar o sotto di essa, con X per chiuderlo. Verde se ok, giallo per warning, rosso per errori.

---

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
