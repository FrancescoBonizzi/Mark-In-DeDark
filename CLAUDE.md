# CLAUDE.md — Mark In DeDark Migration Rules

This file governs every session working on the MonoGame → PixiJS migration.
Read it at the start of every conversation before writing a single line of code.

---

## Current Migration Status

> **Update this section at the end of every session.**

| Task | Description | Check manuale | Status |
|------|-------------|---------------|--------|
| Specs | CLAUDE.md, ARCHITECTURE_MAP.md, PATTERN_COOKBOOK.md | — | ✅ Done (2026-03-18) |
| Task 0 | Project scaffold (package.json, tsconfig, vite.config, index.html, main.ts, router skeleton) | `npm run dev` → navigare menu → levels → game/1 (canvas nero) → gameover → menu. Canvas si ridimensiona al resize. | ✅ Done (2026-03-18) |
| Task 1 | Core Primitives & Services (Camera, Controller, CollisionSolver, ScoreRepository, SoundManager, Numbers, Interval, ParticleSystem) | Console: ScoreRepository salva/legge da localStorage. Controller risponde a tastiera/touch. Camera si può spostare via codice di test. | ✅ Done (2026-03-18) |
| Task 2 | Asset Pipeline (AssetsLoader, MarkInDeDarkAssets, spritesheet + level conversion) | game/1: spritesheet caricato, almeno uno sprite visibile su canvas. File `.mark` parsato senza errori in console. | ⬜ Pending |
| Task 3 | Grid System (GameGrid, GameCell, Wall types, GridLoader) | game/1: griglia disegnata con celle e muri visibili. Navigare la griglia con la camera. | ⬜ Pending |
| Task 4 | Shadow System (ShadowSystem, Light, Hull, WGSL shaders) | game/1: una luce visibile, ombre proiettate dai muri. Muovere la luce e vedere le ombre aggiornarsi. | ⬜ Pending |
| Task 5 | Normal Mapping (NormalMapFilter WGSL/GLSL) | game/1: normal map attivo, superfici reagiscono alla posizione della luce con effetto 3D. | ⬜ Pending |
| Task 6 | Characters Base (Character abstract, Player) | game/1: player visibile, si muove con tastiera/touch, collide con i muri, camera lo segue. | ⬜ Pending |
| Task 7 | Enemy AI (IEnemyAI + all implementations, Enemy + concrete types) | game/1: almeno 2 tipi di nemico con AI diversa (patrol + hunter). Il player muore al contatto. | ⬜ Pending |
| Task 8 | Objects & Traps (Candle, CandlesManager, Exit, traps) | game/1: candele accese visibili, si spengono all'interazione. Uscita livello funziona. Trappole danneggiano il player. | ⬜ Pending |
| Task 9 | HUD & Pages (GameHud, menu, levelselect, gameover, router) | Flusso completo: menu → seleziona livello → gioca → muori → gameover con punteggio → menu. HUD mostra candele/tempo. | ⬜ Pending |
| Task 10 | Integration & Polish (Game.ts loop, audio unlock, responsive, GameConfig) | Giocare un livello intero dall'inizio alla fine. Audio funziona. Resize/orientamento gestiti. Nessun errore in console. | ⬜ Pending |

**Legend:** ⬜ Pending · 🔄 In Progress · ✅ Done · ❌ Blocked

### Last session notes
<!-- Replace this with a short summary at the end of each session -->
- 2026-03-18: Created all spec files. Migration not yet started. Awaiting approval to begin Task 0.
- 2026-03-18: Task 0 completed. Scaffold created: package.json, tsconfig, vite.config, index.html, main.ts, router (4 routes with Navigo hash mode), menu/levelselect/gameover placeholders, gamebootstrap (1280x720 canvas with resize), styles.css, AudioUnlocker. `npm run build` passes cleanly.
- 2026-03-18: Task 1 completed. 14 files created: Interval, StringHelper, Numbers, IHasCollisionRectangle, CharacterActions, constants, CollisionSolver, ScoreRepository, SoundManager, SoundInstance, Camera (worldHeight param), Controller (4-dir keyboard-only), Particle, ParticleSystem. Keyboard-only overlay added to gamebootstrap. `npm run build` passes cleanly.

