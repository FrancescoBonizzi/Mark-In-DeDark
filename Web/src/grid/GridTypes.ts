import type { WallTypes } from './enums';

export interface CellData {
    groundType: number;
    objectType: number | null;
    objectAdditionalData: string;
    top: WallTypes;
    right: WallTypes;
    bottom: WallTypes;
    left: WallTypes;
    stringCode: string;
}

export interface GridBundle {
    rows: number;
    columns: number;
    envTexturePackId: number;
    dirtyProbabilityPct: number;
    /** Indexed as grid[col][row] */
    grid: (CellData | null)[][];
}
