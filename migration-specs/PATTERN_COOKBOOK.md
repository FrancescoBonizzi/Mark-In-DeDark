# PATTERN_COOKBOOK.md — MonoGame → PixiJS Pattern Reference

Each section shows the C# pattern on the left and the TypeScript/PixiJS 8
equivalent on the right. Code is taken from or aligned with
`progetto-riferimento/Web/src/`.

---

## 1. Game Loop & Ticker

### C# (MonoGame)

```csharp
protected override void Update(GameTime gameTime)
{
    float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
    player.Update(elapsed);
}

protected override void Draw(GameTime gameTime)
{
    spriteBatch.Begin();
    player.Draw(spriteBatch);
    spriteBatch.End();
}
```

### TypeScript (PixiJS 8)

```typescript
// gamebootstrap.ts
app.ticker.add((time: Ticker) => {
    game.update(time);
    // Draw is implicit — PixiJS renders the scene graph automatically
});

// Game.ts
update(time: Ticker) {
    const deltaMS = time.deltaMS; // milliseconds since last frame
    const deltaS  = deltaMS / 1000; // seconds if needed
    this._player.update(time);
}
```

**Key differences:**

- `time.deltaMS` ≈ `gameTime.ElapsedGameTime.TotalMilliseconds`
- No explicit `Draw()` call — mutate the scene graph in `update()`, PixiJS renders it
- `app.ticker.stop()` / `app.ticker.start()` for pause (e.g., orientation lock)

---

## 2. Asset Loading

### C# (MonoGame)

```csharp
Texture2D texture = Content.Load<Texture2D>("player/idle");
SpriteFont font   = Content.Load<SpriteFont>("Fonts/main");
```

### TypeScript (PixiJS 8)

```typescript
// AssetsLoader.ts
export const loadAssets = async (): Promise<MarkInDeDarkAssets> => {
  const gameSheet = await Assets.load(
    `${base}assets/images/spriteSheets/game.json`,
  );
  const spriteSheet = gameSheet as Spritesheet;

  return {
    textures: {
      player: {
        idleDown: new AnimatedSprite(spriteSheet.animations.playerIdleDown!),
        walkUp: new AnimatedSprite(spriteSheet.animations.playerWalkUp!),
        // …
      },
      candle: spriteSheet.textures["candle"]!,
    },
  };
};

// Font loading
const bundleName = "fonts";
Assets.addBundle(bundleName, [
  { alias: "MyFont", src: `${base}assets/fonts/MyFont.ttf` },
]);
await Assets.loadBundle(bundleName);
```

**Key differences:**

- Single `Assets.load(jsonUrl)` loads both the atlas JSON and PNG
- Animations come from `spriteSheet.animations['animName']` → `Texture[]`
- Static frames from `spriteSheet.textures['frameName']`

---

## 3. Sprite & AnimatedSprite

### C# (MonoGame)

```csharp
// Static sprite
spriteBatch.Draw(texture, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, layerDepth);

// Animated sprite
animationsManager.PlayAnimation("WalkDown");
animationsManager.Update(elapsed);
animationsManager.Draw(spriteBatch, drawingInfos);
```

### TypeScript (PixiJS 8)

```typescript
// Static sprite
const sprite = new Sprite(texture);
sprite.anchor.set(0.5); // origin at center
sprite.position.set(x, y);
sprite.scale.set(scaleX, scaleY);
sprite.zIndex = layerDepth;
camera.addToWorld(sprite);

// Animated sprite
const anim = new AnimatedSprite(frames);
anim.animationSpeed = 0.15; // frames per tick (not per second)
anim.anchor.set(0.5);
anim.loop = true;
anim.play();
camera.addToWorld(anim);

// Flip horizontally (mirrors SpriteEffects.FlipHorizontally)
sprite.scale.x = -Math.abs(sprite.scale.x); // or sprite.texture.source.flipX
```

**Changing animation:** stop old AnimatedSprite, start new one, or swap `textures`:

```typescript
anim.textures = spriteSheet.animations["walkUp"]!;
anim.play();
```

---

## 4. Camera

The camera in the reference uses a bottom-up coordinate system.
`Camera.ts` wraps a PixiJS `Container` (`_world`) whose origin is pinned to the
bottom-left of the screen.

