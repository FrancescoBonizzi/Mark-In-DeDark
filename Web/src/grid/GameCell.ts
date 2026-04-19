import { Container, Sprite, Texture } from "pixi.js";
import { CellRelativePosition } from "./CellRelativePosition";
import { LinkDirection, PillarPositions, WallTypes } from "./enums";
import type { CellData } from "./GridTypes";
import type { IWall } from "./IWall";
import type { LevelConfig } from "./LevelConfig";
import type { LevelCustomization } from "./LevelCustomization";
import type { Pillar } from "./Pillar";
import { createWall } from "./WallFactory";
import { Z_GROUND_BLOOD, Z_GROUND_DIRTY } from "./ZIndexCalculator";

export const CELL_SIZE = 192;

export class GameCell {
  readonly col: number;
  readonly row: number;
  readonly x: number;
  readonly y: number;
  readonly centerX: number;
  readonly centerY: number;

  groundSprite: Sprite | null = null;
  groundNormalSprite: Sprite | null = null;
  groundSpecularSprite: Sprite | null = null;
  dirtySprite: Sprite | null = null;
  private _bloodSprite: Sprite | null = null;
  private _groundContainer: Container | undefined;

  /** Neighbour cells by direction */
  links: Map<LinkDirection, GameCell | null> = new Map([
    [LinkDirection.NorthLink, null],
    [LinkDirection.EastLink, null],
    [LinkDirection.SouthLink, null],
    [LinkDirection.WestLink, null],
  ]);

  walls: Map<LinkDirection, IWall> = new Map();
  pillars: Map<PillarPositions, Pillar> = new Map();

  /** Whether each side is passable (no blocking wall) */
  northPassable = true;
  eastPassable = true;
  southPassable = true;
  westPassable = true;

  /** Whether each side's wall is destroyable */
  northDestroyable = false;
  eastDestroyable = false;
  southDestroyable = false;
  westDestroyable = false;

  noDecals = false;
  isUnwalkable = false;
  isDoor = false;
  isNorthDoor = false;
  isSouthDoor = false;
  isEntrance = false;
  isExit = false;
  /** Not part of pathfinding graph (Buio, Riempimento) */
  isUnLinked = false;

  constructor(col: number, row: number) {
    this.col = col;
    this.row = row;
    this.x = col * CELL_SIZE;
    this.y = row * CELL_SIZE;
    this.centerX = this.x + CELL_SIZE / 2;
    this.centerY = this.y + CELL_SIZE / 2;
  }

  setDirtySprite(texture: Texture): void {
    const ROTATIONS = [0, Math.PI / 2, Math.PI, (3 * Math.PI) / 2];
    const sprite = new Sprite(texture);
    sprite.anchor.set(0.5, 0.5);
    sprite.x =
      this.x +
      CELL_SIZE / 2 +
      (Math.random() - 0.5) * (CELL_SIZE - texture.width);
    sprite.y =
      this.y +
      CELL_SIZE / 2 +
      (Math.random() - 0.5) * (CELL_SIZE - texture.height);
    sprite.rotation = ROTATIONS[Math.floor(Math.random() * ROTATIONS.length)]!;
    sprite.zIndex = Z_GROUND_DIRTY;
    this.dirtySprite = sprite;
  }

  /** Port of C# GameCell.ApplyBloodDecal — sparge macchia di sangue sulla cella. */
  applyBloodDecal(position: CellRelativePosition, bloodTexture: Texture): void {
    if (this.isUnLinked || this.noDecals) return;

    const randomDelta = 2 + Math.random(); // C#: Numbers.RandomBetween(2f, 3f)
    let decalX = this.centerX;
    let decalY = this.centerY;

    switch (position) {
      case CellRelativePosition.Up:
        decalY += CELL_SIZE / randomDelta;
        break;
      case CellRelativePosition.Down:
        decalY -= CELL_SIZE / randomDelta;
        break;
      case CellRelativePosition.Left:
        decalX += CELL_SIZE / randomDelta;
        break;
      case CellRelativePosition.Right:
        decalX -= CELL_SIZE / randomDelta;
        break;
      case CellRelativePosition.UpLeft:
        decalX += CELL_SIZE / randomDelta;
        decalY += CELL_SIZE / randomDelta;
        break;
      case CellRelativePosition.UpRight:
        decalX -= CELL_SIZE / randomDelta;
        decalY += CELL_SIZE / randomDelta;
        break;
      case CellRelativePosition.DownLeft:
        decalX += CELL_SIZE / randomDelta;
        decalY -= CELL_SIZE / randomDelta;
        break;
      case CellRelativePosition.DownRight:
        decalX -= CELL_SIZE / randomDelta;
        decalY -= CELL_SIZE / randomDelta;
        break;
      case CellRelativePosition.Center:
        break;
    }

    const sprite = new Sprite(bloodTexture);
    sprite.anchor.set(0.5, 0.5);
    sprite.x = decalX;
    sprite.y = decalY;
    sprite.rotation = Math.random() * 20; // C#: Numbers.RandomBetween(0f, 20f)
    sprite.scale.set(0.3 + Math.random() * 0.7); // C#: Numbers.RandomBetween(0.3f, 1f)
    sprite.zIndex = Z_GROUND_BLOOD;
    this._bloodSprite = sprite;
    this._groundContainer?.addChild(sprite);
  }

