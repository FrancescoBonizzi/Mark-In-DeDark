# Enemy AI — Mark In DeDark

Documento di riferimento completo per l'intelligenza artificiale dei nemici.
Descrive ogni AI implementata, ogni stato, ogni transizione, ogni parametro e ogni comportamento.
Da leggere in parallelo con `RENDERING_PIPELINE_CSHARP.md` / `RENDERING_PIPELINE_PIXIJS.md` per avere il quadro completo del progetto.

---

## Panoramica architetturale

Tutti i nemici usano il **Strategy Pattern**: ogni nemico possiede un campo `currentAI: IEnemyAI` che può essere sostituito a runtime. L'AI corrente viene invocata ogni frame (quando il nemico non è in movimento) e ritorna un `CharacterActions` — la prossima mossa da eseguire.

```
IEnemyAI
  └─ update(enemy: Enemy, deltaMS: number): CharacterActions
```

`CharacterActions` è un enum con i valori:
`Up | Down | Left | Right | Idle | IdleUp | IdleDown | IdleLeft | IdleRight`

I valori `IdleXxx` significano "rimani fermo ma guarda in quella direzione" (usati dalla fase Incredulity dello Skeleton).

Il cambio di AI avviene tramite `enemy.setNewAI(newAI)`. Questo consente composizione dinamica: un nemico può passare da `PatrolAI` → `KillCandleAI` → `PatrolAI` senza riscrivere nulla.

---

## File principali

| File | Responsabilità |
|------|----------------|
| `Web/src/ai/IEnemyAI.ts` | Interfaccia base |
| `Web/src/ai/IdleAI.ts` | Nemico fermo |
| `Web/src/ai/PatrolAI.ts` | Pattuglia su waypoint |
| `Web/src/ai/PatrolGhostAI.ts` | Pattuglia ignorando i muri |
| `Web/src/ai/RandomMovementAI.ts` | Movimento casuale (fallback) |
| `Web/src/ai/PlayerHunterAI.ts` | Inseguimento diretto del giocatore |
| `Web/src/ai/PlayerHunterLineOfSightAI.ts` | Inseguimento con line-of-sight |
| `Web/src/ai/PatrolHunterLineOfSightAI.ts` | Pattuglia che attiva line-of-sight |
| `Web/src/ai/PatrolChargeAI.ts` | Pattuglia che attiva carica |
| `Web/src/ai/ChargeDirectionAI.ts` | Carica distruttiva in linea retta |
| `Web/src/ai/KillCandleAI.ts` | Inseguimento e spegnimento candela |
| `Web/src/ai/LineOfSight.ts` | Funzione `checkLineOfSight()` condivisa |
| `Web/src/characters/Enemy.ts` | Classe base nemici |
| `Web/src/characters/enemies/Zombie.ts` | Zombie |
| `Web/src/characters/enemies/GhostEnemy.ts` | Ghost |
| `Web/src/characters/enemies/Skeleton.ts` | Skeleton (Scheletron) |
| `Web/src/characters/enemies/Goblin.ts` | Goblin |
| `Web/src/characters/enemies/Taurus.ts` | Taurus |
| `Web/src/characters/enemies/Bubu.ts` | Bubu |
| `Csharp/MarkInDeDark.Library/AI/` | Equivalenti C# |
| `Csharp/MarkInDeDark.Library/Characters/` | Equivalenti C# |

---

## Ciclo di update di Enemy (classe base)

Ogni frame il metodo `update(deltaMS)` di `Enemy` esegue nell'ordine:

1. **Candle collision check** — se `nCellsCandleRange != null` e il nemico è sulla stessa cella di una candela viva → chiama `killCandleLogic(candle)` (sovrascrivibile dai figli, default: `setNewAI(new KillCandleAI(candle, path, prevAI, killDurationMS))`)
2. **Player collision check** — se il rettangolo di collisione del nemico interseca quello del giocatore → chiama `player.die()`, `setNewAI(new IdleAI())`, imposta `_gameFinished = true`
3. **Attacking check** — se il nemico sta eseguendo l'animazione di attacco, aspetta che finisca prima di procedere
4. **AI update** — chiama `currentAI.update(this, deltaMS)`, riceve `CharacterActions`, passa a `handleInput()`

Il metodo `handleInput()` trasforma il `CharacterActions` in un input di movimento cell-by-cell, aggiornando direzione corrente, animazione sprite e posizione via lerp.

---

## Algoritmo Line of Sight (`checkLineOfSight`)

