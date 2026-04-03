import { Container, Spritesheet } from "pixi.js";
import { AnimationsManager } from "../animation/AnimationsManager";
import { SpriteAnimation } from "../animation/SpriteAnimation";
import { getTexture } from "../assets/AssetsLoader";
import { GameGrid } from "../grid/GameGrid";
import { CharacterActions } from "../interaction/CharacterActions";
import { MarkLight } from "../lighting/MarkLight";
import { CandlesManager } from "../objects/CandlesManager";
import { GameTexts } from "../ui/GameTexts";
import { Character, ShadowConfig, ShadowOffsets, TorchOffsets } from "./Character";

const PLAYER_SPEED = 0.6; // seconds per cell (from C# GameConfig.PlayerSpeed)
const FRAME_DURATION_MS = 150; // ms per frame (from C# CharactersAnimationsMillisecondsBetweenFrames)

const TORCH_OFFSETS: TorchOffsets = {
  up: { x: -30, y: -38 },
  left: { x: -38, y: -38 },
  right: { x: 38, y: -38 },
  down: { x: 35, y: -30 },
};

// Task 1 parity: in C# the player torch is the shadow-casting light used by Penumbra.

// C# CharacterShadowConfiguration — TorchCharacter type (player, goblin, skeleton)
const TORCH_SHADOW_OFFSETS: ShadowOffsets = {
  up:       { x:  32, y: 32 },
  left:     { x:  32, y:  0 },  // sprite will be flipped
  right:    { x: -32, y:  0 },
  down:     { x:  -8, y:  0 },
  downIdle: { x: -32, y:  0 },
};

function buildAnimation(
  sheets: Spritesheet[],
  baseName: string,
  frameCount: number,
  loop = true,
): SpriteAnimation {
  const frames = Array.from({ length: frameCount }, (_, i) =>
    getTexture(sheets, `${baseName}_${i + 1}`),
  );
  const normalFrames = Array.from({ length: frameCount }, (_, i) => {
    try {
      return getTexture(sheets, `${baseName}_${i + 1}_normal`);
    } catch {
      return null;
    }
  }).filter((t): t is NonNullable<typeof t> => t !== null);

  return new SpriteAnimation(frames, normalFrames, FRAME_DURATION_MS, loop);
}

// Some death frames are absent from the spritesheets (2, 4, 6, 8 missing) —
// load only existing ones via try/catch, matching C# AssetsLoader frame-skip logic.
function buildDeathAnimation(sheets: Spritesheet[]): SpriteAnimation {
  const frames: ReturnType<typeof getTexture>[] = [];
  for (let i = 1; i <= 26; i++) {
    try {
      frames.push(getTexture(sheets, `Player/Death/player_front_dying_${i}`));
    } catch {
      /* frame not in spritesheet, skip */
    }
  }
  return new SpriteAnimation(frames, [], FRAME_DURATION_MS, false);
}

function buildShadowAnim(sheets: Spritesheet[], baseName: string, frameCount: number): SpriteAnimation {
  const frames = Array.from({ length: frameCount }, (_, i) =>
    getTexture(sheets, `${baseName}_${i + 1}`),
  );
  return new SpriteAnimation(frames, [], FRAME_DURATION_MS, true);
}

function buildTorchCharacterShadowAnimations(sheets: Spritesheet[]): AnimationsManager {
  const mgr = new AnimationsManager();
  const base = 'Shadows/TorchCharacter/shadow';
  mgr.add('IdleDown',  buildShadowAnim(sheets, `${base}_front_iddle`, 4));
  mgr.add('IdleUp',    buildShadowAnim(sheets, `${base}_back_iddle`,  4));
  mgr.add('IdleSide',  buildShadowAnim(sheets, `${base}_side_iddle`,  4));
  mgr.add('WalkDown',  buildShadowAnim(sheets, `${base}_front_walking`, 4));
  mgr.add('WalkUp',    buildShadowAnim(sheets, `${base}_back_walking`,  4));
  mgr.add('WalkSide',  buildShadowAnim(sheets, `${base}_side_walking`,  4));
  return mgr;
}

