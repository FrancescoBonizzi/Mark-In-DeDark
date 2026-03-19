import { LinkDirection, PillarPositions } from './enums';
import { Pillar } from './Pillar';
import type { GameCell } from './GameCell';
import type { LevelConfig } from './LevelConfig';
import type { CollisionOffset } from './LevelCustomization';

const CELL_SIZE = 192;
/** Y offset from cell top to north wall centre (C# NorthWallYOffset) */
const NORTH_WALL_Y = -96;
/** Y offset from cell top to south wall centre (C# SouthWallYOffset) */
const SOUTH_WALL_Y = 96;

function pickRandom<T>(arr: T[]): T | null {
    if (arr.length === 0) return null;
    return arr[Math.floor(Math.random() * arr.length)]!;
}

/**
 * Generates pillars for a cell — faithful port of C# PillarFactory.GeneratePillars().
 * Must be called after _buildGraph() so links and passability flags are final.
 *
 * Uses cell.links for neighbour access (mirrors C# Links[]) so that unlinked /
 * missing neighbours yield null and skip their rule groups.
 *
 * Positions match C# PillarFactory:
 *   NW: (cellX - pillarWidth/2, cellY + NorthWallYOffset)
 *   NE: (cellX + cellWidth - pillarWidth/2, cellY + NorthWallYOffset)
 *   SW: (cellX - pillarWidth/2, cellY + SouthWallYOffset)
 *   SE: (cellX + cellWidth - pillarWidth/2, cellY + SouthWallYOffset)
 */
export function createPillarsForCell(
    col: number,
    row: number,
    cell: GameCell,
    _cells: (GameCell | null)[][],
    config: LevelConfig,
    pillarOffset: CollisionOffset,
): Pillar[] {
    if (config.pillarTextures.length === 0) return [];

    const firstTex = config.pillarTextures[0]!;
    const pillarW = firstTex.width;
    const cellX = col * CELL_SIZE;
    const cellY = row * CELL_SIZE;

    const westEdgeX = cellX - Math.floor(pillarW / 2);
    const eastEdgeX = cellX + CELL_SIZE - Math.floor(pillarW / 2);

    const added = new Set<PillarPositions>();
    const pillars: Pillar[] = [];

    const addPillar = (pos: PillarPositions, px: number, py: number): void => {
        if (added.has(pos)) return;
        added.add(pos);
        const tex = pickRandom(config.pillarTextures);
        if (!tex) return;
        pillars.push(new Pillar(pos, px, py, tex, pillarOffset));
    };

    const addNE = (): void => addPillar(PillarPositions.NE, eastEdgeX, cellY + NORTH_WALL_Y);
    const addNW = (): void => addPillar(PillarPositions.NW, westEdgeX, cellY + NORTH_WALL_Y);
    const addSE = (): void => addPillar(PillarPositions.SE, eastEdgeX, cellY + SOUTH_WALL_Y);
    const addSW = (): void => addPillar(PillarPositions.SW, westEdgeX, cellY + SOUTH_WALL_Y);

    const north = cell.links.get(LinkDirection.NorthLink) ?? null;
    const south = cell.links.get(LinkDirection.SouthLink) ?? null;
    const east  = cell.links.get(LinkDirection.EastLink)  ?? null;
    const west  = cell.links.get(LinkDirection.WestLink)  ?? null;

    // Rule 1: cell.east blocked + north exists + north's east exists + NE's south blocked → NE
    if (!cell.eastPassable && north) {
        const ne = north.links.get(LinkDirection.EastLink) ?? null;
        if (ne && !ne.southPassable) addNE();
    }

    // Rule 2: cell.west blocked + north exists + north's west exists + NW's south blocked → NW
    if (!cell.westPassable && north) {
        const nw = north.links.get(LinkDirection.WestLink) ?? null;
        if (nw && !nw.southPassable) addNW();
    }

    // Rule 3: north's east blocked + east's north blocked → NE
    if (north && !north.eastPassable && east && !east.northPassable) addNE();

    // Rule 4: north's west blocked + west's north blocked → NW
    if (north && !north.westPassable && west && !west.northPassable) addNW();

    // Rule 5: north blocked + east exists + east.north passable/destroyable + door guards → NE
    if (!cell.northPassable && east) {
        if (
            !east.isNorthDoor &&
            (east.northPassable || east.northDestroyable) &&
            (!cell.northDestroyable || !east.northDestroyable) &&
            east.links.get(LinkDirection.NorthLink)?.isSouthDoor === false
        ) {
            addNE();
        }
    }

    // Rule 6: north blocked + west exists + west.north passable/destroyable + door guards → NW
    if (!cell.northPassable && west) {
        if (
            !west.isNorthDoor &&
            (west.northPassable || west.northDestroyable) &&
            (!cell.northDestroyable || !west.northDestroyable) &&
            west.links.get(LinkDirection.NorthLink)?.isSouthDoor === false
        ) {
            addNW();
        }
    }

    // Rule 7: east blocked + north exists + north.east passable/destroyable + guard → NE
    if (!cell.eastPassable && north) {
        if (
            (north.eastPassable || north.eastDestroyable) &&
            (!cell.eastDestroyable || !north.eastDestroyable)
        ) {
            addNE();
        }
    }

    // Rule 8a: east blocked + south exists + south.east passable/destroyable + guard → SE
    if (!cell.eastPassable && south) {
        if (
            (south.eastPassable || south.eastDestroyable) &&
            (!cell.eastDestroyable || !south.eastDestroyable)
        ) {
            addSE();
        }
    }

    // Rule 8b: west blocked + south exists + south.west passable/destroyable + guard → SW
    if (!cell.westPassable && south) {
        if (
            (south.westPassable || south.westDestroyable) &&
            (!cell.westDestroyable || !south.westDestroyable)
        ) {
            addSW();
        }
    }

    // Rule 9: two walls meeting on the same cell corners
    if (!cell.northPassable && !cell.eastPassable) addNE();
    if (!cell.northPassable && !cell.westPassable) addNW();
    if (!cell.southPassable && !cell.eastPassable) addSE();
    if (!cell.southPassable && !cell.westPassable) addSW();

    return pillars;
}