**File:** `Web/src/ai/LineOfSight.ts`
**C#:** metodo `PathToPositionIfFacingAndSameDirection()` in `PatrolHunterLineOfSightAI.cs`

Usato da `PatrolHunterLineOfSightAI` e `PatrolChargeAI`. Ritorna il percorso verso il target oppure `null` se non c'è visuale.

```
Input:  enemy, targetX, targetY, grid
Output: CharacterActions[] | null
```

**Passi in ordine:**

1. Converti posizioni pixel → celle della griglia (`enemyCell`, `targetCell`)
2. Controlla che enemy e target siano sulla **stessa riga** (`sameRow`) o sulla **stessa colonna** (`sameCol`) — se nessuna delle due, ritorna `null`
3. Controlla che il nemico **non stia guardando nella direzione opposta** rispetto al target:
   - Stessa colonna: target sopra + nemico va su → `null`; target sotto + nemico va giù → `null`
   - Stessa riga: target a sinistra + nemico va a sinistra → `null`; target a destra + nemico va a destra → `null`
4. Calcola il percorso con `grid.findActionsToPath(enemyCell, targetCell)`
5. Verifica che **tutte le azioni siano nella stessa direzione** (`actions.every(a => a === actions[0])`) — se ci sono curve (muri in mezzo) ritorna `null`
6. Se tutti i controlli passano → ritorna l'array di azioni

Il punto 3 modella il fatto che un nemico non ha occhi dietro la schiena: se sta camminando verso destra e il giocatore è a sinistra, non lo vede.

---

## AI implementate — dettaglio completo

### 1. `IdleAI`

**Assegnata a:** tutti i nemici dopo che hanno ucciso il giocatore; fase intermedia interna

**Comportamento:** ritorna sempre `CharacterActions.Idle`. Il nemico rimane completamente fermo.

**Transizioni:** nessuna — terminale fino a reset del livello.

---

### 2. `PatrolAI`

**Assegnata a:** Zombie (ricerca candele: `false`), Bubu (ricerca candele: `true`)

**Costruttore:**
```typescript
new PatrolAI(waypoints, grid, candlesManager, player, searchesCandles)
```

**Stato interno:**
- `_waypoints: GameCell[]` — lista ordinata delle celle waypoint definite nel livello
- `_currentWaypointIndex: number` — indice corrente, incrementato ciclicamente
- `_currentActions: CharacterActions[]` — coda delle azioni verso il waypoint corrente

**Algoritmo per frame:**

1. Se `searchesCandles == true` → chiedi a `candlesManager.getNearestCandleActionsPath(enemy, nCellsCandleRange)`:
   - Se restituisce un percorso non vuoto → `enemy.setNewAI(new KillCandleAI(candle, path, this, killDurationMS))` e ritorna subito
2. Se la coda `_currentActions` è vuota → calcola nuovo percorso verso `_waypoints[_currentWaypointIndex]`
   - Se il pathfinding fallisce (nessun percorso) → `enemy.setNewAI(new RandomMovementAI(this))` (fallback)
   - Se arriva al waypoint → `_currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.length`
3. Dequeue e ritorna la prossima azione dalla coda

**Transizioni:**
- → `KillCandleAI` se vede una candela entro range (solo se `searchesCandles`)
- → `RandomMovementAI` se pathfinding fallisce
- → `IdleAI` se il giocatore viene ucciso (gestito da `Enemy` base)

---

### 3. `PatrolGhostAI`

**Assegnata a:** Ghost

**Costruttore:**
```typescript
new PatrolGhostAI(waypoints, grid, player)
```

**Differenza da `PatrolAI`:** usa `grid.findActionsToPathWithoutConsideringWalls()` anziché `findActionsToPath()`. Il Ghost attraversa fisicamente i muri, ma il pathfinding deve ugualmente calcolare un percorso — lo fa ignorando gli ostacoli.

Non ricerca mai candele (il Ghost non ha `searchesCandles`).

**Fase Hovering (teatralità):**

Il Ghost alterna tra due fasi interne: `Patrol` e `Hovering`.

- Ad ogni cella attraversata, se il Ghost è entro **6 celle** (distanza Manhattan) dal player, c'è il **25% di probabilità** di passare a `Hovering` per **500 ms**.
- In `Hovering` il Ghost restituisce `CharacterActions.Idle` (si ferma) e non calcola nuovi percorsi.
- Alla scadenza torna a `Patrol` con un cooldown di **5 celle** prima del check successivo.
- La condizione di attacco (in `GhostEnemy.ts`) non è modificata.
- Mentre è in `Hovering`, `GhostEnemy.update()` espone `PatrolGhostAI.isHovering` per applicare l'effetto visivo: tint dello sprite che pulsa da blu acceso (`0x66AAFF`) a bianco, e opacità della luce aumentata fino a 2.5×.

