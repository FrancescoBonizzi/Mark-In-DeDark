# ARCHITECTURE_MAP.md — MonoGame → PixiJS Class Mapping

## Legend

| Status | Meaning |
|--------|---------|
| Copy | Take verbatim from `progetto-riferimento`, adapt names/paths |
| Adapt | Take from reference as base, significant changes needed |
| Port | Translate the C# logic to TypeScript (no reference equivalent) |
| Rewrite | New implementation in web idioms (no direct translation) |
| Skip | Not needed on web |

---

## Lifecycle Comparison

| MonoGame | PixiJS 8 |
|----------|----------|
| `Game.Initialize()` | `app.init({ ... })` (async) |
| `Game.LoadContent()` | `await loadAssets()` |
| `Game.Update(GameTime)` | `app.ticker.add((time: Ticker) => game.update(time))` |
| `Game.Draw(GameTime)` | Automatic — PixiJS renders every tick |
| `GameTime.ElapsedGameTime` | `time.deltaMS` (milliseconds) or `time.elapsedMS` |
| `GameTime.TotalGameTime` | Accumulate manually |
| `Content.Load<Texture2D>()` | `await Assets.load(url)` |
| `SpriteBatch.Begin/End` | Implicit — PixiJS auto-batches |
| `GraphicsDevice.SetRenderTarget()` | `app.renderer.render({ target: renderTexture })` |

---

## Render Pipeline Comparison

| MonoGame | PixiJS 8 |
|----------|----------|
| `SpriteBatch.Draw(texture, pos, …)` | `sprite.position.set(x, y)` + scene graph |
| `RenderTarget2D` | `PIXI.RenderTexture.create({ width, height })` |
| `BlendState.Multiply` | `container.blendMode = 'multiply'` |
| `Effect` (HLSL shader) | `PIXI.Filter` with WGSL/GLSL dual source |
| `DepthStencilState` | `zIndex` on containers, `sortableChildren = true` |
| `SamplerState.LinearClamp` | Default in PixiJS |
| `Matrix.CreateTranslation/Scale` | `Container.position`, `Container.scale` |
| `SpriteSortMode.BackToFront` | `Container.sortableChildren = true` + `zIndex` |

---

## Class Mapping Table

### Framework / Library Layer

| C# Class | TS File | Status | Notes |
|----------|---------|--------|-------|
| `FbonizziMonoGame/Drawing/Camera2D.cs` | `world/Camera.ts` | Copy | Bottom-up coordinate system |
| `FbonizziMonoGame/Input/KeyboardListener.cs` | `interaction/Controller.ts` | Copy | Unifies keyboard + pointer |
| `FbonizziMonoGame/Input/MouseListener.cs` | _(merged into Controller)_ | Skip | |
| `FbonizziMonoGame/Input/TouchListener.cs` | _(merged into Controller)_ | Skip | |
| `FbonizziMonoGame/Extensions/Numbers.cs` | `services/Numbers.ts` | Copy | Math utilities |
| `FbonizziMonoGame/Particles/ParticleGenerator.cs` | `particleEmitters/ParticleSystem.ts` | Copy | Pool pattern |
| `FbonizziMonoGame/Particles/Particle.cs` | `particleEmitters/Particle.ts` | Copy | |
| `FbonizziMonoGame/Sprites/AnimationsManager.cs` | _(use PIXI.AnimatedSprite)_ | Skip | |
| `FbonizziMonoGame/Sprites/SpriteAnimation.cs` | _(use PIXI.AnimatedSprite)_ | Skip | |
| `FbonizziMonoGame/PlatformAbstractions/ISettingsRepository.cs` | `services/ScoreRepository.ts` | Copy+Adapt | localStorage |

### Common / Config

