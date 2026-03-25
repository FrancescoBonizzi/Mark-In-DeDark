# INTERACTION_AND_OBJECTS.md — Analisi Fase 2

Prerequisito ai Task 3, 4 e 5.
Confronto tra il comportamento C# originale e l'implementazione TypeScript attuale.

---

## 1. Interaction Logic — Come il player interagisce con le porte

### 1.1 Algoritmo C# (`GameGrid.SwitchDoorIfAny`)

Non c'è raycast né calcolo del "fronte" basato sulla direzione del player. L'algoritmo è:

```
1. Prendi la cella corrente del player: GetCellAtPixel(player.WorldCenter)
2. Calcola l'indice (i, j) della cella → GetCellIndexByPixel(cell.CenterPosition)
3. Recupera le 4 celle adiacenti via indice grezzo: (i, j-1), (i, j+1), (i-1, j), (i+1, j)
   ↑ NOTA: usa indici diretti, NON i link di grafo — perché quando la porta è chiusa
     i link non esistono ancora (vengono aggiunti solo all'apertura)
4. Per ogni porta WallInteractiveDoor trovata nelle celle adiacenti:
   - Se aperta → Close()
   - Se chiusa → Open() + aggiunge link bidirezionali tra le celle
   - Se in movimento (Opening/Closing) → ignora
```

**In pratica:** qualunque porta entro 1 cella dal player viene attivata, indipendentemente
dalla direzione in cui il player sta guardando.

### 1.2 Stato TS attuale — **BUG (Task 3)**

La logica di `GameGrid.switchDoorIfAny()` è **correttamente portata** in TS (riga 646).

Il bug è nella mappatura dei tasti in `Controller.ts`:

| Tasto | C# (atteso) | TS (attuale) |
|-------|-------------|--------------|
| `Space` | `DoorSwitch` (interazione) | `Candle` (piazza candela) |
| `Control` | ? | `DoorSwitch` |

`Player.handleInput()` gestisce `DoorSwitch` correttamente (riga 92-96), ma non
viene mai ricevuto via Space perché Space produce `Candle`.

**Fix Task 3:** In `Controller.ts`, cambiare `' ': CharacterActions.DoorSwitch`.
Decidere dove spostare il piazzamento candele (probabile `'e'` / `'E'` / `'f'` / `'F'`).

> **Attenzione:** il C# probabilmente usava Space anche per le candele, ma con
> priorità porte: controlla prima se c'è una porta, altrimenti piazza candela.
> Alternativa: logica dual-action in `Player.handleInput()` — se `DoorSwitch`
> e non ci sono porte nella cella → piazza candela come fallback.

---

## 2. Brazier Specs — BrazierLit (Task 5)

### 2.1 ObjectType e causa invisibilità

**ID nel file `.mark`:** `ObjectTypes.Lucina = 12`

**Perché non compare nel rendering:**

```
GameGrid.ts riga 269:
  case ObjectTypes.Lucina:   ← raggruppato con nemici (Zombie, Scheletro, etc.)
      this.enemyPlacements.push({ objectType: Lucina, ... });

EnemyFactory.ts:
  switch (placement.objectType) {
    case ObjectTypes.Zombie:   ...
    case ObjectTypes.Scheletro: ...
    case ObjectTypes.Taurus:   ...
    // Lucina non ha case → nessuna azione → silenziosamente ignorato
  }
```

Il braciere viene collezionato nell'array `enemyPlacements` ma l'`EnemyFactory`
non lo gestisce → zero rendering, zero luce, zero ombra.

### 2.2 Specifiche visive C# (`BrazierLit.cs`)

Il braciere in C# è composto da **4 layer** separati:

| Layer | Descrizione | Z-Index | Note |
|-------|-------------|---------|------|
| `_brazierAnimation` | Sprite animata del braciere | `zForGenericObject(y)` | Stessa formula di muri/oggetti |
| `_shadowAnimation` | Ombra animata | sotto il braciere | Sprite-based, come character shadows |
| `_normal` | Normal map statica | (stesso Z del brazier) | Per il lighting pipeline |
| `_glow` | Sprite glow circolare | `Z_OVERALL = 3` | `Effects/glow`, scala 0.7, posizione +y(-20) |

### 2.3 Luce associata (`MarkLight`)

