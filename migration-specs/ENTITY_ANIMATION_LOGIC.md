# ENTITY_ANIMATION_LOGIC.md — Analisi Fase 3

Prerequisito ai Task 6 (animazione morte) e Task 7 (animazione attacco nemici).
Confronto tra il comportamento C# originale e l'implementazione TypeScript attuale.

---

## 1. State Machine — Caratteri (Player e Enemy)

### 1.1 Stati

```
GoingUp / GoingDown / GoingLeft / GoingRight  — movimento
IdleUp  / IdleDown  / IdleLeft  / IdleRight   — in attesa input
Attacking                                      — animazione attacco in corso
Dying                                          — animazione morte in corso (no input)
Dead                                           — stato terminale (nulla si aggiorna)
```

### 1.2 Transizioni

| Da → A | Condizione |
|--------|-----------|
| Any → **Dying** | Collision rectangle con nemico / trappola |
| Any → **Attacking** | Nemico raggiunge player (collision) |
| **Dying** → **Dead** | Animazione morte completata (in C#: fine animazione o fine explosion-lifetime) |
| **Attacking** → **Idle** | Animazione attacco completata (loop=false) |

### 1.3 Bug critico TS — Task 1 (collegato)

In TS, `die()` imposta lo stato a `Dying`. La transizione `Dying → Dead` avviene solo
nella logica di caduta in trappola (fade opacità → 0 → Dead). Per la morte da nemico,
**lo stato rimane `Dying` per sempre.**

`gamebootstrap.ts` controlla `player.isDead` per triggerare la navigazione:
```typescript
if (player.isDead) { ... navigateToGameOver ... }
```
→ **`isDead` non diventa mai `true` per morte da nemico → Task 1 non funziona.**

**Fix trasversale:** il completamento dell'animazione di morte deve chiamare
`this._state = CharacterState.Dead` (o un callback `onDeathAnimationComplete`).
Risolvere Task 6 correttamente risolve automaticamente anche Task 1.

---

## 2. Death Sequence — Passo per passo

### 2.1 C# (comportamento di riferimento)

```
t = 0ms
  - Collision rectangle enemy ↔ player
  - enemy.customActionOnPlayerDead()      ← hook per Taurus (vedi §3)
  - enemy.attack(player)                  ← avvia animazione attacco sul nemico
  - player.Die()                          ← Character.Die() base
      → _gameGrid.ApplyBloodDecal(WorldCenter)
      → CurrentCharacterState = Dying
      → se ShouldDoExplosionAnimation:
            DieAnimationType = Explosion
            CharacterExplosionParticleSystem.Explode(WorldCenter, ...)
        altrimenti:
            DieAnimationType = StandardAnimation
            AnimationsManager.PlayAnimation("Dying")
  - HUD.ShowMessage("Game Over", color, isImportant: true)
  - Controller.Shake(intensity: 6, duration: 2s)
  - gameState = GameStates.Finishing

t = 0ms → 8000ms (parallelo)
  - Body part particles aggiornano posizione/rotazione (se Explosion)
  - Ogni particella emette sotto-particelle di sangue (BodyPartBloodParticleSystem)

t ≈ 1000ms
  - _gameOverFadeObject = FadeObject(duration: 1.5s, Color.White)
  - _gameOverFadeObject.FadeOut()          ← alpha 1.0 → 0.0 (lineare)

t = 1000ms → 2500ms  (FadeObject in corso)
  - Ogni frame si compongono DUE render target:
      RT_background = scena completa, disegnata con alpha = fadeObject.alpha (1→0)
      RT_foreground = sfondo nero + SOLO player disegnato sopra
      Composizione finale: draw(RT_background) poi draw(RT_foreground)
  - Effetto: scena svanisce al nero, player rimane visibile sopra

t = 2500ms
  - Scena completamente nera, solo player visibile

t = 5000ms
  - gameFinishTimerIfDead scade → navigazione al menu
```

### 2.2 TS (stato attuale — cosa manca)

```
t = 0ms
  - Collision → player.die() → state = Dying
  - gameEnded = true (blocca logica di gioco, mantiene rendering)
  (nessuna animazione di morte, nessuna esplosione, nessun blood decal)

t = 0ms → infinito
  - player rimane in stato Dying, non transita mai a Dead
  - player.isDead === false → gamebootstrap non triggered

t = ?? (mai)
  - deathOverlay?.classList.add('active')  ← non viene mai eseguito
  - setTimeout(navigateToGameOver, 1500)   ← non viene mai eseguito
```

**Riepilogo mancanze per Task 6:**

| Elemento | C# | TS | Status |
|----------|----|----|--------|
| Animazione morte player | `player_front_dying_1..26` (26 frame) | Non registrata in Player.ts | ❌ |
| Transizione Dying→Dead | Fine animazione | Non implementata | ❌ |
| Esplosione body parts | `CharacterExplosionParticleSystem` | Non esiste | ❌ |
| Effetto schermo nero | Due RT + FadeObject | CSS overlay (parziale) | ⚠️ |
| Blood decal sul terreno | `ApplyBloodDecal` | Non implementato | ❌ |
| Camera shake | `Controller.Shake(6, 2s)` | `camera.startShake()` già disponibile | ⚠️ |

---

## 3. Corpse Explosion — Specifiche Particelle

### 3.1 Quando viene attivata

**Solo quando Taurus uccide il player:**
```csharp
// Taurus.cs
public override void CustomActionOnPlayerDead() {
    _player.ShouldDoExplosionAnimation = true;
}
// Character.Die()
if (ShouldDoExplosionAnimation)
    → DieAnimationType.Explosion → Explode()
else
    → DieAnimationType.StandardAnimation → PlayAnimation("Dying")
```

Per tutti gli altri nemici (Zombie, Skeleton, Goblin, Ghost, Bubu):
→ animazione standard `player_front_dying_1..26`.

### 3.2 Sprite body parts del player (7 pezzi)

| Sprite | Frame name asset | Note |
|--------|-----------------|------|
| Braccio sinistro | `Player/Death/player_armL` | |
| Braccio destro | `Player/Death/player_armR` | |
| Testa | `Player/Death/player_head` | |
| Torso | `Player/Death/player_torso` | |
| Gamba sinistra | `Player/Death/player_legL` | |
| Gamba destra | `Player/Death/player_legR` | |
| Intestini | `Player/Death/player_guts` | |

Tutti presenti negli spritesheet. ✅

### 3.3 Matematica delle particelle (da `CharacterExplosionParticleSystem.cs`)

```
Per ogni body part:
  angle       = random(0°, 360°)  → radians
  direction   = Vector2(cos(angle), sin(angle))
  speed       = random(80, 140)   [px/s]
  velocity    = direction * speed
  rotSpeed    = random(0.0, 1.0) * 5.0  [rad/s]
  acceleration = (0, 0)           ← NO gravità
  lifetime    = 8000ms            [player config]
  withFading  = false             ← scompaiono di scatto (no fade-out)

Update per frame (dt in secondi):
  velocity += acceleration * dt   ← costante (noop)
  position += velocity * dt
  rotation += rotSpeed * dt
  bloodEmitter.setPosition(position)
  bloodEmitter.update(dt)
```

### 3.4 Sotto-particelle di sangue (BodyPartBloodParticleSystem)

Ogni body part ha un emettitore di piccole sprite di sangue attaccato:
- Emette continuamente mentre il body part è in movimento
- Sprite: piccoli splat/gocce (asset `Effects/blood` o simile — verificare negli spritesheet)
- In PixiJS: implementabile come `ParticleContainer` con spawn di sprite a ogni frame
  oppure come semplice sequenza di sprite statiche sul `groundContainer`

### 3.5 Blood decal

`_gameGrid.ApplyBloodDecal(WorldCenter)` applica una texture di sangue statica
sul pavimento nella cella del player. In TS: aggiungere sprite sul `groundContainer`
con z-index `Z_GROUND_DIRTY` (già definito in `ZIndexCalculator.ts`).
Asset da verificare: cercare `blood`, `bloodDecal`, `decal` negli spritesheet.

---

## 4. Enemy Attack — Frame Timing e Bug TS

### 4.1 Modello danni (C#)

**Non esiste frame-perfect timing.** Il danno è istantaneo al contatto:
```
Collision rettangolo nemico ↔ player
→ enemy.attack(player)     ← avvia animazione direzionale
→ player.Die()             ← danno contestuale alla collision
→ enemy.currentAI = IdleAI ← nemico si ferma
```
Il frame esatto in cui l'animazione di attacco inizia non determina il danno —
il danno è già avvenuto prima che il primo frame sia mostrato.

### 4.2 Struttura animazioni attacco

Ogni nemico ha 3 varianti direzionali (determinate dalla posizione relativa del player):

| Nemico | AttackDown (front) | AttackUp (back) | AttackSide | Loop C# |
|--------|-------------------|-----------------|------------|---------|
| Zombie | 4 frame | 4 frame | 4 frame | No |
| Skeleton | 4 frame | 4 frame | 4 frame | No |
| Goblin | 5 frame | 5 frame | 5 frame | No |
| Taurus | 2 frame | 2 frame | 2 frame | No |
| Bubu | 5 frame | 4 frame | 5 frame | No |
| Ghost | 8 frame | 8 frame | 8 frame | No |

Tutti gli asset sono presenti negli spritesheet. ✅

### 4.3 Bug TS (Task 7)

**Bug 1 — Animazioni attacco in loop infinito:**
```typescript
// enemyAnimations.ts — ERRATO
mgr.add('AttackDown', buildAnim(sheets, `${type}_front_attack`, frames, /* loop= */ true));
```
→ L'animazione non termina mai, il nemico rimane visivamente in loop di attacco.

**Bug 2 — `stopAttacking()` mai chiamato:**
```typescript
// Enemy.ts — il metodo esiste ma nessuno lo invoca
stopAttacking(): void {
    if (this._state === CharacterState.Attacking) {
        this._state = CharacterState.IdleDown;
        this._animations.play('IdleDown');
    }
}
```
→ Dopo aver ucciso il player, il nemico resta nello stato `Attacking` per sempre.
   L'AI (già impostata a `IdleAI`) non viene mai aggiornata perché il blocco:
   ```typescript
   if (!this.isMoving && !this.isDying) { const action = this.currentAI.update(...) }
   ```
   esegue solo se `isMoving` o `isDying`, non se `Attacking`.

**Fix Task 7:**
1. `enemyAnimations.ts`: `loop = false` per `AttackDown/Up/Side`
2. `Enemy.ts` in `update()`: aggiungere blocco:
   ```typescript
   if (this._state === CharacterState.Attacking) {
       if (this._animations.isCurrentFinished) this.stopAttacking();
       return;
   }
   ```

---

## 5. Visual FX — Effetto Schermo Nero Tranne il Player

### 5.1 Approccio C# (due render target)

```
RT_background = scena completa → alpha decresce linearmente 1.0→0.0 in 1.5s
RT_foreground = sfondo Color.Black + solo player disegnato sopra (alpha 1.0)
Composizione:
  draw(RT_background, Color.White * fadeAlpha)
  draw(RT_foreground, Color.White)
```
Risultato: la scena scompare nel nero mentre il player rimane visibile sopra.

### 5.2 Opzioni per TS / PixiJS

**Opzione A — CSS overlay (attuale, parziale):**
- `deathOverlay.classList.add('active')` → fade-in CSS a nero
- **Limite:** copre anche il player, non è possibile tenerlo visibile sopra

**Opzione B — PixiJS sprite overlay (consigliata per Task 6):**
```
1. Creare un Graphics rect black (GAME_W × GAME_H), alpha = 0, zIndex = Z_OVERALL + 0.1
   → si trova sopra tutto
2. Spostare lo sprite del player (e la sua normal map) a zIndex = Z_OVERALL + 0.2
   → player visibile sopra l'overlay
3. Al momento della morte: lerp alpha dell'overlay da 0 a 1 in ~1500ms
```
Differenza rispetto al C#: la luce e le ombre del player vengono oscurate insieme alla
scena (non c'è RT separato), ma l'effetto visivo è sufficientemente vicino.

**Opzione C — PixiJS RenderTexture (fedele al C#):**
- Richiede due `RenderTexture` PixiJS e compositing manuale
- Complessità alta, non necessaria per Task 6 — considerare in futuro

**Raccomandazione:** Opzione B. Rimuovere il `deathOverlay` CSS (o tenerlo come fallback
per il caso in cui PixiJS non sia inizializzato) e gestire l'effetto tutto in PixiJS.

---

## 6. Asset Completeness

### 6.1 Player

| Asset | Frame | Status |
|-------|-------|--------|
| Animazione morte | `Player/Death/player_front_dying_1..26` | ✅ |
| Body parts | `player_armL/R, head, torso, legL/R, guts` | ✅ |
| Normal map morte | `player_front_dying_*_normal` | ✅ |
| Blood decal | cerca `blood`/`decal` negli spritesheet | ❓ verificare |

### 6.2 Nemici

| Nemico | Body parts | Animazioni attacco | Status |
|--------|-----------|-------------------|--------|
| Zombie | ✅ | ✅ (4f×3dir) | OK |
| Skeleton | ✅ | ✅ (4f×3dir) | OK |
| Goblin | ✅ | ✅ (5f×3dir) | OK |
| Taurus | ✅ | ✅ (2f×3dir) | OK |
| Bubu | ✅ | ✅ (4-5f×3dir) | OK |
| Ghost | ✅ | ✅ (8f×3dir) | OK |

---

## 7. Piano di Fix

### Task 6 — Animazione morte + Corpse Explosion

| File | Modifica | Righe stimate |
|------|----------|---------------|
| `characters/Player.ts` | Aggiungere animazione `'Death'` (26f, loop=false) | ~5 |
| `characters/Character.ts` | In `die()`: chiamare `playAnimationIfExists('Death')`; aggiungere callback `onDeathComplete` → `state=Dead` | ~15 |
| `characters/Character.ts` | Se `ShouldDoExplosionAnimation`: spawn `CorpseExplosion`, skip animazione Death | ~10 |
| `effects/CorpseExplosion.ts` | **Nuova classe**: spawna 7 sprite body part con velocità random 80-140px/s, rotazione 0-5rad/s, lifetime 8s | ~80 |
| `gamebootstrap.ts` | Rimuovere overlay CSS; aggiungere Graphics rect PixiJS per effetto nero; spostare player zIndex > Z_OVERALL | ~20 |
| `characters/enemies/Taurus.ts` | Override `customActionOnPlayerDead` → `player.shouldDoExplosionAnimation = true` | ~5 |

### Task 7 — Animazione attacco nemici

| File | Modifica | Righe stimate |
|------|----------|---------------|
| `characters/enemies/enemyAnimations.ts` | `loop = false` per `AttackDown/Up/Side` | ~3 |
| `characters/Enemy.ts` | In `update()`: se `Attacking` e animazione finita → `stopAttacking()` | ~8 |

### Ordine consigliato

1. Task 7 (2 file, ~11 righe) — indipendente, semplice da verificare visivamente
2. Task 6 animazione standard (Character.ts + Player.ts) — risolve automaticamente Task 1
3. Task 6 visual FX (overlay PixiJS) — separabile dalla logica
4. Task 6 CorpseExplosion (Taurus only) — ultimo, richiede la nuova classe