| C# Class | TS File | Status | Notes |
|----------|---------|--------|-------|
| `MarkInDeDark.Common/GameConfig.cs` | `services/GameConfig.ts` | Port | JSON-loaded config |
| `MarkInDeDark.Common/NormalMapParameters.cs` | `lighting/NormalMapParameters.ts` | Port | Uniforms for normal map filter |
| `MarkInDeDark.Common/TextFileStringsManager.cs` | `services/StringsManager.ts` | Port | Localization; use JSON |

### Grid System

| C# Class | TS File | Status | Notes |
|----------|---------|--------|-------|
| `GameGridCommon/Cell.cs` | `grid/GameCell.ts` | Port | Exact property set |
| `GameGridCommon/GameGridBundle.cs` | `grid/GameGridBundle.ts` | Port | 2D array wrapper |
| `GameGridCommon/GameGridSerializerV4.cs` | `grid/GridLoader.ts` | Port | Text deserializer (see format below) |
| `GameGridCommon/GroundTypes.cs` | `grid/GroundTypes.ts` | Port | `const enum` |
| `GameGridCommon/ObjectTypes.cs` | `grid/ObjectTypes.ts` | Port | `const enum` |
| `GameGridCommon/WallTypes.cs` | `grid/WallTypes.ts` | Port | `const enum` |
| `GameGridCommon/WallPositions.cs` | `grid/WallPositions.ts` | Port | `const enum` |
| `GameGridCommon/PillarPositions.cs` | `grid/PillarPositions.ts` | Port | |
| `Library/Levels/GameGrid.cs` | `grid/GameGrid.ts` | Port | Runtime grid (with `GameCell` links) |
| `Library/Levels/GameCell.cs` | `grid/GameCell.ts` | Port | Per-cell runtime state + neighbour refs |
| `Library/Levels/IWall.cs` | `grid/IWall.ts` | Port | Interface |
| `Library/Levels/WallFixed.cs` | `grid/WallFixed.ts` | Port | |
| `Library/Levels/WallBreakable.cs` | `grid/WallBreakable.ts` | Port | |
| `Library/Levels/WallInteractiveDoor.cs` | `grid/WallInteractiveDoor.ts` | Port | |
| `Library/Levels/WallFactory.cs` | `grid/WallFactory.ts` | Port | |
| `Library/Levels/Pillar.cs` | `grid/Pillar.ts` | Port | |
| `Library/Levels/Level.cs` | `grid/Level.ts` | Port | |
| `Library/Levels/GameGridLoader.cs` | `grid/GameGridFactory.ts` | Port | Builds runtime GameGrid from bundle |
| `Library/Levels/LevelCustomization.cs` | `grid/LevelCustomization.ts` | Port | Per-level visual params |

### Lighting / Shadow System

| C# Class | TS File | Status | Notes |
|----------|---------|--------|-------|
| `Library/Lighting/MarkLight.cs` | `lighting/Light.ts` | Rewrite | See shadow system section |
| `Penumbra.PointLight` | `lighting/PointLight.ts` | Rewrite | |
| `Penumbra.Hull` | `lighting/Hull.ts` | Rewrite | Wall-segment geometry for shadows |
| `Penumbra` (library) | `lighting/ShadowSystem.ts` | Rewrite | Full reimplementation |
| _(no equivalent)_ | `lighting/shaders/lightShader.wgsl` | Rewrite | |
| _(no equivalent)_ | `lighting/shaders/lightShader.glsl` | Rewrite | |
| _(no equivalent)_ | `lighting/shaders/shadowShader.wgsl` | Rewrite | |
| _(no equivalent)_ | `lighting/shaders/shadowShader.glsl` | Rewrite | |
| _(no equivalent)_ | `lighting/shaders/normalMapShader.wgsl` | Rewrite | |
| _(no equivalent)_ | `lighting/shaders/normalMapShader.glsl` | Rewrite | |
| `Common/NormalMapParameters.cs` | `lighting/NormalMapFilter.ts` | Rewrite | PIXI.Filter wrapper |

### Characters

