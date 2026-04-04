import type { CellData, GridBundle } from '../../grid/GridTypes';
import { serializeMark } from '../../grid/GridSerializer';
import { EditorAction, SINGLETON_OBJECTS, isRiempimento } from './editorTypes';

// Posizione muro (speculare a WallPositions enum)
export const WP_TOP    = 0;
export const WP_RIGHT  = 1;
export const WP_LEFT   = 2;
export const WP_BOTTOM = 3;

// WallTypes usati per la validazione
const WT_INGRESSO = 5;
const WT_USCITA   = 6;

export interface EditorState {
    grid: GridBundle;
    currentAction: EditorAction;
    selectedGround: number;
    selectedWall: number;
    selectedObject: number;
    hoveredCell: { col: number; row: number } | null;
    lastSavedSnapshot: string;
    currentFilename: string;
    singletonPlaced: Set<number>;
}

// ---------------------------------------------------------------------------
// Factory
// ---------------------------------------------------------------------------

export function createEmptyState(): EditorState {
    const ROWS = 25;
    const COLS = 45;
    const grid: (CellData | null)[][] = Array.from(
        { length: COLS },
        () => new Array<CellData | null>(ROWS).fill(null)
    );
    const bundle: GridBundle = {
        rows: ROWS,
        columns: COLS,
        envTexturePackId: 1,
        dirtyProbabilityPct: 0,
        grid,
    };
    return {
        grid: bundle,
        currentAction: EditorAction.Ground,
        selectedGround: 1,   // Terra1
        selectedWall: 1,     // Occludente
        selectedObject: 0,   // Cancella
        hoveredCell: null,
        lastSavedSnapshot: serializeMark(bundle),
        currentFilename: 'nuovo-livello.mark',
        singletonPlaced: new Set(),
    };
}

export function createStateFromBundle(bundle: GridBundle, filename: string): EditorState {
    const singletonPlaced = new Set<number>();
    for (let col = 0; col < bundle.columns; col++) {
        for (let row = 0; row < bundle.rows; row++) {
            const cell = bundle.grid[col]?.[row];
            if (cell?.objectType != null && SINGLETON_OBJECTS.has(cell.objectType)) {
                singletonPlaced.add(cell.objectType);
            }
        }
    }
    return {
        grid: bundle,
        currentAction: EditorAction.Ground,
        selectedGround: 1,
        selectedWall: 1,
        selectedObject: 0,
        hoveredCell: null,
        lastSavedSnapshot: serializeMark(bundle),
        currentFilename: filename,
        singletonPlaced,
    };
}

// ---------------------------------------------------------------------------
// Cell helpers
// ---------------------------------------------------------------------------

export function ensureCell(grid: GridBundle, col: number, row: number): CellData {
    let cell = grid.grid[col]?.[row] ?? null;
    if (!cell) {
        cell = {
            groundType: 0,
            objectType: null,
            objectAdditionalData: '',
            top: 0,
            right: 0,
            bottom: 0,
            left: 0,
            stringCode: '',
        };
        grid.grid[col]![row] = cell;
    }
    return cell;
}

// ---------------------------------------------------------------------------
// Mutazioni
// ---------------------------------------------------------------------------

export function setGroundType(state: EditorState, col: number, row: number, type: number): void {
    const cell = ensureCell(state.grid, col, row);
    cell.groundType = type;
    // Se riempimento, cancella anche l'oggetto sulla cella (come nel C#)
    if (isRiempimento(type)) {
        if (cell.objectType !== null && SINGLETON_OBJECTS.has(cell.objectType)) {
            state.singletonPlaced.delete(cell.objectType);
        }
        cell.objectType = null;
        cell.objectAdditionalData = '';
        cell.stringCode = '';
    }
}

/** Validazione muri: Left/Right non ammettono Ingresso/Uscita; Bottom non ammette Ingresso/Uscita. */
function isWallPositionValid(wallType: number, wallPosition: number): boolean {
    if (wallType === WT_INGRESSO || wallType === WT_USCITA) {
        // Solo Top è ammesso per Ingresso/Uscita
        if (wallPosition === WP_LEFT || wallPosition === WP_RIGHT || wallPosition === WP_BOTTOM) {
            return false;
        }
    }
    return true;
}

export function setWallType(
    state: EditorState,
    col: number,
    row: number,
    wallType: number,
    wallPosition: number
): boolean {
    if (!isWallPositionValid(wallType, wallPosition)) {
        return false;
    }
    const cell = ensureCell(state.grid, col, row);
    switch (wallPosition) {
        case WP_TOP:    cell.top    = wallType; break;
        case WP_RIGHT:  cell.right  = wallType; break;
        case WP_LEFT:   cell.left   = wallType; break;
        case WP_BOTTOM: cell.bottom = wallType; break;
    }
    return true;
}

export function setObjectType(
    state: EditorState,
    col: number,
    row: number,
    objectType: number | null,
    additionalData: string = ''
): void {
    const cell = ensureCell(state.grid, col, row);

    // Rimuovi il vecchio singleton tracking
    if (cell.objectType !== null && SINGLETON_OBJECTS.has(cell.objectType)) {
        state.singletonPlaced.delete(cell.objectType);
    }

    if (objectType === null || objectType === 0) {
        // Cancella oggetto
        cell.objectType = null;
        cell.objectAdditionalData = '';
    } else {
        // Auto-set Terra1 se la cella è NonCella
        if (cell.groundType === 0) {
            cell.groundType = 1;
        }
        // Se riempimento, converti a Terra1
        if (isRiempimento(cell.groundType)) {
            cell.groundType = 1;
        }
        cell.objectType = objectType;
        cell.objectAdditionalData = additionalData;

        // Tracking singleton
        if (SINGLETON_OBJECTS.has(objectType)) {
            state.singletonPlaced.add(objectType);
        }
    }
}

export function setStringCode(state: EditorState, col: number, row: number, code: string): void {
    const cell = ensureCell(state.grid, col, row);
    cell.stringCode = code;
    // Auto-set Terra1 se la cella è NonCella
    if (cell.groundType === 0) {
        cell.groundType = 1;
    }
}

export function isDirty(state: EditorState): boolean {
    return serializeMark(state.grid) !== state.lastSavedSnapshot;
}

export function markSaved(state: EditorState): void {
    state.lastSavedSnapshot = serializeMark(state.grid);
}
