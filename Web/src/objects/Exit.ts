import { Rectangle } from "pixi.js";
import { GameGrid } from "../grid/GameGrid";
import { WallEsteticDoor } from "../grid/WallEsteticDoor";
import { MarkLight } from "../lighting/MarkLight";

/** The level exit zone (port of C# Exit.cs). */
export class Exit {
  readonly collisionRectangle: Rectangle;
  readonly surroundingLight: MarkLight;

  private readonly _exitDoor: WallEsteticDoor | null;
  private readonly _grid: GameGrid;
  private _isOpen = false;

  constructor(
    centerX: number,
    centerY: number,
    exitDoor: WallEsteticDoor | null,
    grid: GameGrid,
  ) {
    this._exitDoor = exitDoor;
    this._grid = grid;
    // HitBoxTolerance(60, 60, 120, 120)
    this.collisionRectangle = new Rectangle(
      centerX - 60,
      centerY - 60,
      120,
      120,
    );

    this.surroundingLight = new MarkLight({
      x: centerX,
      y: centerY,
      z: 31,
      opacity: 0.3,
      radius: 80,
      castsShadows: false,
      color1: 0x55ddcc,
      color2: 0x33aaaa,
      distanceSquared: 2_200,
      distanceSquaredForWall: 7_000,
    });
    this.surroundingLight.lightMaskScale = 0.9;
    // La porta è sul bordo alto della cella; spostiamo la luce verso l'alto
    this.surroundingLight.deltaForWall = { x: 0, y: -110 };
    this.surroundingLight.deltaForGround = { x: 0, y: -20 };
  }

  get isOpen(): boolean {
    return this._isOpen;
  }

  setOpen(): void {
    if (this._isOpen) return;
    this._isOpen = true;
    this._exitDoor?.setOpen();
    this._grid.openExitPassage();
  }
}
