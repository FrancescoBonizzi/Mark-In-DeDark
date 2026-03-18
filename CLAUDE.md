# CLAUDE.md — Mark In DeDark Migration Rules

This file governs every session working on the MonoGame → PixiJS migration.
Read it at the start of every conversation before writing a single line of code.

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
