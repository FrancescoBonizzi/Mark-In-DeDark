import { Container, Rectangle, Sprite, Spritesheet } from "pixi.js";
import { getTexture } from "../assets/AssetsLoader";
import { Character } from "../characters/Character";
import { Z_GROUND_OBJECTS } from "../grid/ZIndexCalculator";
import { TriggerObject } from "./TriggerObject";

/** Hidden trigger that fires a callback once (goblin spawn — port of C# GoblinTriggerTrap.cs). */
export class GoblinTriggerTrap extends TriggerObject {
  private readonly _sprite: Sprite;
  private readonly _onFired: () => void;
  private readonly _container: Container;

  constructor(
    sheets: Spritesheet[],
    cellX: number,
    cellY: number,
    cellSize: number,
    container: Container,
    onFired: () => void,
    affectedCharacters: Character[] = [],
  ) {
    const cx = cellX + cellSize / 2;
    const cy = cellY + cellSize / 2;
    super(new Rectangle(cx - 70, cy - 70, 140, 140), true, affectedCharacters);

    this._onFired = onFired;
    this._container = container;

    const tex = getTexture(sheets, "Grounds/goblin_trigger");
    this._sprite = new Sprite(tex);
    this._sprite.anchor.set(0.5);
    this._sprite.x = cx;
    this._sprite.y = cy;
    this._sprite.zIndex = Z_GROUND_OBJECTS;
    container.addChild(this._sprite);
  }

  protected _onTrigger(_character: Character, _deltaMS: number): void {
    this._onFired();
    // Fade out the trigger sprite
    this._sprite.alpha = 0;
  }

  override update(deltaMS: number): void {
    super.update(deltaMS);
  }

  destroy(): void {
    this._container.removeChild(this._sprite);
    this._sprite.destroy();
  }
}