```typescript
// world/Camera.ts (copy from reference, adjust worldHeight for Mark In DeDark)
class Camera {
  private readonly _worldHeight = 4000; // adjust to level height in pixels
  private readonly _world: Container;

  constructor(app: Application<Renderer>) {
    this._world = new Container();
    this._world.y = app.screen.height; // origin at bottom
    this._world.pivot.y = this._world.height; // y=0 is floor
    app.stage.addChild(this._world);
  }

  addToWorld(child: Container) {
    this._world.addChild(child);
  }

  set x(x: number) {
    this._world.x = -x * this._world.scale.x;
  }
  set y(y: number) {
    this._world.y = -y * this._world.scale.y + this._height;
  }
}
```

**Following the player:**

```typescript
// Game.ts update()
this._camera.x = Numbers.lerp(this._camera.x, player.position.x - 640, 0.08);
this._camera.y = Numbers.lerp(this._camera.y, player.position.y - 360, 0.08);
```

MonoGame equivalent: `Matrix.CreateTranslation(-camera.x, -camera.y, 0)` passed
to `SpriteBatch.Begin(transformMatrix: matrix)`.

---

## 5. Collisions

### C# (MonoGame)

```csharp
Rectangle playerRect = player.CollisionRectangle;
if (playerRect.Intersects(wall.BoundingRectangle))
    ResolveCollision(player, wall);
```

### TypeScript (PixiJS 8)

```typescript
// services/CollisionSolver.ts (copy from reference)
import { Rectangle } from "pixi.js";

const CollisionSolver = {
  checkCollisions(who: Rectangle, withRectangles: Rectangle[]): boolean {
    for (const r of withRectangles) if (who.intersects(r)) return true;
    return false;
  },

  checkCollisionsReturnCollidingObjectSpecific<
    T extends IHasCollisionRectangle,
  >(who: IHasCollisionRectangle, withList: T[]): T | null {
    for (const item of withList)
      if (who.collisionRectangle.intersects(item.collisionRectangle))
        return item;
    return null;
  },
};

// IHasCollisionRectangle.ts
interface IHasCollisionRectangle {
  collisionRectangle: Rectangle;
}
```

---

## 6. Input (Keyboard / Mouse / Touch)

### C# (MonoGame)

```csharp
KeyboardState kb = Keyboard.GetState();
if (kb.IsKeyDown(Keys.Up))    player.HandleInput(CharacterActions.Up);
if (kb.IsKeyDown(Keys.Down))  player.HandleInput(CharacterActions.Down);
```

### TypeScript (PixiJS 8)

For the single-button Infart-style controller, use `Controller.ts` verbatim.

For Mark In DeDark's 4-directional input, extend it:

```typescript
// interaction/Controller.ts (adapted)
class Controller {
  public up = false;
  public down = false;
  public left = false;
  public right = false;

  constructor() {
    window.addEventListener("keydown", (e) => {
      if (e.key === "ArrowUp" || e.key === "w") this.up = true;
      if (e.key === "ArrowDown" || e.key === "s") this.down = true;
      if (e.key === "ArrowLeft" || e.key === "a") this.left = true;
      if (e.key === "ArrowRight" || e.key === "d") this.right = true;
    });
    window.addEventListener("keyup", (e) => {
      if (e.key === "ArrowUp" || e.key === "w") this.up = false;
      if (e.key === "ArrowDown" || e.key === "s") this.down = false;
      if (e.key === "ArrowLeft" || e.key === "a") this.left = false;
      if (e.key === "ArrowRight" || e.key === "d") this.right = false;
    });
    // Touch: virtual d-pad handled in HUD
    window.addEventListener("blur", () => this.reset());
  }

  toCharacterAction(): CharacterActions {
    if (this.up) return CharacterActions.Up;
    if (this.down) return CharacterActions.Down;
    if (this.left) return CharacterActions.Left;
    if (this.right) return CharacterActions.Right;
    return CharacterActions.Idle;
  }

  private reset() {
    this.up = this.down = this.left = this.right = false;
  }
}
```

---

## 7. Particle System (Pool Pattern)

The reference `ParticleSystem.ts` uses a free/active pool. Copy it verbatim,
then subclass for each emitter type:

