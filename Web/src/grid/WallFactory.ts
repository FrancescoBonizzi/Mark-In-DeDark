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

function pickRandomWithIndex<T>(arr: T[]): { item: T; index: number } {
    const index = Math.floor(Math.random() * arr.length);
    return { item: arr[index]!, index };
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

function pickWallTextureWithIndex(direction: LinkDirection, config: LevelConfig): { texture: Texture; normalTexture: Texture | null } {
    if (isVertical(direction)) {
        if (config.randomVerticalWalls.length > 0) {
            const { item, index } = pickRandomWithIndex(config.randomVerticalWalls);
            return { texture: item, normalTexture: config.randomVerticalWallsNormal[index] ?? null };
        }
        return { texture: config.randomHorizontalWalls[0] ?? config.darkCellTexture, normalTexture: null };
    }
    if (config.randomHorizontalWalls.length > 0) {
        const { item, index } = pickRandomWithIndex(config.randomHorizontalWalls);
        return { texture: item, normalTexture: config.randomHorizontalWallsNormal[index] ?? null };
    }
    return { texture: config.darkCellTexture, normalTexture: null };
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
    return config.specialWalls[index] ?? pickWallTextureWithIndex(LinkDirection.NorthLink, config).texture;
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

    // C# treats all east/west walls (except Fragile) as plain random vertical walls
    if (vert && wallType !== WallTypes.Fragile) {
        const { texture: tex, normalTexture } = pickWallTextureWithIndex(direction, config);
        const pos = wallSpritePos(direction, cellX, cellY, tex.width);
        return new WallFixed(direction, pos.x, pos.y, tex, offset, undefined, normalTexture ?? undefined);
    }

    switch (wallType) {
        case WallTypes.Occludente:
        case WallTypes.Muro1:
        case WallTypes.Muro2:
        case WallTypes.Muro3:
        case WallTypes.Muro4:
        case WallTypes.Muro5: {
            const { texture: tex, normalTexture } = wallType === WallTypes.Occludente
                ? pickWallTextureWithIndex(direction, config)
                : { texture: pickSpecialWallTexture(wallType, config), normalTexture: null };
            const pos = wallSpritePos(direction, cellX, cellY, tex.width);
            return new WallFixed(direction, pos.x, pos.y, tex, offset, undefined, normalTexture ?? undefined);
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
            return new WallInteractiveDoor(direction, pos.x, pos.y, closedTex, config.doorFrames, config.wallDoorUp ?? undefined);
        }

        case WallTypes.Ingresso: {
            const tex = config.ingressoClosed;
            if (!tex) return null;
            const pos = wallSpritePos(direction, cellX, cellY, tex.width);
            pos.x = cellX + CELL_SIZE / 2 - tex.width / 2;
            const arcTex = config.ingressoUp ?? undefined;
            const arcX = arcTex ? cellX + CELL_SIZE / 2 - arcTex.width / 2 : undefined;
            return new WallEsteticDoor(direction, pos.x, pos.y, tex, config.ingressoOpen ?? undefined, arcTex, arcX);
        }

        case WallTypes.Uscita: {
            const tex = config.uscitaClosed;
            if (!tex) return null;
            const pos = wallSpritePos(direction, cellX, cellY, tex.width);
            pos.x = cellX + CELL_SIZE / 2 - tex.width / 2;
            const arcTex = config.uscitaUp ?? undefined;
            const arcX = arcTex ? cellX + CELL_SIZE / 2 - arcTex.width / 2 : undefined;
            return new WallEsteticDoor(direction, pos.x, pos.y, tex, config.uscitaOpen ?? undefined, arcTex, arcX);
        }

        case WallTypes.Decal1:
        case WallTypes.Decal2:
        case WallTypes.Decal3:
        case WallTypes.Decal4:
        case WallTypes.Decal5:
        case WallTypes.Decal6:
        case WallTypes.Decal7:
        case WallTypes.Decal8: {
            const { texture: baseTex, normalTexture } = pickWallTextureWithIndex(direction, config);
            const pos = wallSpritePos(direction, cellX, cellY, baseTex.width);
            const decalTex = config.decalTextures[decalIndex(wallType)] ?? undefined;
            return new WallFixed(direction, pos.x, pos.y, baseTex, offset, decalTex, normalTexture ?? undefined);
        }

        default:
            return null;
    }
}
