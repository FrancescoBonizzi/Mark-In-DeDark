import type { IWall } from './IWall';
import type { Pillar } from './Pillar';
import { WallBreakable } from './WallBreakable';

/**
 * Removes duplicate walls that share the same integer position.
 * When duplicates are found, keeps WallBreakable over WallFixed.
 */
export function removeDuplicateWalls(walls: IWall[]): IWall[] {
    const seen = new Map<string, IWall>();

    for (const wall of walls) {
        const key = `${Math.round(wall.position.x)},${Math.round(wall.position.y)}`;
        const existing = seen.get(key);
        if (!existing) {
            seen.set(key, wall);
        } else if (wall instanceof WallBreakable) {
            seen.set(key, wall);
        }
    }

    return Array.from(seen.values());
}

/**
 * Removes duplicate pillars at the same integer position.
 */
export function removeDuplicatePillars(pillars: Pillar[]): Pillar[] {
    const seen = new Map<string, Pillar>();
    for (const pillar of pillars) {
        const key = `${Math.round(pillar.x)},${Math.round(pillar.y)}`;
        if (!seen.has(key)) {
            seen.set(key, pillar);
        }
    }
    return Array.from(seen.values());
}