| C# Class | TS File | Status | Notes |
|----------|---------|--------|-------|
| `Library/Characters/Character.cs` | `characters/Character.ts` | Port | Abstract base; grid-based movement via lerp |
| `Library/Characters/Player.cs` | `characters/Player.ts` | Port | |
| `Library/Characters/Enemy.cs` | `characters/enemies/Enemy.ts` | Port | Abstract |
| `Library/Characters/Zombie.cs` | `characters/enemies/Zombie.ts` | Port | |
| `Library/Characters/Ghost.cs` | `characters/enemies/Ghost.ts` | Port | Can walk through walls |
| `Library/Characters/Goblin.cs` | `characters/enemies/Goblin.ts` | Port | |
| `Library/Characters/Taurus.cs` | `characters/enemies/Taurus.ts` | Port | |
| `Library/Characters/Skeletron.cs` | `characters/enemies/Skeletron.ts` | Port | |
| `Library/Characters/Bubu.cs` | `characters/enemies/Bubu.ts` | Port | |
| `Library/Characters/CharacterActions.cs` | `characters/CharacterActions.ts` | Port | `const enum` |
| `Library/Characters/CharacterState` (inner) | `characters/CharacterState.ts` | Port | `const enum` |
| `Library/Characters/CharacterConfiguration.cs` | `characters/CharacterConfiguration.ts` | Port | Constructor params |
| `Library/Characters/EnemyConfiguration.cs` | `characters/EnemyConfiguration.ts` | Port | |

### AI

| C# Class | TS File | Status | Notes |
|----------|---------|--------|-------|
| `Library/AI/IEnemyAI.cs` | `ai/IEnemyAI.ts` | Port | `interface IEnemyAI { update(enemy: Enemy, deltaMS: number): CharacterActions }` |
| `Library/AI/IdleAI.cs` | `ai/IdleAI.ts` | Port | |
| `Library/AI/PatrolAI.cs` | `ai/PatrolAI.ts` | Port | |
| `Library/AI/PlayerHunterAI.cs` | `ai/PlayerHunterAI.ts` | Port | |
| `Library/AI/PatrolHunterLineOfSightAI.cs` | `ai/PatrolHunterLineOfSightAI.ts` | Port | |
| `Library/AI/PlayerHunterLineOfSightAI.cs` | `ai/PlayerHunterLineOfSightAI.ts` | Port | |
| `Library/AI/KillCandleAI.cs` | `ai/KillCandleAI.ts` | Port | |
| `Library/AI/ChargeDirectionAI.cs` | `ai/ChargeDirectionAI.ts` | Port | |
| `Library/AI/PatrolChargeAI.cs` | `ai/PatrolChargeAI.ts` | Port | |
| `Library/AI/PatrolGhostAI.cs` | `ai/PatrolGhostAI.ts` | Port | |
| `Library/AI/RandomMovementAI.cs` | `ai/RandomMovementAI.ts` | Port | |

### Objects & Traps

| C# Class | TS File | Status | Notes |
|----------|---------|--------|-------|
| `Library/Objects/Candle.cs` | `objects/Candle.ts` | Port | |
| `Library/Objects/CandlePack.cs` | _(merged into Candle)_ | Port | |
| `Library/Objects/CandlesManager.cs` | `objects/CandlesManager.ts` | Port | |
| `Library/Objects/Exit.cs` | `objects/Exit.ts` | Port | |
| `Library/Objects/BrazierLit.cs` | `objects/BrazierLit.ts` | Port | Light source object |
| `Library/Objects/InstantlyDieTrap.cs` | `objects/traps/InstantlyDieTrap.ts` | Port | |
| `Library/Objects/TimedBreakableTrap.cs` | `objects/traps/TimedBreakableTrap.ts` | Port | |
| `Library/Objects/GoblinTriggerTrap.cs` | `objects/traps/GoblinTriggerTrap.ts` | Port | |
| `Library/Objects/UnpassableObject.cs` | `objects/UnpassableObject.ts` | Port | |
| `Library/Objects/PassableObject.cs` | `objects/PassableObject.ts` | Port | |
| `Library/Objects/CollectableObject.cs` | `objects/CollectableObject.ts` | Port | |