**Transizioni:**
- → `IdleAI` se il giocatore viene ucciso

---

### 4. `RandomMovementAI`

**Assegnata a:** fallback per qualsiasi nemico che non riesce a raggiungere un waypoint

**Costruttore:**
```typescript
new RandomMovementAI(previousAI: IEnemyAI)
```

**Stato interno:**
- `_previousAI` — AI da ripristinare quando si sblocca
- `_phase: 'attack' | 'move'` — fase corrente
- `_attackTimer: number` — timer per la fase di attacco (500 ms)

**Algoritmo alternato in due fasi:**

**Fase `attack` (500 ms):**
- Chiama `enemy.attack()` (animazione di attacco senza danni reali)
- Ritorna `CharacterActions.Idle`
- Dopo 500 ms → transizione a `move`

**Fase `move`:**
- Prima tenta di eseguire un passo della `_previousAI.update()`
- Se l'azione restituita è diversa da `Idle` → ripristina `_previousAI` (`enemy.setNewAI(this._previousAI)`) e ritorna quell'azione
- Altrimenti → sceglie una direzione casuale tra `[Up, Down, Left, Right]` e ritorna
- Poi torna a `attack`

**Transizioni:**
- → `_previousAI` non appena quella AI ritorna un'azione valida

---

### 5. `PlayerHunterAI`

**Assegnata a:** Goblin

**Costruttore:**
```typescript
new PlayerHunterAI(player, grid, candlesManager)
```

**Stato interno:**
- `_lastPlayerCell: GameCell | null` — ultima cella del giocatore vista
- `_currentActions: CharacterActions[]` — coda percorso verso il giocatore

**Algoritmo per frame:**

1. Se `searchesCandles` e candela vicina → `KillCandleAI` (stessa logica di `PatrolAI`)
2. Leggi `player.currentCell`
3. Se la cella del giocatore è cambiata rispetto a `_lastPlayerCell` → ricalcola il percorso
4. Dequeue azione
5. Se coda vuota → `RandomMovementAI` (fallback)

**Transizioni:**
- → `KillCandleAI` (Goblin cerca le candele)
- → `RandomMovementAI` se pathfinding fallisce

---

### 6. `PatrolHunterLineOfSightAI`

**Assegnata a:** Skeleton

**Costruttore:**
```typescript
new PatrolHunterLineOfSightAI(waypoints, grid, candlesManager, player)
```

**Comportamento:** è un `PatrolAI` con un controllo aggiuntivo ogni frame:

1. Prima di dequeue l'azione di pattuglia → chiama `checkLineOfSight(enemy, player.x, player.y, grid)`
2. Se ritorna un array di azioni non vuoto → `enemy.setNewAI(new PlayerHunterLineOfSightAI(actions, grid, candlesManager, player))`
3. Altrimenti → esegui passo normale di `PatrolAI`

**Transizioni:**
- → `PlayerHunterLineOfSightAI` quando il giocatore è visibile in linea retta

---

### 7. `PlayerHunterLineOfSightAI`

**Assegnata a:** Skeleton (attivata da `PatrolHunterLineOfSightAI`)

**Costruttore:**
```typescript
new PlayerHunterLineOfSightAI(initialActions, grid, candlesManager, player)
```

**State machine interna:**

```
Following
  ├─ (perde visuale o coda esaurita) → Incredulity
  └─ (candela vicina) → KillCandleAI (poi ritorna qui)

Incredulity  [500 ms × 6 = 3 secondi]
  └─ (timer finito) → Lost

Lost
  └─ (immediato) → PatrolHunterLineOfSightAI
```

**Fase `Following`:**
- Ricalcola il percorso ogni volta che il giocatore cambia cella (`checkLineOfSight()` di nuovo)
- Se `checkLineOfSight()` ritorna `null` (persa visuale) → `Incredulity`
- Se coda azioni vuota → `Incredulity`
- Imposta il colore del corpo del nemico al colore "rabbia" (`SkeletronRageColorRGB`)
- Imposta velocità a `SkeletronHuntingSpeed` (più veloce della pattuglia)

**Fase `Incredulity`:**
- Nemico fermo per 6 × 500 ms = 3 secondi totali
- Alterna `CharacterActions.IdleLeft` e `CharacterActions.IdleRight` ogni 500 ms (guarda a sinistra e destra)
- Nessun movimento, solo cambio direzione sprite
- Suono di confusione