```typescript
// particleEmitters/CandleFireParticleSystem.ts
import ParticleSystem from './ParticleSystem.ts';
import Camera from '../world/Camera.ts';

export class CandleFireParticleSystem extends ParticleSystem {
    constructor(texture: Texture, camera: Camera) {
        super(
            texture, camera,
            density: 1,
            numParticles: { min: 3, max: 6 },
            speed: { min: 20, max: 60 },
            acceleration: { min: -5, max: 5 },
            rotationSpeed: { min: -1, max: 1 },
            lifetimeSeconds: { min: 0.3, max: 0.8 },
            scale: { min: 0.2, max: 0.5 },
            spawnAngleInDegrees: { min: 60, max: 120 }, // mostly upward
        );
    }
}
```

---

## 8. UI / HUD

### C# (MonoGame)

```csharp
// Draw HUD after game world, no transform matrix
spriteBatch.Begin();
spriteBatch.DrawString(font, score.ToString(), new Vector2(20, 20), Color.White);
spriteBatch.End();
```

### TypeScript (PixiJS 8)

```typescript
// hud/GameHud.ts
class GameHud {
  private readonly _scoreText: Text;
  private readonly _container: Container;

  constructor(app: Application) {
    this._container = new Container();
    // HUD is added to stage DIRECTLY (not to camera world)
    app.stage.addChild(this._container);
    this._container.zIndex = 9999;

    this._scoreText = new Text({
      text: "0",
      style: { fill: 0xffffff, fontSize: 24 },
    });
    this._scoreText.position.set(20, 20);
    this._container.addChild(this._scoreText);
  }

  updateScore(score: number) {
    this._scoreText.text = score.toString();
  }

  update(_time: Ticker) {
    /* animate HUD elements */
  }
}
```

**Key rule:** HUD lives on `app.stage` directly, not inside `camera.world`.
This pins it to screen coordinates regardless of camera scroll.

---

## 9. Audio

### C# (MonoGame / XACT)

```csharp
soundEffect.Play();
soundEffectInstance.IsLooped = true;
soundEffectInstance.Play();
soundEffectInstance.Stop();
```

### TypeScript (PixiJS 8 + Howler.js)

```typescript
// services/SoundManager.ts (adapt from reference)
import { Howl, Howler } from "howler";

class SoundManager {
  private readonly sounds: Record<string, Howl> = {};

  constructor() {
    Howler.autoUnlock = true;
    this.preload();
  }

  private preload() {
    this.sounds["musicGame"] = new Howl({
      src: [`${assetsRoot}/sounds/music/game.mp3`],
      loop: true,
      volume: 0.5,
    });
    this.sounds["candlePick"] = new Howl({
      src: [`${assetsRoot}/sounds/effects/candlePick.mp3`],
    });
    // …
  }

  playGameMusic() {
    if (!this.sounds["musicGame"]!.playing()) this.sounds["musicGame"]!.play();
  }

  stopAll() {
    Howler.stop();
  }
}

// services/AudioUnlocker.ts (copy from reference)
export function wireAudioUnlockOnce() {
  const unlock = () => {
    Howler.ctx?.resume();
    window.removeEventListener("pointerdown", unlock);
    window.removeEventListener("keydown", unlock);
  };
  window.addEventListener("pointerdown", unlock, { once: true });
  window.addEventListener("keydown", unlock, { once: true });
}
```

**iOS note:** call `wireAudioUnlockOnce()` in `main.ts` before any user interaction
is needed. iOS requires a user gesture before audio context can start.

---

## 10. Score & Persistence

### C# (MonoGame)

```csharp
// ISettingsRepository
_settings.Set("level_1_completed", "true");
bool completed = _settings.Get("level_1_completed") == "true";
```

### TypeScript (PixiJS 8)

```typescript
// services/ScoreRepository.ts (adapt from reference)
const PREFIX = "mark-in-dedark-";

export type LevelKey = `level-${number}-${"completed" | "time" | "candles"}`;

export default {
  setLevel(key: LevelKey, value: string | number) {
    localStorage.setItem(PREFIX + key, String(value));
  },
  getLevel(key: LevelKey): string | null {
    return localStorage.getItem(PREFIX + key);
  },
  isLevelCompleted(level: number): boolean {
    return localStorage.getItem(`${PREFIX}level-${level}-completed`) === "true";
  },
};
```

---

## 11. Routing / Scene Management

### C# (MonoGame)

```csharp
// GameState machine
switch (_currentState) {
    case GameState.Menu:   _menuPage.Update(elapsed); break;
    case GameState.InGame: _game.Update(elapsed); break;
}
```

### TypeScript (PixiJS 8 + Navigo)

