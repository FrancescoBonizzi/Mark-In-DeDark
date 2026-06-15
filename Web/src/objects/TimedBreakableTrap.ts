import { Container, Rectangle, Sprite, Spritesheet } from "pixi.js";
import { SpriteAnimation } from "../animation/SpriteAnimation";
import { getTexture } from "../assets/AssetsLoader";
import { Character } from "../characters/Character";
import { Z_GROUND_OBJECTS } from "../grid/ZIndexCalculator";
import { InstantlyDieTrap } from "./InstantlyDieTrap";
import { TriggerObject } from "./TriggerObject";

const BREAK_TIME_MS = 2000; // GameConfig.BreakableTrapBreakingTimeSeconds
const COVER_FRAME_MS = 200;
const BREAK_FRAME_MS = 80;

const enum BreakState {
  Idle,
  GoingToBreak,
  Breaking,
  Broken,
}

/** Breakable cover trap — breaks after standing on it (port of C# TimedBreakableTrap.cs). */
export class TimedBreakableTrap extends TriggerObject {
  private _breakState: BreakState = BreakState.Idle;
  private _elapsed = 0;
  private _shakeElapsed = 0;
  private readonly _onGoingToBreak: (() => void) | undefined;

  private readonly _coverSprite: Sprite;
  private readonly _coverAnim: SpriteAnimation;
  private readonly _breakingAnim: SpriteAnimation;
  private readonly _brokenSprite: Sprite;
  private readonly _holeTrap: InstantlyDieTrap;

  private readonly _container: Container;

  constructor(
    sheets: Spritesheet[],
    cellX: number,
    cellY: number,
    cellSize: number,
    container: Container,
    onGoingToBreak?: () => void,
    affectedCharacters: Character[] = [],
  ) {
    const cx = cellX + cellSize / 2;
    const cy = cellY + cellSize / 2;
    // HitBoxTolerance(30, 30, 60, 60)
    super(new Rectangle(cx - 30, cy - 30, 60, 60), false, affectedCharacters);

    this._container = container;
    this._onGoingToBreak = onGoingToBreak;

    this._holeTrap = new InstantlyDieTrap(
      sheets,
      cellX,
      cellY,
      cellSize,
      container,
      affectedCharacters,
    );
    this._holeTrap.setVisible(false);

    // Cover animation (idle: 2 frames)
    const coverFrames = [
      getTexture(sheets, "Grounds/trap/trap_1"),
      getTexture(sheets, "Grounds/trap/trap_2"),
    ];
    this._coverAnim = new SpriteAnimation(
      coverFrames,
      [],
      COVER_FRAME_MS,
      true,
    );
    this._coverSprite = new Sprite(coverFrames[0]!);
    this._coverSprite.anchor.set(0.5);
    this._coverSprite.x = cx;
    this._coverSprite.y = cy;
    this._coverSprite.zIndex = Z_GROUND_OBJECTS;
    container.addChild(this._coverSprite);

    // Breaking animation (7 frames)
    const breakFrames = Array.from({ length: 7 }, (_, i) =>
      getTexture(sheets, `Grounds/trap/trap_breaking_${i + 1}`),
    );
    this._breakingAnim = new SpriteAnimation(
      breakFrames,
      [],
      BREAK_FRAME_MS,
      false,
    );

    // Broken state sprite
    const brokenTex = getTexture(sheets, "Grounds/trap/trap_broken");
    this._brokenSprite = new Sprite(brokenTex);
    this._brokenSprite.anchor.set(0.5);
    this._brokenSprite.x = cx;
    this._brokenSprite.y = cy;
    this._brokenSprite.zIndex = Z_GROUND_OBJECTS;
    this._brokenSprite.visible = false;
    container.addChild(this._brokenSprite);
  }

  override addAffectedCharacter(character: Character): void {
    this._holeTrap.addAffectedCharacter(character);
    super.addAffectedCharacter(character);
  }

  override removeAffectedCharacter(character: Character): void {
    this._holeTrap.removeAffectedCharacter(character);
    super.removeAffectedCharacter(character);
  }

  protected _onTrigger(character: Character, deltaMS: number): void {
    if (this._breakState === BreakState.Idle) {
      this._breakState = BreakState.GoingToBreak;
      this._elapsed = 0;
      this._currentTriggeringCharacter = character;
      return;
    }

    if (this._breakState === BreakState.GoingToBreak) {
      this._currentTriggeringCharacter = character;
      this._elapsed += deltaMS;
    }
  }

  protected override _onTriggerLeft(): void {
    if (this._breakState === BreakState.GoingToBreak) {
      this._breakState = BreakState.Idle;
      this._elapsed = 0;
      this._shakeElapsed = 0;
      this._coverSprite.texture = this._coverAnim.currentFrame;
    }
  }

  override update(deltaMS: number): void {
    switch (this._breakState) {
      case BreakState.Idle:
        super.update(deltaMS);
        this._coverSprite.visible = true;
        this._brokenSprite.visible = false;
        this._holeTrap.setVisible(false);
        break;

      case BreakState.GoingToBreak:
        super.update(deltaMS);
        this._coverAnim.update(deltaMS);
        this._coverSprite.texture = this._coverAnim.currentFrame;
        this._shakeElapsed += deltaMS;
        if (this._shakeElapsed >= 25 && this._currentTriggeringCharacter) {
          this._onGoingToBreak?.();
          this._shakeElapsed = 0;
        }
        if (this._elapsed >= BREAK_TIME_MS) {
          this._breakState = BreakState.Breaking;
          this._breakingAnim.reset();
          this._coverSprite.texture = this._breakingAnim.currentFrame;
          this._brokenSprite.visible = true;
          this._holeTrap.setVisible(true);
        }
        break;

      case BreakState.Breaking:
        this._brokenSprite.visible = true;
        this._holeTrap.setVisible(true);
        this._breakingAnim.update(deltaMS);
        this._coverSprite.texture = this._breakingAnim.currentFrame;
        if (this._breakingAnim.isFinished) {
          this._breakState = BreakState.Broken;
          this._coverSprite.visible = false;
          this._brokenSprite.visible = true;
          this.resetTriggerState();
        }
        break;

      case BreakState.Broken:
        this._brokenSprite.visible = true;
        this._holeTrap.setVisible(true);
        this._holeTrap.update(deltaMS);
        break;
    }
  }

  override destroy(): void {
    this._holeTrap.destroy();
    this._container.removeChild(this._coverSprite);
    this._container.removeChild(this._brokenSprite);
    this._coverSprite.destroy();
    this._brokenSprite.destroy();
  }
}
