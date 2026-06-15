const GRID_MAX_HEIGHT = 4800; // 25 rows * 192px

export const Z_GROUND = 0;
export const Z_GROUND_DIRTY = 0.0000001;
export const Z_GROUND_BLOOD = 0.0000002;
export const Z_GROUND_OBJECTS = 0.0000003;
export const Z_SHADOWS = 0.0000004;
export const Z_OVERALL = 3;

export function zForWall(spriteCenterY: number): number {
    return 1 + Math.min(Math.max(spriteCenterY / GRID_MAX_HEIGHT, 0), 1);
}

export function zForWallDecal(wallZ: number): number {
    return wallZ + 0.00001;
}

export function zForPillar(spriteY: number): number {
    return 1 + Math.min(Math.max((spriteY + 96) / GRID_MAX_HEIGHT, 0), 1) + 0.001;
}

export function zForRiempimento(cellY: number): number {
    return 1 + Math.min(Math.max(cellY / GRID_MAX_HEIGHT, 0), 1) + 0.002;
}

export function zForCharacter(spriteCenterY: number): number {
    return 1 + Math.min(Math.max(spriteCenterY / GRID_MAX_HEIGHT, 0), 1) + 0.0005;
}