**Fase `Lost`:**
- Ripristina colore normale e velocità normale
- Chiama immediatamente `enemy.setNewAI(new PatrolHunterLineOfSightAI(...))` e torna a pattugliare

**Transizioni:**
- → `KillCandleAI` se vede una candela durante `Following`
- → `IdleAI` se uccide il giocatore (gestito da `Enemy` base)

---

### 8. `PatrolChargeAI`

**Assegnata a:** Taurus

**Costruttore:**
```typescript
new PatrolChargeAI(waypoints, grid, candlesManager, player, onShake?)
```

**Comportamento:** è un `PatrolAI` con doppio controllo LoS ogni frame:

1. Controlla LoS verso il giocatore con `checkLineOfSight(enemy, player.x, player.y, grid)`
2. Per ogni candela viva → controlla LoS verso quella candela con `checkLineOfSight(enemy, candle.x, candle.y, grid)`
3. Se uno dei due restituisce azioni → crea `ChargeDirectionAI` nella direzione della prima azione e chiama `enemy.setNewAI(chargeAI)`
4. Altrimenti → esegui passo normale di `PatrolAI`

**Transizioni:**
- → `ChargeDirectionAI` quando vede il giocatore o una candela in linea retta

---

### 9. `ChargeDirectionAI`

**Assegnata a:** Taurus (attivata da `PatrolChargeAI`)

**Costruttore:**
```typescript
new ChargeDirectionAI(direction, grid, candlesManager, player, onShake?)
```

**Costanti:**
```typescript
CHARGE_SPEED_MS  = 100   // 0.1 s/cella — velocità massima
START_DURATION_MS = 1000  // wind-up prima di partire
STUN_DURATION_MS  = 2000  // stordimento dopo impatto
CHARGE_COLOR      = 0x0044FF  // blu
```

**State machine interna:**

```
Starting  [1 secondo]
  └─ (timer scaduto) → Charging

Charging
  ├─ (impatta muro) → Stunned
  └─ (giocatore ucciso) → continua, poi IdleAI

Stunned  [2 secondi]
  └─ (timer scaduto) → PatrolChargeAI
```

**Fase `Starting` (1 secondo):**
- Nemico fermo
- Colore corpo → `CHARGE_COLOR` (blu)
- Suono: `"TaurusStartingCharge"`
- Animazione: `"TaurusChargeBack"` / `"TaurusChargeSide"` / `"TaurusChargeFront"` in base alla direzione
- Ritorna `CharacterActions.Idle` per tutto il secondo

**Fase `Charging`:**
- Imposta `enemy.movementSpeedMS = CHARGE_SPEED_MS` (sovrascrive temporaneamente)
- Animazione: `"TaurusRunBack"` / `"TaurusRunSide"` / `"TaurusRunFront"`
- Per ogni cella percorsa:
  - Se sulla cella c'è una candela → `candle.die(500)` (spegne)
  - Chiama `onShake?.(5)` → camera shake leggero (intensità 5, 1 secondo)
- Controlla se la cella successiva nella direzione è passabile:
  - **Sì** → continua il movimento
  - **No** (muro) → chiama `grid.breakWall(currentCell, direction)` (distrugge il muro), `onShake?.(15)` (shake intenso), transizione a `Stunned`

**Taurus può uccidere altri nemici durante la carica:**
```typescript
// In Taurus.update():
if (this.currentAI instanceof ChargeDirectionAI
    && chargeAI.currentChargeStatus === ChargeStatus.Charging) {
    for (const enemy of otherEnemies) {
        if (this.collisionRectangle.intersects(enemy.collisionRectangle)) {
            enemy.die();
        }
    }
}
```

**Fase `Stunned` (2 secondi):**
- Nemico fermo
- Ripristina colore normale e velocità normale (`TaurusSpeed`)
- Ritorna `CharacterActions.Idle` per tutto il periodo
- Dopo 2 secondi → `enemy.setNewAI(new PatrolChargeAI(...))`

**Transizioni:**
- → `PatrolChargeAI` dopo stordimento

---

### 10. `KillCandleAI`

**Assegnata a:** tutti i nemici che cercano candele (Bubu, Skeleton, Goblin, Taurus)

**Costruttore:**
```typescript
new KillCandleAI(candle, pathToCandle, previousAI, killDurationMS)
```

**Stato interno:**
- `_candle` — la candela target
- `_actions: CharacterActions[]` — percorso precalcolato verso la candela
- `_previousAI` — AI da ripristinare dopo aver spento la candela
- `_killDurationMS` — durata dell'animazione di spegnimento