```typescript
// pages/router.ts (adapt from reference)
import Navigo from "navigo";
import { renderMenuPage } from "./menu";
import { renderLevelSelectPage } from "./levelselect";
import { initGame, destroyGame } from "./gamebootstrap";
import { renderGameOverPage } from "./gameover";

const router = new Navigo("/", { hash: true });

export function initializeRouter() {
  const app = document.getElementById("app")!;

  router
    .on("/", () => renderMenuPage(app))
    .on("/levels", () => renderLevelSelectPage(app))
    .on("/game/:id", ({ data }) => initGame(app, Number(data!.id)), {
      leave: (done) => {
        destroyGame();
        done();
      },
    })
    .on("/gameover", () => renderGameOverPage(app))
    .notFound(() => renderMenuPage(app))
    .resolve();
}

export default router;
```

**Pattern:** each page is a function that renders HTML into `#app`.
The `/game` route receives the level ID from the URL.

---

## 12. Shadow System (Critical)

The original game uses the **Penumbra** library (MonoGame) which renders
soft-shadow occlusion using stencil-buffer techniques. PixiJS 8 has no
equivalent library, so we reimplement the algorithm using RenderTextures and
custom shaders.

### Algorithm Overview

```
Each frame:
1. Clear lightmap RenderTexture to ambient darkness (e.g. rgba(0,0,0,0.95))
2. For each active PointLight:
   a. Render light cone/circle into lightmap (additive blend)
   b. For each Hull (wall segment) visible from this light:
      - Compute shadow trapezoid (penumbra + umbra) in world space
      - Draw trapezoid into lightmap (subtractive blend — cut out light)
3. Render game scene normally to screen
4. Composite lightmap over scene with BlendMode.MULTIPLY
```

### TS Skeleton

```typescript
// lighting/ShadowSystem.ts
import {
  Application,
  Container,
  RenderTexture,
  Sprite,
  BLEND_MODES,
  Graphics,
  Mesh,
  MeshGeometry,
} from "pixi.js";
import { Light } from "./Light.ts";
import { Hull } from "./Hull.ts";

export class ShadowSystem {
  private readonly _lightmap: RenderTexture;
  private readonly _lightmapSprite: Sprite;
  private readonly _lights: Light[] = [];
  private readonly _hulls: Hull[] = [];

  constructor(app: Application) {
    this._lightmap = RenderTexture.create({
      width: app.screen.width,
      height: app.screen.height,
    });
    this._lightmapSprite = new Sprite(this._lightmap);
    this._lightmapSprite.blendMode = "multiply";
    // Add to stage AFTER the world container
    app.stage.addChild(this._lightmapSprite);
  }

  addLight(light: Light) {
    this._lights.push(light);
  }
  addHull(hull: Hull) {
    this._hulls.push(hull);
  }
  removeLight(light: Light) {
    this._lights.splice(this._lights.indexOf(light), 1);
  }

  update(app: Application) {
    // 1. Clear to dark
    const g = new Graphics();
    g.rect(0, 0, app.screen.width, app.screen.height).fill({
      color: 0x000000,
      alpha: 0.95,
    });
    app.renderer.render({ container: g, target: this._lightmap, clear: true });

    // 2. For each light, render illuminated area + shadows
    for (const light of this._lights) {
      if (!light.enabled) continue;
      this._renderLight(app, light);
    }
  }

  private _renderLight(app: Application, light: Light) {
    // Render radial gradient (light circle) — additive
    const lightGfx = light.buildLightGraphics(); // circle with radial gradient
    app.renderer.render({
      container: lightGfx,
      target: this._lightmap,
      clear: false,
    });

    // Render shadow trapezoids for each hull — subtractive
    for (const hull of this._hulls) {
      const trapezoid = hull.computeShadowMesh(light);
      if (!trapezoid) continue;
      app.renderer.render({
        container: trapezoid,
        target: this._lightmap,
        clear: false,
      });
    }
  }
}
```

### Hull Shadow Geometry