### Particles

| C# Class | TS File | Status | Notes |
|----------|---------|--------|-------|
| `Library/Particles/CandleFireParticleSystem.cs` | `particleEmitters/CandleFireParticleSystem.ts` | Port | |
| `Library/Particles/CharacterExplosionParticleSystem.cs` | `particleEmitters/CharacterExplosionParticleSystem.ts` | Port | |
| `Library/Particles/GhostParticleSystem.cs` | `particleEmitters/GhostParticleSystem.ts` | Port | |
| `Library/Particles/EnemyEyeParticleSystem.cs` | `particleEmitters/EnemyEyeParticleSystem.ts` | Port | |
| `Library/Particles/BodyPartBloodParticleSystem.cs` | `particleEmitters/BloodParticleSystem.ts` | Port | |
| `Library/Particles/CharacterTorchParticleSystem.cs` | `particleEmitters/TorchParticleSystem.ts` | Port | |
| `Library/Particles/Snowing/SnowingManager.cs` | `particleEmitters/SnowingManager.ts` | Port | |

### HUD & Menu

| C# Class | TS File | Status | Notes |
|----------|---------|--------|-------|
| `Library/GameHUD.cs` | `hud/GameHud.ts` | Port | |
| `Library/Menu/MainMenuPage.cs` | `pages/menu.ts` | Rewrite | HTML/CSS + Navigo |
| `Library/Menu/LevelsPage.cs` | `pages/levelselect.ts` | Rewrite | |
| _(no equivalent)_ | `pages/gameover.ts` | Adapt | From reference |
| _(no equivalent)_ | `pages/gamebootstrap.ts` | Adapt | From reference (1280×720) |
| _(no equivalent)_ | `pages/router.ts` | Adapt | From reference (different routes) |

### Services

| C# Class | TS File | Status | Notes |
|----------|---------|--------|-------|
| `Library/SoundManager.cs` | `services/SoundManager.ts` | Adapt | Howler; different sound keys |
| `FbonizziMonoGame/Implementations/FileWindowsSettingsRepository.cs` | `services/ScoreRepository.ts` | Adapt | localStorage |
| `Library/Maths/Floats.cs` | `services/Numbers.ts` | Copy | |
| `Library/Maths/Utilities.cs` | _(merged into Numbers)_ | Copy | |
| `Library/Extensions/ZIndexCalculator.cs` | `services/ZIndexCalculator.ts` | Port | |
| `Library/GameOrchestrator.cs` | `Game.ts` | Port | Main game loop class |
| `Library/MarkInDeDarkBootstrap.cs` | `pages/gamebootstrap.ts` | Adapt | |

---

## Enum Tables (integer values preserved in level files)

### GroundTypes

```typescript
export const enum GroundTypes {
  NonCella = 0, Terra1 = 1, TrappolaMorteIstantanea = 3,
  TrappolaTemporizzata = 4, TriggerGoblin = 5, Terra2 = 6,
  Buio = 7, Riempimento1 = 8, Terra3 = 9, Terra4 = 10,
  Terra5 = 11, Terra6 = 12, Terra7 = 13, Terra8 = 14,
  Terra9 = 15, Terra10 = 16, Riempimento2 = 17, Riempimento3 = 18,
  Riempimento4 = 19, Riempimento5 = 20, Riempimento6 = 21,
  Riempimento7 = 22, Riempimento8 = 23, Riempimento9 = 24,
  Riempimento10 = 25, Riempimento11 = 26, Riempimento12 = 27,
  Riempimento13 = 28,
}
```

### ObjectTypes

