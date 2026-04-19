import { Container, Rectangle, Sprite, Spritesheet } from "pixi.js";
import { SpriteAnimation } from "../animation/SpriteAnimation";
import { getTexture } from "../assets/AssetsLoader";
import { Character } from "../characters/Character";
import { Z_GROUND_OBJECTS } from "../grid/ZIndexCalculator";
import { TriggerObject } from "./TriggerObject";

const FRAME_DURATION_MS = 60;

/** A hole trap that instantly kills characters that walk over it (port of C# InstantlyDieTrap.cs). */
export class InstantlyDieTrap extends TriggerObject {
  private readonly _sprite: Sprite;
  private readonly _animation: SpriteAnimation;
  private readonly _container: Container;
  private readonly _centerX: number;
  private readonly _centerY: number;
  private _isPlaying = false;

  constructor(
    sheets: Spritesheet[],
    cellX: number,
    cellY: number,
    cellSize: number,
    container: Container,
    affectedCharacters: Character[] = [],
  ) {
    // HitBoxTolerance(70, 70, 140, 140)
    const cx = cellX + cellSize / 2;
    const cy = cellY + cellSize / 2;
    super(new Rectangle(cx - 70, cy - 70, 140, 140), false, affectedCharacters);

    this._container = container;
    this._centerX = cx;
    this._centerY = cy;

    const frames = Array.from({ length: 10 }, (_, i) =>
      getTexture(sheets, `Grounds/trapHole/${i + 1}`),
    );
    this._animation = new SpriteAnimation(frames, [], FRAME_DURATION_MS, false);

    this._sprite = new Sprite(frames[0]!);
    this._sprite.anchor.set(0.5);
    this._sprite.x = cx;
    this._sprite.y = cy;
    this._sprite.zIndex = Z_GROUND_OBJECTS;
    container.addChild(this._sprite);
  }

  setVisible(visible: boolean): void {
    this._sprite.visible = visible;
  }

  protected _onTrigger(character: Character, _deltaMS: number): void {
    this._isPlaying = true;
    this._animation.reset();
    this._sprite.texture = this._animation.currentFrame;
    character.fallInTrap(this._centerX, this._centerY);
  }

  override update(deltaMS: number): void {
    super.update(deltaMS);

    if (this._isPlaying && !this._animation.isFinished) {
      this._animation.update(deltaMS);
      this._sprite.texture = this._animation.currentFrame;
      if (this._animation.isFinished) {
        this._isPlaying = false;
      }
    }
  }

  override destroy(): void {
    this._container.removeChild(this._sprite);
    this._sprite.destroy();
  }
}