---

## Tech Stack (fixed)

| Tool | Version | Notes |
|------|---------|-------|
| Vite | 7.x | Build tool, `base` = `/mark-in-dedark/` |
| TypeScript | 5.7 strict | `"strict": true`, no implicit any |
| PixiJS | 8.6.x | WebGPU primary, WebGL fallback |
| Howler | 2.2.x | Audio; singleton `SoundManager` |
| Navigo | 8.x | Hash router (`{ hash: true }`) |

All dependencies live in `Web/`. No other runtime dependencies are permitted
without explicit approval.

---

## Migration order (bottom-up)

Implement in this order. Never start a layer before its dependencies compile.

```
0. Scaffold (package.json, tsconfig, vite.config, index.html, main.ts)
1. Core Primitives & Services   (Camera, Controller, CollisionSolver,
                                  ScoreRepository, SoundManager,
                                  Numbers, Interval, StringHelper,
                                  ParticleSystem, Particle, AudioUnlocker)
2. Asset Pipeline               (AssetsLoader, MarkInDeDarkAssets types,
                                  spritesheet conversion, level conversion)
3. Grid System                  (GameGrid, GameCell, Wall types, GridLoader)
4. Shadow System                (ShadowSystem, Light, Hull, shaders)
5. Normal Mapping               (NormalMapFilter WGSL/GLSL)
6. Characters Base              (Character abstract, Player)
7. Enemy AI                     (IEnemyAI interface + all implementations,
                                  Enemy abstract + Zombie/Ghost/Goblin/…)
8. Objects & Traps              (Candle, CandlesManager, Exit, traps)
9. HUD & Pages                  (GameHud, menu, levelselect, gameover,
                                  router with lifecycle)
10. Integration & Polish        (Game.ts main loop, responsive, GameConfig)
```

**Dependency-first rule:** a class may only be written after all its
constructor/import dependencies are already in a compilable state.

---

## Copy-first policy

The following files must be **copied verbatim from `progetto-riferimento/Web/src/`**
and then adapted (rename identifiers, change asset paths, adjust types):

- `world/Camera.ts`
- `interaction/Controller.ts`
- `services/CollisionSolver.ts`
- `services/ScoreRepository.ts`
- `services/SoundManager.ts`
- `services/Numbers.ts`
- `services/StringHelper.ts`
- `services/AudioUnlocker.ts`
- `primitives/Interval.ts`
- `particleEmitters/ParticleSystem.ts`
- `particleEmitters/Particle.ts`
- `pages/router.ts` (structure only — routes differ)
- `pages/gamebootstrap.ts` (structure only — game dimensions differ: 1280×720)

Do not rewrite these from scratch. Copy first, then adapt.

---

## Asset pipeline

- Spritesheets: TexturePacker JSON format + PNG atlas (same as `progetto-riferimento`)
- Fonts: loaded via `Assets.addBundle` / `Assets.loadBundle`
- Sounds: MP3, loaded lazily by `SoundManager` via Howler
- Level files: **`.mark` files are plain-text** (pipe-delimited, serialized by
  `GameGridSerializerV4`). Port the deserializer to TypeScript as
  `grid/GridLoader.ts`. Do NOT treat them as binary.

### Level file format (V4)

```
Line 0:  "GameGridSerializerV4"          ← version tag
Line 1:  rows|columns
Line 2:  environmentTexturePackId|dirtyProbabilityPercentage
Line 3+: row|col|groundType|objectType|objectAdditionalData|top|right|bottom|left|stringCode
```

Only non-empty cells (groundType ≠ 0) are written. All enum values are stored
as their integer codes. See `migration-specs/ARCHITECTURE_MAP.md` for the enum
tables.

---

## Shadow system rules

- Implement as `lighting/ShadowSystem.ts` — **no external shadow library**.
- Architecture: RenderTexture lightmap, composited over the scene with
  `PIXI.BlendMode.MULTIPLY`.
- Shadow hull geometry: `PIXI.Mesh` with trapezoid vertices computed in JS
  from wall segments.