```typescript
export const enum ObjectTypes {
  Cancella = 0, Giocatore = 1, Chiave = 2, Uscita = 3,
  Candele = 4, Zombie = 5, Scheletro = 6, Goblin = 7,
  Taurus = 8, Ghost = 10, Bubu = 11, Lucina = 12,
  OggettoNonPassabile1 = 13, OggettoNonPassabile2 = 14,
  OggettoPassabile1 = 15,
}
```

### WallTypes

```typescript
export const enum WallTypes {
  Niente = 0, Occludente = 1, Fragile = 2, Porta = 3,
  Muro1 = 4, Ingresso = 5, Uscita = 6,
  Decal1 = 7, Decal2 = 8, Decal3 = 9, Decal4 = 10, Decal5 = 11,
  Muro2 = 12, Muro3 = 13, Muro4 = 14, Muro5 = 15,
  Decal6 = 16, Decal7 = 17, Decal8 = 18,
}
```

### WallPositions

```typescript
export const enum WallPositions { Top = 0, Right = 1, Bottom = 2, Left = 3 }
```

---

## Proposed `Web/src/` Directory Structure

```
Web/src/
├── assets/
│   ├── AssetsLoader.ts           (adapt from reference)
│   └── MarkInDeDarkAssets.ts     (typed asset interface)
├── grid/
│   ├── GroundTypes.ts
│   ├── ObjectTypes.ts
│   ├── WallTypes.ts
│   ├── WallPositions.ts
│   ├── GameCell.ts               (port Cell.cs + runtime GameCell.cs)
│   ├── GameGridBundle.ts         (port GameGridBundle.cs)
│   ├── GameGrid.ts               (port GameGrid.cs — runtime with links)
│   ├── GridLoader.ts             (port GameGridSerializerV4.cs)
│   ├── GameGridFactory.ts        (port GameGridLoader.cs)
│   ├── IWall.ts
│   ├── WallFixed.ts
│   ├── WallBreakable.ts
│   ├── WallInteractiveDoor.ts
│   ├── WallFactory.ts
│   ├── Pillar.ts
│   ├── Level.ts
│   └── LevelCustomization.ts
├── characters/
│   ├── CharacterActions.ts
│   ├── CharacterState.ts
│   ├── CharacterConfiguration.ts
│   ├── Character.ts              (abstract)
│   ├── Player.ts
│   └── enemies/
│       ├── EnemyConfiguration.ts
│       ├── Enemy.ts              (abstract)
│       ├── Zombie.ts
│       ├── Ghost.ts
│       ├── Goblin.ts
│       ├── Taurus.ts
│       ├── Skeletron.ts
│       └── Bubu.ts
├── ai/
│   ├── IEnemyAI.ts
│   ├── IdleAI.ts
│   ├── PatrolAI.ts
│   ├── PlayerHunterAI.ts
│   ├── PatrolHunterLineOfSightAI.ts
│   ├── PlayerHunterLineOfSightAI.ts
│   ├── KillCandleAI.ts
│   ├── ChargeDirectionAI.ts
│   ├── PatrolChargeAI.ts
│   ├── PatrolGhostAI.ts
│   └── RandomMovementAI.ts
├── lighting/
│   ├── Light.ts                  (port MarkLight.cs)
│   ├── PointLight.ts
│   ├── Hull.ts                   (shadow occluder geometry)
│   ├── NormalMapFilter.ts        (PIXI.Filter wrapper)
│   ├── ShadowSystem.ts           (main orchestrator)
│   └── shaders/
│       ├── lightShader.wgsl
│       ├── lightShader.glsl
│       ├── shadowShader.wgsl
│       ├── shadowShader.glsl
│       ├── normalMapShader.wgsl
│       └── normalMapShader.glsl
├── objects/
│   ├── Candle.ts
│   ├── CandlesManager.ts
│   ├── Exit.ts
│   ├── BrazierLit.ts
│   ├── UnpassableObject.ts
│   ├── PassableObject.ts
│   ├── CollectableObject.ts
│   └── traps/
│       ├── InstantlyDieTrap.ts
│       ├── TimedBreakableTrap.ts
│       └── GoblinTriggerTrap.ts
├── particleEmitters/
│   ├── Particle.ts               (copy from reference)
│   ├── ParticleSystem.ts         (copy from reference)
│   ├── CandleFireParticleSystem.ts
│   ├── CharacterExplosionParticleSystem.ts
│   ├── GhostParticleSystem.ts
│   ├── EnemyEyeParticleSystem.ts
│   ├── BloodParticleSystem.ts
│   ├── TorchParticleSystem.ts
│   └── SnowingManager.ts
├── hud/
│   └── GameHud.ts
├── pages/
│   ├── router.ts
│   ├── gamebootstrap.ts          (1280×720 variant)
│   ├── menu.ts
│   ├── levelselect.ts
│   └── gameover.ts
├── interaction/
│   └── Controller.ts             (copy from reference)
├── services/
│   ├── CollisionSolver.ts        (copy from reference)
│   ├── ScoreRepository.ts        (adapt from reference)
│   ├── SoundManager.ts           (adapt from reference)
│   ├── Numbers.ts                (copy from reference)
│   ├── StringHelper.ts           (copy from reference)
│   ├── AudioUnlocker.ts          (copy from reference)
│   ├── ZIndexCalculator.ts       (port)
│   └── GameConfig.ts             (port GameConfig.cs)
├── world/
│   └── Camera.ts                 (copy from reference)
├── primitives/
│   └── Interval.ts               (copy from reference)
├── IHasCollisionRectangle.ts     (copy from reference)
├── Game.ts                       (main loop)
└── main.ts                       (entry point)
```

