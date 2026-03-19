import { Container, Sprite, Texture } from 'pixi.js';
import { LinkDirection, PillarPositions, WallTypes } from './enums';
import type { IWall } from './IWall';
import type { Pillar } from './Pillar';
import type { CellData } from './GridTypes';
import type { LevelConfig } from './LevelConfig';
import type { LevelCustomization } from './LevelCustomization';
import { createWall } from './WallFactory';

export const CELL_SIZE = 192;

export class GameCell {
    readonly col: number;
    readonly row: number;
    readonly x: number;
    readonly y: number;
    readonly centerX: number;
    readonly centerY: number;

    groundSprite: Sprite | null = null;

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

    setGroundSprite(texture: Texture, zIndex: number, yOffset = 0): void {
        this.groundSprite = new Sprite(texture);
        this.groundSprite.x = this.x;
        this.groundSprite.y = this.y + yOffset;
        this.groundSprite.zIndex = zIndex;
    }

    initializeWalls(data: CellData, config: LevelConfig, customization: LevelCustomization): void {
        const directions: [LinkDirection, WallTypes][] = [
            [LinkDirection.NorthLink, data.top],
            [LinkDirection.EastLink, data.right],
            [LinkDirection.SouthLink, data.bottom],
            [LinkDirection.WestLink, data.left],
        ];

        for (const [dir, wallType] of directions) {
            if (wallType === WallTypes.Niente) continue;

            const wall = createWall(wallType, dir, this.x, this.y, config, customization);
            if (!wall) continue;

            this.walls.set(dir, wall);
            this._applyWallPassability(dir, wallType);
        }
    }

    private _applyWallPassability(dir: LinkDirection, wallType: WallTypes): void {
        // Porta (doors) are passable — their open/closed state is checked at runtime
        const blocking = wallType !== WallTypes.Niente && wallType !== WallTypes.Porta;
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

    /** Adds only the ground sprite. Walls and pillars are added separately by GameGrid after deduplication. */
    addToContainer(container: Container): void {
        if (this.groundSprite) container.addChild(this.groundSprite);
    }

    setVisible(visible: boolean): void {
        if (this.groundSprite) this.groundSprite.visible = visible;
    }

    updateWalls(deltaMS: number): void {
        for (const wall of this.walls.values()) wall.update(deltaMS);
    }
}