  setGroundSprite(texture: Texture, zIndex: number, yOffset = 0): void {
    this.groundSprite = new Sprite(texture);
    this.groundSprite.x = this.x;
    this.groundSprite.y = this.y + yOffset;
    this.groundSprite.zIndex = zIndex;
  }

  setGroundNormalSprite(texture: Texture, zIndex: number, yOffset = 0): void {
    this.groundNormalSprite = new Sprite(texture);
    this.groundNormalSprite.x = this.x;
    this.groundNormalSprite.y = this.y + yOffset;
    this.groundNormalSprite.zIndex = zIndex;
  }

  setGroundSpecularSprite(texture: Texture, zIndex: number, yOffset = 0): void {
    this.groundSpecularSprite = new Sprite(texture);
    this.groundSpecularSprite.x = this.x;
    this.groundSpecularSprite.y = this.y + yOffset;
    this.groundSpecularSprite.zIndex = zIndex;
  }

  initializeWalls(
    data: CellData,
    config: LevelConfig,
    customization: LevelCustomization,
  ): void {
    const directions: [LinkDirection, WallTypes][] = [
      [LinkDirection.NorthLink, data.top],
      [LinkDirection.EastLink, data.right],
      [LinkDirection.SouthLink, data.bottom],
      [LinkDirection.WestLink, data.left],
    ];

    for (const [dir, wallType] of directions) {
      if (wallType === WallTypes.Niente) continue;

      const wall = createWall(
        wallType,
        dir,
        this.x,
        this.y,
        config,
        customization,
      );
      if (!wall) continue;

      this.walls.set(dir, wall);
      this._applyWallPassability(dir, wallType);
    }
  }

  private _applyWallPassability(dir: LinkDirection, wallType: WallTypes): void {
    // Porta (doors) are passable — their open/closed state is checked at runtime
    const blocking =
      wallType !== WallTypes.Niente && wallType !== WallTypes.Porta;
    const destroyable = wallType === WallTypes.Fragile;
    const isDoor = wallType === WallTypes.Porta;
    const isEntrance = wallType === WallTypes.Ingresso;
    const isExit = wallType === WallTypes.Uscita;

    if (isEntrance) this.isEntrance = true;
    if (isExit) this.isExit = true;
    if (isDoor) {
      this.isDoor = true;
      if (dir === LinkDirection.NorthLink) this.isNorthDoor = true;
      if (dir === LinkDirection.SouthLink) this.isSouthDoor = true;
    }

    switch (dir) {
      case LinkDirection.NorthLink:
        this.northPassable = !blocking;
        this.northDestroyable = destroyable;
        break;
      case LinkDirection.EastLink:
        this.eastPassable = !blocking;
        this.eastDestroyable = destroyable;
        break;
      case LinkDirection.SouthLink:
        this.southPassable = !blocking;
        this.southDestroyable = destroyable;
        break;
      case LinkDirection.WestLink:
        this.westPassable = !blocking;
        this.westDestroyable = destroyable;
        break;
    }
  }

  /**
   * Adds ground/dirty sprites to the appropriate layer.
   * Sprites with zIndex >= 1 (Riempimento fill blocks) go into objectsContainer
   * so they sort correctly with walls and pillars. Pure ground tiles go into
   * groundContainer which is rendered separately for the lighting pass.
   */
  addToContainer(
    groundContainer: Container,
    objectsContainer: Container,
    groundNormalContainer: Container,
    objectsNormalContainer: Container,
    groundSpecularContainer: Container,
  ): void {
    this._groundContainer = groundContainer;
    if (this.groundSprite) {
      if (this.groundSprite.zIndex >= 1) {
        objectsContainer.addChild(this.groundSprite);
        if (this.groundNormalSprite)
          objectsNormalContainer.addChild(this.groundNormalSprite);
      } else {
        groundContainer.addChild(this.groundSprite);
        if (this.groundNormalSprite)
          groundNormalContainer.addChild(this.groundNormalSprite);
        if (this.groundSpecularSprite)
          groundSpecularContainer.addChild(this.groundSpecularSprite);
      }
    }
    if (this.dirtySprite) groundContainer.addChild(this.dirtySprite);
  }

  setVisible(visible: boolean): void {
    if (this.groundSprite) this.groundSprite.visible = visible;
    if (this.dirtySprite) this.dirtySprite.visible = visible;
    if (this._bloodSprite) this._bloodSprite.visible = visible;
  }

  updateWalls(deltaMS: number): void {
    for (const wall of this.walls.values()) wall.update(deltaMS);
  }
}