---

## Dependency Graph (simplified)

```
main.ts
  └── router.ts
        ├── menu.ts
        ├── levelselect.ts
        ├── gameover.ts
        └── gamebootstrap.ts
              └── Game.ts
                    ├── Camera.ts ← world/
                    ├── Controller.ts ← interaction/
                    ├── SoundManager.ts ← services/
                    ├── GameGrid.ts ← grid/
                    │     ├── GridLoader.ts
                    │     ├── GameCell.ts
                    │     └── WallFactory.ts
                    ├── ShadowSystem.ts ← lighting/
                    │     ├── Light.ts
                    │     ├── Hull.ts
                    │     └── shaders/
                    ├── Player.ts ← characters/
                    │     ├── Character.ts
                    │     └── CollisionSolver.ts
                    ├── Enemy.ts (abstract) ← characters/enemies/
                    │     ├── Character.ts
                    │     └── IEnemyAI.ts ← ai/
                    ├── CandlesManager.ts ← objects/
                    ├── GameHud.ts ← hud/
                    └── ScoreRepository.ts ← services/
```

---

## Game Resolution & Coordinate System

- **Logical canvas:** 1280 × 720 (16:9)
- **Coordinate origin:** bottom-left (y=0 at bottom, positive y goes up)
  — same as `Camera.ts` from reference: `world.y = app.screen.height`,
    `world.pivot.y = world.height`
- **Camera:** follows player, clamps to level bounds
- **Cell size:** defined in `LevelCustomization` (typically 96 × 96 px)
- **Z-index:** manual via `zIndex` property; containers use `sortableChildren = true`

---

## What is NOT ported

| C# Component | Reason |
|-------------|--------|
| `MarkInDeDark.LevelEditor/` | Desktop WPF app — not needed on web |
| `Penumbra` library | Reimplemented as `ShadowSystem.ts` |
| `FbonizziMonoGame/Drawing/DynamicScalingMatrixProvider.cs` | CSS scaling replaces this |
| `FbonizziMonoGame/Implementations/WindowsWebSiteOpener.cs` | `window.open()` on web |
| `XACT` / `SoundEffect` | Replaced by Howler.js |
| `FbonizziMonoGame/StringsLocalization/` | Use JSON i18n files |
| `MarkInDeDark.Windows/Program.cs` | Desktop entry point — replaced by `main.ts` |