function buildPlayerAnimations(sheets: Spritesheet[]): AnimationsManager {
  const mgr = new AnimationsManager();
  mgr.add("WalkDown", buildAnimation(sheets, "Player/player_front_walking", 4));
  mgr.add("WalkUp", buildAnimation(sheets, "Player/player_back_walking", 4));
  mgr.add("WalkSide", buildAnimation(sheets, "Player/player_side_walking", 4));
  mgr.add("IdleDown", buildAnimation(sheets, "Player/player_front_iddle", 4));
  mgr.add("IdleUp", buildAnimation(sheets, "Player/player_back_iddle", 4));
  mgr.add("IdleSide", buildAnimation(sheets, "Player/player_side_iddle", 4));
  mgr.add("Dying", buildDeathAnimation(sheets));
  return mgr;
}

/** Concrete Player class (port of C# Player.cs). */
export class Player extends Character {
  private _candlesManager: CandlesManager | null = null;
  /** Called when the player tries to interact but gets a feedback message. */
  onMessage: ((text: string, color: string) => void) | undefined = undefined;

  constructor(
    grid: GameGrid,
    sheets: Spritesheet[],
    objectsContainer: Container,
    objectsNormalContainer: Container,
    shadowContainer: Container | null = null,
  ) {
    const startCell = grid.playerStartCell;
    const startX = startCell?.centerX ?? grid.playerStartCol * 192 + 96;
    const startY = startCell?.centerY ?? grid.playerStartRow * 192 + 96;

    const torchLight = new MarkLight({
      x: startX + TORCH_OFFSETS.down.x,
      y: startY + TORCH_OFFSETS.down.y,
      // C# z=90 is NOT scaled by C# camera zoom (0.8) — it stays as world-space z.
      // PixiJS XY screen-space deltas scale by zoom_pixi/zoom_c# = (8/15)/0.8 = 2/3.
      // To preserve nDotL (light direction), z must scale by the same 2/3 → 90 × 2/3 = 60.
      // To preserve attenuation, distSq scales by (2/3)² → 20_600 × 4/9 = 9156.
      z: 60, // C# 90 × (2/3)
      opacity: 1,
      radius: 1200,
      castsShadows: true,
      color1: 0xffedd6,
      color2: 0xffffff,
      distanceSquared: 9_156, // C# 20_600 × (2/3)²
    });
    // C#: player glow is used for specular, but excluded from additional lightmask composited on Penumbra lightmap.
    torchLight.drawInAdditionalLightMask = false;

    const shadowConfig: ShadowConfig | null = shadowContainer !== null ? {
      container: shadowContainer,
      animations: buildTorchCharacterShadowAnimations(sheets),
      offsets: TORCH_SHADOW_OFFSETS,
    } : null;

    super(grid, objectsContainer, objectsNormalContainer, {
      startX,
      startY,
      movementSpeed: PLAYER_SPEED,
      animations: buildPlayerAnimations(sheets),
      torch: { light: torchLight, offsets: TORCH_OFFSETS },
      ...(shadowConfig !== null ? { shadow: shadowConfig } : {}),
    });
  }

  /** Wire up the CandlesManager so the player can place candles. */
  setCandlesManager(mgr: CandlesManager): void {
    this._candlesManager = mgr;
  }

  override handleInput(action: CharacterActions): void {
    if (this.isDead || this.isDying) return;

    if (action === CharacterActions.DoorSwitch) {
      const cell = this.currentCell;
      if (!this.isMoving && cell && this._grid.hasDoorOnCell(cell)) {
        this._grid.switchDoorIfAny(cell);
      }
      return;
    }

    if (action === CharacterActions.Candle) {
      const cell = this.currentCell;
      if (this._candlesManager) {
        if (!this._candlesManager.doIHaveACandleLeft) {
          this.onMessage?.(GameTexts.notEnoughCandles, "#ff6b6b");
        } else if (cell && !this._candlesManager.isThereACandle(cell)) {
          this._candlesManager.placeCandle(cell.centerX, cell.centerY);
        }
      }
      return;
    }

    super.handleInput(action);
  }
}
