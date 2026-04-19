import { Container, Sprite, Texture } from "pixi.js";
import { GameCell } from "../grid/GameCell";
import { zForCharacter } from "../grid/ZIndexCalculator";

export class PassableObject {
  protected readonly _container: Container;
  protected readonly _sprite: Sprite;

  constructor(texture: Texture, gameCell: GameCell, container: Container) {
    this._container = container;
    this._sprite = new Sprite(texture);
    this._sprite.anchor.set(0.5);
    this._sprite.x = gameCell.centerX;
    this._sprite.y = gameCell.centerY;
    this._sprite.zIndex = zForCharacter(gameCell.centerY);
    container.addChild(this._sprite);
  }

  destroy(): void {
    this._container.removeChild(this._sprite);
    this._sprite.destroy();
  }
}