```
SurroundingLight = new MarkLight(
    color1:          gameConfig.CandleColorRGB1,   // stessi colori della candela
    color2:          gameConfig.CandleColorRGB2,
    z:               67f,                           // più basso della candela (z=90)
    opacity:         1f,
    distanceSquared: 7_280f,                        // più corto della candela (20_600)
    castsShadows:    false,                         // NON proietta ombre hull
    penumbraScale:   150f
);
SurroundingLight.Position      = worldCenter + (0, -50f);  // offset verso l'alto
SurroundingLight.DeltaForWall  = (0, 4f);
SurroundingLight.DistanceSqWall = 17_000f;
```

**Differenze rispetto alla candela:**
- `distanceSquared` più piccolo (7 280 vs 20 600) → luce più raccolta
- `z` più basso (67 vs 90) → luce meno "alta"
- `castsShadows: false` → non genera penumbra, solo normal map lighting
- Posizione Y offset `-50` (candela non ha offset)

### 2.4 Animazione

- Sprite key: `"BrazierLit"` (spritesheet esistente, verificato in `Spritesheet-*.json`)
- Shadow key: `"BrazierShadow"`
- Glow key: `"Effects/glow"` (già usato da `Candle.ts` — riutilizzabile)
- Stessa logica flickering della candela (lerp pingpong tra color1/color2 su 3000ms)

### 2.5 Fix richiesto (Task 5)

1. Creare `Web/src/objects/BrazierLit.ts` (analogo a `Candle.ts`)
2. In `GameGrid.ts`: spostare `ObjectTypes.Lucina` fuori da `enemyPlacements`,
   aggiungere array `brazierPlacements` (come `candlePlacements`, `trapPlacements`)
3. In `gamebootstrap.ts`: istanziare `BrazierLit` per ogni placement,
   aggiungere `brazier.surroundingLight` a `shadowSystem`
4. In `LevelConfig.ts`: aggiungere texture per `brazier_normal` se necessario

---

## 3. HUD Triggers — Messaggi a schermo (Task 4)

### 3.1 Sistema popup C# (`GameHUD.ShowMessage` + `PopupObject`)

Ogni popup:
- Dura **6 secondi**
- Alpha segue curva parabola: `alpha = 4 * t * (1 - t)` (t ∈ [0,1])
- Fluttua verso l'alto di **60px** durante la vita
- Se stesso testo già in coda → non duplica
- Nuovi messaggi si accodano 50px sotto l'ultimo

### 3.2 Catalogo eventi che scatenano messaggi

| Evento | Testo C# (es.) | Colore | Trigger |
|--------|---------------|--------|---------|
| Player entra in cella con `stringCode` | testo dal `.mark` | Giallo | `ShowTextTriggerObject` (collision check) |
| Candele insufficienti per piazzare | "Non hai abbastanza candele" | Rosso | `CandlesManager.placeCandle()` quando count=0 |
| Chiave raccolta | "Chiave trovata! Trova l'uscita" | Bianco | `Key.collect()` |
| Uscita aperta | (nessun popup, solo animazione) | — | `Exit.setOpen()` |
| Porta senza chiave / locked | Non trovato nel C# analizzato | — | N/A? |

> **Nota:** Il C# non mostra un popup "porta bloccata". L'uscita si apre automaticamente
> dopo raccolta della chiave — non c'è "porta locked" nel senso tradizionale.
> L'unica "porta locked" è l'uscita stessa, gestita visivamente.

### 3.3 `ShowTextTriggerObject` — tutorial/aiuto da `.mark`

Nel formato `.mark`, ogni cella ha un campo `stringCode` (stringa libera).
In C# questo diventa una `ShowTextTriggerObject`: quando il player entra nella cella,
viene chiamato `GameHUD.ShowMessage(stringCode, Color.Yellow)`.

In TS il `stringCode` viene già parsato da `GridLoader.ts` e presente in
`GridBundle.cells[].stringCode`, ma **non viene usato** — nessun trigger di testo
è attivo.

### 3.4 Stato attuale TS — **MANCANTE (Task 4)**

`GameHud.ts` contiene solo timer + contatore candele. Mancano:
- Sistema popup (classe `PopupMessage` o simile)
- Hook in `CandlesManager` per "candele insufficienti"
- Hook in `Key.collect()` per "chiave trovata"
- Trigger su celle con `stringCode` (loop in gamebootstrap simile ai trap-check)

### 3.5 Fix richiesto (Task 4)