```typescript
// lighting/Hull.ts
import { Point, Mesh, MeshGeometry, Shader } from "pixi.js";
import { Light } from "./Light.ts";

export class Hull {
  // An axis-aligned wall segment (from its start to end in world space)
  constructor(
    public p1: Point,
    public p2: Point,
  ) {}

  computeShadowMesh(light: Light): Mesh | null {
    // Project p1 and p2 away from the light to infinity (or far clip)
    const FAR = 2000;
    const lx = light.position.x,
      ly = light.position.y;

    const d1x = this.p1.x - lx,
      d1y = this.p1.y - ly;
    const d2x = this.p2.x - lx,
      d2y = this.p2.y - ly;

    // Extend far point
    const len1 = Math.sqrt(d1x * d1x + d1y * d1y) || 1;
    const len2 = Math.sqrt(d2x * d2x + d2y * d2y) || 1;

    const far1x = this.p1.x + (d1x / len1) * FAR;
    const far1y = this.p1.y + (d1y / len1) * FAR;
    const far2x = this.p2.x + (d2x / len2) * FAR;
    const far2y = this.p2.y + (d2y / len2) * FAR;

    // Trapezoid: p1, p2, far2, far1
    const vertices = new Float32Array([
      this.p1.x,
      this.p1.y,
      this.p2.x,
      this.p2.y,
      far2x,
      far2y,
      far1x,
      far1y,
    ]);
    const indices = new Uint32Array([0, 1, 2, 0, 2, 3]);

    const geometry = new MeshGeometry({ positions: vertices, indices });
    // Use a flat black shader (shadow color)
    // TODO: use ShadowShader for soft penumbra
    return new Mesh({ geometry, shader: ShadowShader.instance });
  }
}
```

### Shader Strategy (WGSL + GLSL dual)

```typescript
// lighting/shaders/lightShader.ts
import { GpuProgram, GlProgram, Shader } from "pixi.js";

const wgsl = /* wgsl */ `
struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) uv: vec2<f32>,
};

@vertex
fn main(@location(0) aPosition: vec2<f32>) -> VertexOutput {
    var out: VertexOutput;
    out.position = vec4<f32>(aPosition, 0.0, 1.0);
    out.uv = (aPosition + 1.0) * 0.5;
    return out;
}

@fragment
fn main(@location(0) uv: vec2<f32>) -> @location(0) vec4<f32> {
    let d = length(uv - vec2<f32>(0.5));
    let intensity = 1.0 - smoothstep(0.0, 0.5, d);
    return vec4<f32>(1.0, 0.85, 0.5, intensity); // warm light color
}
`;

const glsl = /* glsl */ `
precision mediump float;
varying vec2 vUV;
void main() {
    float d = length(vUV - vec2(0.5));
    float intensity = 1.0 - smoothstep(0.0, 0.5, d);
    gl_FragColor = vec4(1.0, 0.85, 0.5, intensity);
}
`;

// Usage:
const program = new GpuProgram({
  vertex: { source: wgsl, entryPoint: "main" },
  fragment: { source: wgsl, entryPoint: "main" },
});
const glProgram = GlProgram.from({ vertex: "...", fragment: glsl });
```

---

## 13. Normal Mapping

### C# (MonoGame)

```csharp
// Custom Effect pass with normal map texture + light positions
_normalMapEffect.Parameters["LightPosition"].SetValue(lightPos);
_normalMapEffect.Parameters["LightColor"].SetValue(lightColor);
spriteBatch.Begin(effect: _normalMapEffect);
spriteBatch.Draw(normalMapTexture, position, Color.White);
spriteBatch.End();
```

### TypeScript (PixiJS 8)

```typescript
// lighting/NormalMapFilter.ts
import { Filter, GlProgram, GpuProgram } from "pixi.js";

export class NormalMapFilter extends Filter {
  constructor() {
    const glProgram = GlProgram.from({
      vertex: normalMapVertGlsl,
      fragment: normalMapFragGlsl,
    });
    super({ glProgram });
  }

  setLights(
    lights: Array<{
      position: { x: number; y: number };
      color: number;
      intensity: number;
    }>,
  ) {
    // Pack into uniform arrays
    this.resources.uniforms.uniforms.uLightCount = lights.length;
    // …
  }
}

// Usage on a sprite:
const sprite = new Sprite(diffuseTexture);
const normalMapFilter = new NormalMapFilter();
sprite.filters = [normalMapFilter];
// Set normal map texture via a uniform
normalMapFilter.resources.normalMapTexture = normalTexture.source;
```

### Task 1 parity notes (C# -> PixiJS)

- Level ambient must match C# `LevelCustomizations` exactly:

1. levels 1-2: `normalMapAmbienceColor = (0.11, 0.10, 0.22)` and `penumbraAmbientColor = (104,104,104)`
2. levels >=3: `normalMapAmbienceColor = (0,0,0)` and `penumbraAmbientColor = black`

- `GlowScale` is not global-constant in C#:

1. default is `1` for levels >=3
2. overridden to `2` only for levels 1-2

- Player light must cast shadows in the penumbra/shadow-lightmap pass (`castsShadows = true`).
  C# enables `PenumbraLight.CastsShadows` only for the player torch.

- Entity light overrides required for parity:

1. `Ghost.deltaForWall = (0, -30)`
2. `Brazier.deltaForWall = (0, +4)` and `distanceSquaredForWall = 17000`
3. Candle keeps default wall delta and uses `distanceSquared = 7280`

- Normal map shader parity constraints:

1. decode normal in `[-1,1]` and flip Y (`normal.y *= -1`)
2. diffuse = Lambert (`dot(normal, lightDir)` with clamp)
3. attenuation = `LightDistanceSquared / |lightVector|^2`
4. keep both WGSL and GLSL implementations aligned

---

## 14. Grid System

### C# (MonoGame)

```csharp
// GameGrid.cs
GameCell cell = _gameGrid.GetCellAtPixel(player.Position);
bool canMove = cell.IsNorthPassable && !cell.NorthCell.IsUnwalkable;
```

### TypeScript (PixiJS 8)

```typescript
// grid/GameGrid.ts
export class GameGrid {
  private readonly _cells: GameCell[][];
  readonly cellWidth: number;
  readonly cellHeight: number;

  constructor(bundle: GameGridBundle, cellWidth: number, cellHeight: number) {
    this.cellWidth = cellWidth;
    this.cellHeight = cellHeight;
    this._cells = this._buildCells(bundle);
    this._linkCells();
  }

  getCellAtPixel(x: number, y: number): GameCell | null {
    const col = Math.floor(x / this.cellWidth);
    const row = Math.floor(y / this.cellHeight);
    return this._cells[row]?.[col] ?? null;
  }

  private _linkCells() {
    // Set northCell, southCell, eastCell, westCell references
    for (let r = 0; r < this._cells.length; r++) {
      for (let c = 0; c < this._cells[r]!.length; c++) {
        const cell = this._cells[r]![c]!;
        cell.northCell = this._cells[r - 1]?.[c] ?? null;
        cell.southCell = this._cells[r + 1]?.[c] ?? null;
        cell.eastCell = this._cells[r]?.[c + 1] ?? null;
        cell.westCell = this._cells[r]?.[c - 1] ?? null;
      }
    }
  }
}
```

### Level Deserializer

```typescript
// grid/GridLoader.ts — port of GameGridSerializerV4.cs
export function loadGridFromText(text: string): GameGridBundle {
  const lines = text.trim().split("\n");
  if (lines[0]!.trim() !== "GameGridSerializerV4")
    throw new Error("Incompatible level format");

  const [rows, cols] = lines[1]!.split("|").map(Number);
  const [envPackId, dirtyPct] = lines[2]!.split("|").map(Number);

  const bundle = new GameGridBundle(rows!, cols!);
  bundle.environmentTexturePackId = envPackId!;
  bundle.dirtyProbabilityPercentage = dirtyPct!;

  for (const line of lines.slice(3)) {
    const parts = line.split("|");
    const row = Number(parts[0]),
      col = Number(parts[1]);
    bundle.setGroundType(row, col, Number(parts[2]) as GroundTypes);
    if (parts[3])
      bundle.setObjectType(
        row,
        col,
        Number(parts[3]) as ObjectTypes,
        parts[4] ?? "",
      );
    bundle.setWallType(
      row,
      col,
      Number(parts[5]) as WallTypes,
      WallPositions.Top,
    );
    bundle.setWallType(
      row,
      col,
      Number(parts[6]) as WallTypes,
      WallPositions.Right,
    );
    bundle.setWallType(
      row,
      col,
      Number(parts[7]) as WallTypes,
      WallPositions.Bottom,
    );
    bundle.setWallType(
      row,
      col,
      Number(parts[8]) as WallTypes,
      WallPositions.Left,
    );
    if (parts[9]) bundle.setStringCode(row, col, parts[9]);
  }

  return bundle;
}
```

---

## 15. Character Movement (Grid-Based Lerp)

### C# (MonoGame)