**State machine interna:**

```
WalkingToCandle
  ├─ (candela già morta) → previousAI
  ├─ (arrivato) → Smashing
  └─ (continua percorso)

Smashing
  ├─ (candela muore) → Ended
  └─ (chiama enemy.attack() ogni frame)

Ended
  └─ (immediato) → previousAI
```

**Fase `WalkingToCandle`:**
- Dequeue azione da `_actions`
- Ogni frame controlla `candle.isAlive`:
  - Se `false` → `enemy.setNewAI(this._previousAI)` (qualcuno l'ha spenta prima)
- Se coda vuota (arrivato) → `Smashing`

**Fase `Smashing`:**
- Chiama `enemy.attack()` — anima l'attacco senza danni al giocatore
- Aspetta che `candle.isAlive == false`
- Chiama `enemy.stopAttacking()`
- → `Ended`

**Fase `Ended`:**
- Ripristina `_previousAI` con `enemy.setNewAI(this._previousAI)`

---

## Nemici — dettaglio completo

### Zombie

**File TS:** `Web/src/characters/enemies/Zombie.ts`
**File C#:** `Csharp/MarkInDeDark.Library/Characters/Zombie.cs`

**AI iniziale:** `PatrolAI(waypoints, grid, candlesManager, player, searchesCandles: false)`

**Configurazione:**
| Parametro | Valore |
|-----------|--------|
| Velocità | `ZombieSpeed` (da `gameConfig`) |
| Ricerca candele | No |
| Spegne candele su collisione | Sì |
| Range candele | N/A |
| `canGoOverWalls` | No |
| Luce propria | Sì (colori `ZombieLightColorRGB1/2`) |

**Comportamento:**
- Patruglia i waypoint definiti nel livello in loop infinito
- Se collide con una candela la spegne immediatamente (collisione fisica, non `KillCandleAI`)
- Se collide con il giocatore lo uccide
- Non ha nessun comportamento avanzato, nessuno stato speciale
- Il più semplice dei nemici

**Nota per il level design:**
Minaccia ambientale prevedibile: patruglia su percorso fisso e spegne le candele al passaggio senza mai deviare. Usalo per creare corridoi dove il giocatore deve sincronizzare i movimenti o rischiare di restare al buio. Non reagisce al giocatore finché non lo tocca fisicamente: è più un "pericolo da incrociare" che un cacciatore attivo.

**Transizioni AI:**
```
PatrolAI → IdleAI  (dopo aver ucciso il giocatore)
```

---

### Ghost

**File TS:** `Web/src/characters/enemies/GhostEnemy.ts`
**File C#:** `Csharp/MarkInDeDark.Library/Characters/Ghost.cs`

**AI iniziale:** `PatrolGhostAI(waypoints, grid, player)`

**Configurazione:**
| Parametro | Valore |
|-----------|--------|
| Velocità | `GhostSpeed` |
| Ricerca candele | No |
| Spegne candele su collisione | Sì (250 ms) |
| Range candele | N/A |
| `canGoOverWalls` | **Sì** — attraversa i muri fisicamente |
| Luce propria | Sì (verde chiaro `0x9CEEC0` + rosa `0xEFC0E3`) |

**Override di `die()`:**
```typescript
override die(): void {
    // no-op — Ghost non può morire
}
```

**Override di `customConditionToAttackPlayer()`:**
```typescript
protected override customConditionToAttackPlayer(): boolean {
    return this._player.isMoving;  // attacca SOLO se il giocatore si muove
}
```

**Override di `killCandleLogic()`:**
```typescript
protected override killCandleLogic(candle: Candle): void {
    candle.die(250);  // spegne in 250 ms (il più veloce di tutti)
}
```

**Comportamento:**
- Patruglia ignorando completamente i muri (il pathfinding non considera ostacoli)
- Spegne le candele estremamente in fretta (250 ms)
- Attacca il giocatore **solo se il giocatore è in movimento** — stare fermi è l'unico modo per sopravvivere al Ghost
- Non può essere ucciso (invulnerabile)

**Nota per il level design:**
Attraversa i muri e non può essere eliminato. Il giocatore sopravvive solo stando **completamente fermo** — qualsiasi movimento è letale. Ideale per stanze senza uscite sicure dove forzare pause tattiche, o per "inseguire" il giocatore attraverso pareti creando tensione psicologica. Abbinalo ad altri nemici che premono il giocatore a muoversi per creare un dilemma.

**Transizioni AI:**
```
PatrolGhostAI → IdleAI  (dopo aver ucciso il giocatore)
```

---

### Skeleton (Scheletron)

**File TS:** `Web/src/characters/enemies/Skeleton.ts`
**File C#:** `Csharp/MarkInDeDark.Library/Characters/Skeletron.cs`

**AI iniziale:** `PatrolHunterLineOfSightAI(waypoints, grid, candlesManager, player)`

**Configurazione:**
| Parametro | Valore |
|-----------|--------|
| Velocità pattuglia | `SkeletronSpeed` (0.6 s/cella) |
| Velocità caccia | `SkeletronHuntingSpeed` (0.5 s/cella) |
| Ricerca candele | Sì |
| Range candele | `SkeletronMaxCellsForNearCandle` (3 celle) |
| `canGoOverWalls` | No |
| Luce propria | Sì (colori `SkeletronLightColorRGB1/2`) |
| Colore rabbia | `SkeletronRageColorRGB` (rosso) |

**Comportamento:**

Patruglia normalmente. Ogni frame controlla la LoS: se il giocatore è allineato orizzontalmente/verticalmente, non ha muri in mezzo e lo Skeleton non gli dà le spalle → scatta la caccia.

Durante la caccia:
- Il corpo diventa **rosso**
- La velocità aumenta a `SkeletronHuntingSpeed`
- Ricalcola il percorso ad ogni cambio cella del giocatore
- Se perde la visuale → fase di confusione (3 secondi di guardarsi intorno) → torna a pattugliare

Se durante la caccia vede una candela entro range → la va a spegnere, poi riprende la caccia.

**Nota per il level design:**
Guardia classica con LoS: pattuglia finché non vede il giocatore in linea retta davanti a sé, poi accelera e insegue. Se perde la visuale si ferma 3 secondi a guardarsi intorno, poi torna al percorso. Si distrae anche con le candele. Posizionalo in corridoi lunghi e rettilinei per sfruttare al massimo la LoS; funziona bene come guardiano di un'area chiave perché ci torna sempre.

**Transizioni AI:**
```
PatrolHunterLineOfSightAI
    ↓ (LoS attiva)
PlayerHunterLineOfSightAI [Following]
    ↓ (perde LoS)
PlayerHunterLineOfSightAI [Incredulity, 3 s]
    ↓ (timer)
PlayerHunterLineOfSightAI [Lost]
    ↓ (immediato)
PatrolHunterLineOfSightAI

PatrolHunterLineOfSightAI / PlayerHunterLineOfSightAI
    → KillCandleAI (candela vicina)
    → IdleAI (giocatore ucciso)
```

---

### Goblin

**File TS:** `Web/src/characters/enemies/Goblin.ts`
**File C#:** `Csharp/MarkInDeDark.Library/Characters/Goblin.cs`

**AI iniziale:** `PlayerHunterAI(player, grid, candlesManager)`

**Configurazione:**
| Parametro | Valore |
|-----------|--------|
| Velocità base | `GoblinSpeed` (0.8 s/cella) |
| Incremento per candela | `GoblinSpeedIncrementForEachCandleTaken` (−0.1 s/cella per ogni candela usata) |
| Velocità minima | 0.2 s/cella (5 celle/sec) |
| Ricerca candele | Sì |
| Range candele | `GoblinMaxCellsForNearCandle` (3 celle) |
| `canGoOverWalls` | No |
| Luce propria | Sì (colori `GoblinLightColorRGB1/2`) |

**Logica di escalation velocità (in `Goblin.update()`):**
```typescript
const currentUsed = this._candlesManager.playerUsedCandles;
if (currentUsed > this._lastUsedCandleCount) {
    const increments = currentUsed - this._lastUsedCandleCount;
    this._lastUsedCandleCount = currentUsed;
    const newSpeed = Math.max(
        MIN_SPEED_S,
        this.movementSpeedMS / 1000 - SPEED_DECREMENT * increments
    );
    this.movementSpeedMS = newSpeed * 1000;
}
```

**Escalation esempio:**
| Candele usate | s/cella | celle/sec |
|--------------|---------|-----------|
| 0 | 0.8 | 1.25 |
| 1 | 0.7 | 1.43 |
| 3 | 0.5 | 2.00 |
| 6 | 0.2 | 5.00 (max) |

**Comportamento:**
- Insegue **sempre e subito** il giocatore dall'inizio del livello (nessuna pattuglia)
- Ogni volta che il giocatore piazza/usa una candela, diventa più veloce
- Cerca anche le candele, ma la priorità è inseguire il giocatore
- Il più aggressivo nella progressione di difficoltà dinamica

**Nota per il level design:**
Insegue il giocatore dal primo secondo e diventa più veloce ogni volta che il giocatore usa una candela. Usalo per creare pressione costante e disincentivare l'uso eccessivo delle candele: più il giocatore illumina, più il Goblin diventa pericoloso. Funziona meglio in livelli dove le candele sono una risorsa limitata e preziosa.

**Transizioni AI:**
```
PlayerHunterAI → KillCandleAI (candela vicina)
PlayerHunterAI → IdleAI (giocatore ucciso)
```

---

### Taurus

**File TS:** `Web/src/characters/enemies/Taurus.ts`
**File C#:** `Csharp/MarkInDeDark.Library/Characters/Taurus.cs`

**AI iniziale:** `PatrolChargeAI(waypoints, grid, candlesManager, player, onShake)`

**Configurazione:**
| Parametro | Valore |
|-----------|--------|
| Velocità pattuglia | `TaurusSpeed` (1.5 s/cella) |
| Velocità carica | `TaurusChargeSpeed` (0.1 s/cella) |
| Durata wind-up | `TaurusTimeToStartSeconds` (1 s) |
| Durata stordimento | `TaurusTimeStunnedSeconds` (2 s) |
| Ricerca candele | Sì (la carica può anche puntare a una candela) |
| Range candele LoS | `TaurusMaxCellsForNearCandle` (3 celle) |
| Colore carica | `TaurusChargeColorRGB` (blu `0x0044FF`) |
| `canGoOverWalls` | No (li sfonda durante la carica) |
| Luce propria | Sì (colori `TaurusLightColorRGB1/2`) |

**Override di `die()`:**
```typescript
override die(): void {
    // no-op — Taurus non può morire
}
```

**Animazioni speciali (solo Taurus):**
- `TaurusChargeFront`, `TaurusChargeBack`, `TaurusChargeSide` — wind-up
- `TaurusRunFront`, `TaurusRunBack`, `TaurusRunSide` — durante la carica

**Comportamento:**
- Patruglia **lentamente** (1.5 s/cella è il più lento)
- Quando vede il giocatore O una candela in linea retta → wind-up 1 secondo (corpo blu), poi carica
- Durante la carica: velocità massima, sfonda muri, spegne candele, camera shake, può uccidere altri nemici
- Dopo aver colpito un muro → stordito 2 secondi → riprende la pattuglia
- **Uccide altri nemici** se li colpisce durante la carica

**Nota per il level design:**
Lento in pattuglia ma devastante quando carica: sfonda muri, spegne candele, uccide altri nemici. Posizionalo in corridoi rettilinei dove la carica è efficace e la distruzione dei muri può aprire nuove vie o creare caos. Nelle stanze aperte il giocatore può aggirarlo facilmente; nei corridoi stretti diventa imprendibile. Evita stanze piccole: il wind-up da 1 secondo è troppo facile da anticipare.

**Transizioni AI:**
```
PatrolChargeAI
    ↓ (LoS attiva verso giocatore o candela)
ChargeDirectionAI [Starting, 1 s]
    ↓ (timer)
ChargeDirectionAI [Charging]
    ↓ (impatta muro)
ChargeDirectionAI [Stunned, 2 s]
    ↓ (timer)
PatrolChargeAI
```

---

### Bubu

**File TS:** `Web/src/characters/enemies/Bubu.ts`
**File C#:** `Csharp/MarkInDeDark.Library/Characters/Bubu.cs`

**AI iniziale:** `PatrolAI(waypoints, grid, candlesManager, player, searchesCandles: true)`

**Configurazione:**
| Parametro | Valore |
|-----------|--------|
| Velocità | `BubuSpeed` (0.6 s/cella) |
| Ricerca candele | **Sì** — obiettivo primario |
| Range candele | `BubuMaxCellsForNearCandle` (3 celle) |
| Durata spegnimento | 500 ms |
| `canGoOverWalls` | No |
| Luce propria | **Nessuna** — si muove nel buio |

**Override di `killCandleLogic()`:**
```typescript
protected override killCandleLogic(candle: Candle): void {
    this.setNewAI(new KillCandleAI(candle, [], this.currentAI, 500));
}
```

**Comportamento:**
- Patruglia normalmente
- Non appena rileva una candela entro 3 celle → interrompe la pattuglia e raggiunge la candela
- Spegne la candela in 500 ms
- Torna a pattugliare dal punto di interruzione
- **Non emette luce propria** — è quasi invisibile nel buio del gioco, rendendolo particolarmente pericoloso per le candele
- Non attacca il giocatore in modo diverso dagli altri (collisione standard)

**Nota per il level design:**
Non emette luce propria: è quasi invisibile nel buio. Patruglia silenziosamente e devia automaticamente verso qualsiasi candela entro 3 celle. È la minaccia silenziosa per le sorgenti di luce: posizionalo vicino a candele che il giocatore vuole preservare per forzarlo a proteggere attivamente la propria visibilità. Non cambia comportamento verso il giocatore (solo collisione standard), ma la sua invisibilità nel buio lo rende insidioso.

**Transizioni AI:**
```
PatrolAI → KillCandleAI (candela vicina) → PatrolAI
PatrolAI → IdleAI (giocatore ucciso)
```

---

## Durate spegnimento candele per nemico

| Nemico | Durata `candle.die()` | Note |
|--------|----------------------|------|
| Ghost | 250 ms | Il più rapido — animazione quasi istantanea |
| Bubu | 500 ms | Veloce |
| Taurus | 500 ms | Durante la carica |
| Skeleton | valore `killCandleDurationMS` da config | Tramite `KillCandleAI` |
| Goblin | valore `killCandleDurationMS` da config | Tramite `KillCandleAI` |
| Zombie | collisione diretta — spegne immediatamente | Non usa `KillCandleAI` |

---

## Parametri GameConfig per AI (C# e TypeScript)

Tutti i parametri vengono letti da `gameConfig.json` / `GameConfig.cs`:

```
ZombieSpeed
ZombieLightColorRGB1 / RGB2

GhostSpeed
GhostLightColorRGB1 / RGB2

SkeletronSpeed
SkeletronHuntingSpeed
SkeletronMaxCellsForNearCandle
SkeletronLightColorRGB1 / RGB2
SkeletronRageColorRGB

GoblinSpeed
GoblinSpeedIncrementForEachCandleTaken
GoblinMaxCellsForNearCandle
GoblinLightColorRGB1 / RGB2

TaurusSpeed
TaurusChargeSpeed
TaurusTimeToStartSeconds
TaurusTimeStunnedSeconds
TaurusMaxCellsForNearCandle
TaurusLightColorRGB1 / RGB2
TaurusChargeColorRGB

BubuSpeed
BubuMaxCellsForNearCandle
```

---

## Diagramma riassuntivo — AI per nemico

```
Zombie      PatrolAI(searchCandles:false)
              └─ nessuna transizione avanzata

Ghost       PatrolGhostAI (attraversa muri)
              └─ customCondition: attacca solo se player.isMoving
              └─ die() = no-op

Skeleton    PatrolHunterLineOfSightAI
              ↓ LoS attiva
            PlayerHunterLineOfSightAI [Following → Incredulity → Lost]
              └─ tornato a PatrolHunterLineOfSightAI

Goblin      PlayerHunterAI (insegue sempre)
              └─ velocità aumenta con ogni candela del giocatore

Taurus      PatrolChargeAI
              ↓ LoS attiva (giocatore o candela)
            ChargeDirectionAI [Starting → Charging → Stunned]
              └─ sfonda muri, uccide altri nemici, camera shake
              └─ die() = no-op

Bubu        PatrolAI(searchCandles:true)
              ↓ candela entro 3 celle
            KillCandleAI → PatrolAI
              └─ nessuna luce propria
```

---

## Differenze C# → TypeScript

| Aspetto | C# | TypeScript |
|---------|----|------------|
| Tempo | `TimeSpan elapsed` | `deltaMS: number` (millisecondi) |
| Cambio AI | `character.CurrentAI = new XxxAI()` | `enemy.setNewAI(new XxxAI())` |
| LoS | metodo su `PatrolHunterLineOfSightAI` | funzione standalone `checkLineOfSight()` in `LineOfSight.ts` |
| Velocità Goblin | decremento diretto di `CellMovementSeconds` | calcolo in ms con `Math.max` per il clamp |
| Fallback PatrolAI | ritorna `RandomMovementAI` direttamente | `enemy.setNewAI(new RandomMovementAI(this))` e ritorna |
| Camera shake | `_game.StartCameraShake(intensity, duration)` | callback `onShake?(intensity)` |
| Animazioni speciali Taurus | `DontPlayDefaultAnimations = true` | `playAnimationIfExists()` su nomi stringa |
| Durata spegnimento candele | hardcoded in `EnemyConfiguration` | parametro `killCandleDurationMS` in `EnemyConfig` |

Tutti gli **algoritmi AI sono semanticamente identici** tra le due versioni: stesse state machine, stesse condizioni di transizione, stessa logica di pathfinding.