1. Aggiungere metodo `showMessage(text: string, color?: number)` a `GameHud`
   con animazione CSS (fade + translateY, stessa parabola del C#)
2. Chiamare da:
   - `CandlesManager.placeCandle()` se `leftCandles === 0`
   - `Key.collect()` per messaggio chiave trovata
   - Game loop in `gamebootstrap.ts`: controllo `stringCode` su celle visitate
     (aggiungere `Set<GameCell>` per le celle già triggerate)

---

## 4. Mapping Table — File `.mark` → Classi TypeScript

### 4.1 ObjectTypes (colonna oggetto per cella)

| ID | Enum | Classe TS | Status |
|----|------|-----------|--------|
| 0 | `Cancella` | — | Cella vuota |
| 1 | `Giocatore` | `Player` | ✅ |
| 2 | `Chiave` | `Key` | ✅ |
| 3 | `Uscita` | `Exit` | ✅ |
| 4 | `Candele` | `CandlePack` | ✅ |
| 5 | `Zombie` | `Zombie` | ✅ |
| 6 | `Scheletro` | `Skeleton` | ✅ |
| 7 | `Goblin` | `Goblin` | ✅ |
| 8 | `Taurus` | `Taurus` | ✅ |
| 10 | `Ghost` | `Ghost` | ✅ |
| 11 | `Bubu` | `Bubu` | ✅ |
| **12** | **`Lucina`** | **`BrazierLit`** | **❌ Task 5** |
| 13 | `OggettoNonPassabile1` | sprite statico | ⚠️ in enemyPlacements, non renderizzato |
| 14 | `OggettoNonPassabile2` | sprite statico | ⚠️ in enemyPlacements, non renderizzato |
| 15 | `OggettoPassabile1` | sprite statico | ⚠️ in enemyPlacements, non renderizzato |

### 4.2 GroundTypes (colonna pavimento per cella)

| ID | Enum | Gestito in TS |
|----|------|---------------|
| 0 | `NonCella` | — |
| 1 | `Terra1` | ✅ |
| 3 | `TrappolaMorteIstantanea` | ✅ `InstantlyDieTrap` |
| 4 | `TrappolaTemporizzata` | ✅ `TimedBreakableTrap` |
| 5 | `TriggerGoblin` | ✅ `GoblinTriggerTrap` |
| 6 | `Terra2` | ✅ |
| 7 | `Buio` | ✅ (cella nera, non passabile) |
| 8–28 | `Riempimento1-13`, `Terra3-10` | ✅ |

### 4.3 WallTypes (per ogni lato della cella: Top/Right/Bottom/Left)

| ID | Enum | Effetto |
|----|------|---------|
| 0 | `Niente` | Passabile, nessun muro |
| 1 | `Occludente` | Muro solido |
| 2 | `Fragile` | Muro distruttibile (Taurus) |
| 3 | `Porta` | Porta interattiva (`WallInteractiveDoor`) |
| 4–15 | `Muro1-5`, `Decal1-8` | Muri decorativi/varianti visive |
| 5 | `Ingresso` | Ingresso livello |
| 6 | `Uscita` | Uscita livello |

---

## 5. Piano di Fix (ordinato per Task)

| Task | File | Modifica | Righe stimate |
|------|------|----------|---------------|
| **Task 3** | `Controller.ts` | `' '` → `DoorSwitch`; decidere nuovo tasto candela | ~2 |
| **Task 3** | `Player.ts` | (opt) dual-action Space: porta first, poi candela | ~5 |
| **Task 4** | `GameHud.ts` | `showMessage()` + CSS animation popup | ~40 |
| **Task 4** | `styles.css` | Stili `.hud-popup` con fade+translateY | ~15 |
| **Task 4** | `CandlesManager.ts` | Callback/event "no candles" → `showMessage` | ~5 |
| **Task 4** | `Key.ts` | Callback/event "key collected" → `showMessage` | ~3 |
| **Task 4** | `gamebootstrap.ts` | Loop `stringCode` cells → `showMessage` | ~15 |
| **Task 5** | `objects/BrazierLit.ts` | Nuova classe (animazione + luce) | ~80 |
| **Task 5** | `grid/GameGrid.ts` | `brazierPlacements` array, spostare `Lucina` | ~10 |
| **Task 5** | `gamebootstrap.ts` | Istanziare BrazierLit, aggiungere luce a shadowSystem | ~10 |
