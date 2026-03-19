import { Texture } from 'pixi.js';
import { LinkDirection, WallTypes } from './enums';
import type { IWall } from './IWall';
import type { LevelConfig } from './LevelConfig';
import type { LevelCustomization } from './LevelCustomization';
import { WallFixed } from './WallFixed';
import { WallBreakable } from './WallBreakable';
import { WallEsteticDoor } from './WallEsteticDoor';
import { WallInteractiveDoor } from './WallInteractiveDoor';

const CELL_SIZE = 192;

function pickRandom<T>(arr: T[]): T {
    return arr[Math.floor(Math.random() * arr.length)]!;
}

/**
 * Compute wall sprite position relative to the cell's top-left corner (cellX, cellY).
 * Returns { x, y } for the sprite's top-left (anchor 0,0).
 */
function wallSpritePos(
    direction: LinkDirection,
    cellX: number,
    cellY: number,
    spriteW: number,
): { x: number; y: number } {
    switch (direction) {
        case LinkDirection.NorthLink:
            return { x: cellX, y: cellY - CELL_SIZE / 2 };
        case LinkDirection.SouthLink:
            return { x: cellX, y: cellY + CELL_SIZE / 2 };
        case LinkDirection.EastLink:
            return { x: cellX + CELL_SIZE - spriteW / 2, y: cellY - CELL_SIZE / 4 };
        case LinkDirection.WestLink:
            return { x: cellX - spriteW / 2, y: cellY - CELL_SIZE / 4 };
    }
}

function isVertical(direction: LinkDirection): boolean {
    return direction === LinkDirection.EastLink || direction === LinkDirection.WestLink;
}

function pickWallTexture(direction: LinkDirection, config: LevelConfig): Texture {
    if (isVertical(direction)) {
        if (config.randomVerticalWalls.length > 0) return pickRandom(config.randomVerticalWalls);
        return config.randomHorizontalWalls[0] ?? config.darkCellTexture;
    }
    if (config.randomHorizontalWalls.length > 0) return pickRandom(config.randomHorizontalWalls);
    return config.darkCellTexture;
}

function pickSpecialWallTexture(wallType: WallTypes, config: LevelConfig): Texture {
    const index = (() => {
        switch (wallType) {
            case WallTypes.Muro1: return 0;
            case WallTypes.Muro2: return 1;
            case WallTypes.Muro3: return 2;
            case WallTypes.Muro4: return 3;
            case WallTypes.Muro5: return 4;
            default: return 0;
        }
    })();
    return config.specialWalls[index] ?? pickWallTexture(LinkDirection.NorthLink, config);
}

function decalIndex(wallType: WallTypes): number {
    switch (wallType) {
        case WallTypes.Decal1: return 0;
        case WallTypes.Decal2: return 1;
        case WallTypes.Decal3: return 2;
        case WallTypes.Decal4: return 3;
        case WallTypes.Decal5: return 4;
        case WallTypes.Decal6: return 5;
        case WallTypes.Decal7: return 6;
        case WallTypes.Decal8: return 7;
        default: return 0;
    }
}

export function createWall(
    wallType: WallTypes,
    direction: LinkDirection,
    cellX: number,
    cellY: number,
    config: LevelConfig,
    customization: LevelCustomization,
): IWall | null {
    if (wallType === WallTypes.Niente) return null;

    const vert = isVertical(direction);
    const offset = vert ? customization.wallFixedVerticalOffset : customization.wallFixedHorizontalOffset;

    switch (wallType) {
        case WallTypes.Occludente:
        case WallTypes.Muro1:
        case WallTypes.Muro2:
        case WallTypes.Muro3:
        case WallTypes.Muro4:
        case WallTypes.Muro5: {
            const tex = wallType === WallTypes.Occludente
                ? pickWallTexture(direction, config)
                : pickSpecialWallTexture(wallType, config);
            const pos = wallSpritePos(direction, cellX, cellY, tex.width);
            return new WallFixed(direction, pos.x, pos.y, tex, offset);
        }

        case WallTypes.Fragile: {
            const intactTex = vert ? config.wallVerticalBreakable : config.wallHorizontalBreakable;
            const brokenA = vert ? config.wallVerticalBrokenA : config.wallHorizontalBroken;
            const brokenB = vert ? config.wallVerticalBrokenB : null;
            const pos = wallSpritePos(direction, cellX, cellY, intactTex.width);
            return new WallBreakable(direction, pos.x, pos.y, intactTex, brokenA, brokenB, vert);
        }

        case WallTypes.Porta: {
            const closedTex = config.wallDoor;
            if (!closedTex) return null;
            const pos = wallSpritePos(direction, cellX, cellY, closedTex.width);
            return new WallInteractiveDoor(direction, pos.x, pos.y, closedTex, config.doorFrames);
        }

        case WallTypes.Ingresso: {
            const tex = config.ingressoClosed;
            if (!tex) return null;
            const pos = wallSpritePos(direction, cellX, cellY, tex.width);
            return new WallEsteticDoor(direction, pos.x, pos.y, tex);
        }

        case WallTypes.Uscita: {
            const tex = config.uscitaClosed;
            if (!tex) return null;
            const pos = wallSpritePos(direction, cellX, cellY, tex.width);
            return new WallEsteticDoor(direction, pos.x, pos.y, tex);
        }

        case WallTypes.Decal1:
        case WallTypes.Decal2:
        case WallTypes.Decal3:
        case WallTypes.Decal4:
        case WallTypes.Decal5:
        case WallTypes.Decal6:
        case WallTypes.Decal7:
        case WallTypes.Decal8: {
            const baseTex = pickWallTexture(direction, config);
            const pos = wallSpritePos(direction, cellX, cellY, baseTex.width);
            const decalTex = config.decalTextures[decalIndex(wallType)] ?? undefined;
            return new WallFixed(direction, pos.x, pos.y, baseTex, offset, decalTex);
        }

        default:
            return null;
    }
}
