import { WallTypes } from './enums';
import type { CellData, GridBundle } from './GridTypes';
import { loadLevelText } from '../assets/LevelLoader';

function parseWallType(s: string | undefined): WallTypes {
    if (s === undefined || s === '') return WallTypes.Niente;
    return Number(s) as WallTypes;
}

export function parseLevel(text: string): GridBundle {
    const lines = text.replace(/\r/g, '').trim().split('\n');

    if (lines[0]?.trim() !== 'GameGridSerializerV4') {
        throw new Error('Invalid level format: expected GameGridSerializerV4');
    }

    const headerParts = lines[1]!.split('|');
    const rows = Number(headerParts[0]);
    const cols = Number(headerParts[1]);

    const envParts = lines[2]!.split('|');
    const envTexturePackId = Number(envParts[0]);
    const dirtyProbabilityPct = Number(envParts[1]);

    // Initialize grid[col][row] with nulls
    const grid: (CellData | null)[][] = Array.from(
        { length: cols },
        () => new Array<CellData | null>(rows).fill(null)
    );

    for (let i = 3; i < lines.length; i++) {
        const line = lines[i]!.trim();
        if (!line) continue;

        const parts = line.split('|');
        const row = Number(parts[0]);
        const col = Number(parts[1]);

        if (col < 0 || col >= cols || row < 0 || row >= rows) continue;

        const groundType = Number(parts[2] ?? 0);
        const objectTypeRaw = parts[3];
        const objectType = (objectTypeRaw !== undefined && objectTypeRaw !== '')
            ? Number(objectTypeRaw)
            : null;
        const objectAdditionalData = parts[4] ?? '';
        const top = parseWallType(parts[5]);
        const right = parseWallType(parts[6]);
        const bottom = parseWallType(parts[7]);
        const left = parseWallType(parts[8]);
        const stringCode = parts[9] ?? '';

        grid[col]![row] = {
            groundType,
            objectType,
            objectAdditionalData,
            top,
            right,
            bottom,
            left,
            stringCode,
        };
    }

    return { rows, columns: cols, envTexturePackId, dirtyProbabilityPct, grid };
}

export async function loadAndParseLevel(folder: string, levelId: number): Promise<GridBundle> {
    const text = await loadLevelText(folder, levelId);
    return parseLevel(text);
}