- Shaders: always write **both** `{ gpu: wgslSource, gl: glslSource }` using
  PixiJS 8 dual-shader objects. WGSL for WebGPU, GLSL for WebGL fallback.
- One `ShadowSystem` instance per scene; it owns all `RenderTexture` resources.
- `MarkLight` properties to preserve: `position`, `color` (animated between two
  colors), `opacity`, `radius` (was `distanceSquared` — convert to linear),
  `castsShadows`, `z` (height for normal mapping).

---

## Shader strategy

```
// PixiJS 8 dual-shader pattern
import { GlProgram, GpuProgram } from 'pixi.js';

const program = new GpuProgram({            // WebGPU
  vertex: { source: vertWgsl, entryPoint: 'main' },
  fragment: { source: fragWgsl, entryPoint: 'main' },
});
// OR use the helper:
const glProgram = GlProgram.from({          // WebGL fallback
  vertex: vertGlsl,
  fragment: fragGlsl,
});
```

Never write GLSL-only shaders. Every shader file must have a WGSL twin.

---

## Game resolution

- Logical canvas: **1280 × 720** (16:9)
- CSS scaling via `ResizeObserver` (same pattern as `gamebootstrap.ts` in
  reference, with `GAME_W = 1280`, `GAME_H = 720`)
- Camera coordinate system: **bottom-up** (y=0 at bottom, positive y goes up),
  matching `Camera.ts` from the reference

---

## Anti-patterns (forbidden)

| Forbidden | Reason |
|-----------|--------|
| `XNA.Rectangle` wrapper class | `PIXI.Rectangle` is sufficient |
| `BinaryFormatter` deserialization | `.mark` files are text, not binary |
| External Penumbra/shadow library | Reimplement in WGSL/GLSL |
| GLSL-only shaders | PixiJS 8 needs dual shaders |
| `any` TypeScript type | `"strict": true` — use proper types |
| Polyfills for MonoGame patterns | Adapt to web idioms directly |
| Mixing scene teardown and construction | Always `destroyGame()` before `initGame()` |

---

## Validation checkpoint (end of each Task)

At the end of every significant Task, output a checkpoint block:

```
## Checkpoint — Task N
- Files created/modified: …
- Dependencies satisfied: …
- TypeScript compiles: ✓ / ✗
- Known gaps: …
- Next task: …
```

---

## Enemy AI implementations

Port all `IEnemyAI` implementations from `MarkInDeDark.Library/AI/`:

| C# class | TS file | Behaviour |
|----------|---------|-----------|
| `IdleAI` | `ai/IdleAI.ts` | Does nothing |
| `PatrolAI` | `ai/PatrolAI.ts` | Follows a predefined path in cell coords |
| `PlayerHunterAI` | `ai/PlayerHunterAI.ts` | Pathfinds toward player |
| `PatrolHunterLineOfSightAI` | `ai/PatrolHunterLineOfSightAI.ts` | Patrol + aggro on LoS |
| `PlayerHunterLineOfSightAI` | `ai/PlayerHunterLineOfSightAI.ts` | Hunt only when LoS |
| `KillCandleAI` | `ai/KillCandleAI.ts` | Seeks and extinguishes candles |
| `ChargeDirectionAI` | `ai/ChargeDirectionAI.ts` | Charges in a fixed direction |
| `PatrolChargeAI` | `ai/PatrolChargeAI.ts` | Patrol then charge |
| `PatrolGhostAI` | `ai/PatrolGhostAI.ts` | Ghost variant (walks through walls) |
| `RandomMovementAI` | `ai/RandomMovementAI.ts` | Random walk |

---

## Pages / Routes

| Route | Page |
|-------|------|
| `/` | Main menu |
| `/levels` | Level select |
| `/game/:levelId` | Game (loads level by ID) |
| `/gameover` | Game over + score |

Router uses Navigo 8 with `{ hash: true }`.
Each route with a game must implement `leave` lifecycle to call `destroyGame()`.

---

## Score / Persistence

Key prefix: `mark-in-dedark-` (not `infart-score-`).
Keys: `level-{n}-completed`, `level-{n}-time`, `level-{n}-candles`.
Implementation: adapt `ScoreRepository.ts` from reference (same localStorage
pattern, different key schema).