```csharp
// Character.cs — lerps position from current cell center to next cell center
_movementLerpAmount += (float)(elapsed.TotalSeconds / CellMovementSeconds);
Position = Vector2.Lerp(_startingLocation, _nextLocation, _movementLerpAmount);
```

### TypeScript (PixiJS 8)

```typescript
// characters/Character.ts
abstract class Character {
  protected _position = new Point(0, 0);
  protected _startPos = new Point(0, 0);
  protected _targetPos = new Point(0, 0);
  protected _lerpAmount = 1.0;
  protected cellMovementSeconds = 0.3;

  get position() {
    return this._position;
  }

  get isMoving() {
    return this._lerpAmount < 1.0;
  }

  protected moveToCell(cell: GameCell) {
    this._startPos.copyFrom(this._position);
    this._targetPos.copyFrom(cell.centerPosition);
    this._lerpAmount = 0;
  }

  update(time: Ticker) {
    if (this.isMoving) {
      this._lerpAmount += time.deltaMS / 1000 / this.cellMovementSeconds;
      this._lerpAmount = Math.min(this._lerpAmount, 1.0);
      this._position.x = Numbers.lerp(
        this._startPos.x,
        this._targetPos.x,
        this._lerpAmount,
      );
      this._position.y = Numbers.lerp(
        this._startPos.y,
        this._targetPos.y,
        this._lerpAmount,
      );
      this._sprite.position.copyFrom(this._position);
    }
  }
}
```

---

## 16. Enemy AI Interface

### C# (MonoGame)

```csharp
public interface IEnemyAI {
    CharacterActions Update(Enemy character, TimeSpan elapsed);
}
```

### TypeScript (PixiJS 8)

```typescript
// ai/IEnemyAI.ts
import type { Enemy } from "../characters/enemies/Enemy.ts";
import type { CharacterActions } from "../characters/CharacterActions.ts";

export interface IEnemyAI {
  update(enemy: Enemy, deltaMS: number): CharacterActions;
}

// ai/PatrolAI.ts (example)
export class PatrolAI implements IEnemyAI {
  private _pathIndex = 0;
  private readonly _path: Point[]; // cell centre positions

  constructor(path: Point[]) {
    this._path = path;
  }

  update(enemy: Enemy, _deltaMS: number): CharacterActions {
    if (enemy.isMoving) return CharacterActions.Idle;

    const target = this._path[this._pathIndex]!;
    const action = directionToward(enemy.position, target);

    if (enemy.position.x === target.x && enemy.position.y === target.y) {
      this._pathIndex = (this._pathIndex + 1) % this._path.length;
    }

    return action;
  }
}
```

---

## 17. Responsive & Orientation Lock

```typescript
// pages/gamebootstrap.ts — adapt from reference
const GAME_W = 1280,
  GAME_H = 720;

function resize() {
  if (!app || !gameContainer) return;
  const scale = Math.min(
    gameContainer.clientWidth / GAME_W,
    gameContainer.clientHeight / GAME_H,
  );
  app.renderer.resize(GAME_W, GAME_H);
  app.canvas.style.width = Math.floor(GAME_W * scale) + "px";
  app.canvas.style.height = Math.floor(GAME_H * scale) + "px";
  app.stage.scale.set(1);
}

// Orientation lock (best-effort — iOS ignores this)
try {
  await (screen.orientation as any).lock("landscape");
} catch {}
```

---

## 18. Scene Teardown

```typescript
// pages/gamebootstrap.ts
export function destroyGame() {
  if (!app) return;
  resizeObserver?.disconnect();
  resizeObserver = null;
  try {
    (screen.orientation as any).unlock();
  } catch {}
  app.destroy(true, { children: true }); // destroys all textures + containers
  app = null;
  gameContainer = null;
}
```

**Never** create a new `Application` without calling `destroyGame()` first —
WebGL contexts are limited.

---

## 19. GameConfig Loading

```typescript
// services/GameConfig.ts
export interface GameConfig {
  playerMovementSpeed: number;
  candleLightRadius: number;
  ambientLightAlpha: number;
  // … all params from GameConfig.cs
}

export async function loadGameConfig(base: string): Promise<GameConfig> {
  const response = await fetch(`${base}assets/config/game.json`);
  return response.json() as Promise<GameConfig>;
}
```

The JSON file is a direct port of `GameConfig.cs` properties, serialized once
from the C# original.
