import type { CellData, GridBundle } from './GridTypes';

/**
 * Serializza un GridBundle nel formato .mark (GameGridSerializerV4).
 * Itera row 0..rows-1, col 0..cols-1, salta celle null o con groundType === 0.
 */
export function serializeMark(bundle: GridBundle): string {
    const lines: string[] = [
        'GameGridSerializerV4',
        `${bundle.rows}|${bundle.columns}`,
        `${bundle.envTexturePackId}|${bundle.dirtyProbabilityPct}`,
    ];

    for (let row = 0; row < bundle.rows; row++) {
        for (let col = 0; col < bundle.columns; col++) {
            const cell: CellData | null = bundle.grid[col]?.[row] ?? null;
            if (!cell || cell.groundType === 0) continue;

            const objStr = cell.objectType !== null ? String(cell.objectType) : '';
            const line = [
                row,
                col,
                cell.groundType,
                objStr,
                cell.objectAdditionalData,
                cell.top,
                cell.right,
                cell.bottom,
                cell.left,
                cell.stringCode,
            ].join('|');

            lines.push(line);
        }
    }

    return lines.join('\n') + '\n';
}
