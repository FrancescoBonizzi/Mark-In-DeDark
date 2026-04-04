import type { CellData } from '../../grid/GridTypes';
import type { EditorState } from './editorState';
import {
    GROUND_COLOR_MAP,
    WALL_COLOR_MAP,
    WALL_CODE_MAP,
    OBJECT_LABEL_MAP,
    isRiempimento,
} from './editorTypes';

// Dimensione cella in px (identica al C#)
export const CELL_SIZE = 30;

// ---------------------------------------------------------------------------
// Rendering griglia
// ---------------------------------------------------------------------------

/**
 * Crea l'intera griglia CSS e la monta nel container.
 * Ritorna la matrice cellElements[col][row].
 */
export function renderGrid(
    container: HTMLElement,
    state: EditorState
): HTMLElement[][] {
    const { rows, columns } = state.grid;

    // Wrapper con CSS grid
    const wrapper = document.createElement('div');
    wrapper.className = 'editor-grid';
    wrapper.style.gridTemplateColumns = `repeat(${columns}, ${CELL_SIZE}px)`;
    wrapper.style.gridTemplateRows    = `repeat(${rows}, ${CELL_SIZE}px)`;

    // cellElements[col][row]
    const cellElements: HTMLElement[][] = Array.from({ length: columns }, () =>
        new Array<HTMLElement>(rows)
    );

    // Le celle sono disposte per riga nel DOM (left-to-right, top-to-bottom)
    for (let row = 0; row < rows; row++) {
        for (let col = 0; col < columns; col++) {
            const el = createCellElement(col, row);
            const cellData = state.grid.grid[col]?.[row] ?? null;
            applyCellData(el, cellData);
            wrapper.appendChild(el);
            cellElements[col]![row] = el;
        }
    }

    container.innerHTML = '';
    container.appendChild(wrapper);
    return cellElements;
}

function createCellElement(col: number, row: number): HTMLElement {
    const el = document.createElement('div');
    el.className = 'editor-cell';
    el.dataset['col'] = String(col);
    el.dataset['row'] = String(row);
    return el;
}

/**
 * Aggiorna l'aspetto visivo di una singola cella.
 * Chiamata ogni volta che la cella cambia stato.
 */
export function updateCell(el: HTMLElement, cellData: CellData | null): void {
    applyCellData(el, cellData);
}

function applyCellData(el: HTMLElement, cellData: CellData | null): void {
    // Sfondo
    const groundType = cellData?.groundType ?? 0;
    el.style.backgroundColor = GROUND_COLOR_MAP[groundType] ?? '#000000';

    // Bordi muri
    applyWallBorders(el, cellData);

    // Contenuto interno
    el.innerHTML = '';
    if (!cellData) return;

    // Riempimento: mostra numero come label piccola
    if (isRiempimento(groundType)) {
        const rIdx = getRiempimentoIndex(groundType);
        const span = document.createElement('span');
        span.className = 'cell-fill-label';
        span.textContent = String(rIdx);
        el.appendChild(span);
    }

    // Oggetto
    if (cellData.objectType !== null) {
        const emoji = OBJECT_LABEL_MAP[cellData.objectType];
        if (emoji) {
            const span = document.createElement('span');
            span.className = 'cell-object';
            span.textContent = emoji;
            el.appendChild(span);
        }
        // Quantità/dati extra (candele)
        if (cellData.objectAdditionalData) {
            const qty = document.createElement('span');
            qty.className = 'cell-quantity';
            qty.textContent = cellData.objectAdditionalData;
            el.appendChild(qty);
        }
    }

    // Testo (stringCode → mostra "A" in corsivo)
    if (cellData.stringCode) {
        const lbl = document.createElement('span');
        lbl.className = 'cell-text-marker';
        lbl.textContent = 'A';
        lbl.title = cellData.stringCode;
        el.appendChild(lbl);
    }

    // Label muri
    appendWallLabels(el, cellData);
}

function applyWallBorders(el: HTMLElement, cellData: CellData | null): void {
    if (!cellData) {
        el.style.borderTop    = '1px solid #333';
        el.style.borderRight  = '1px solid #333';
        el.style.borderBottom = '1px solid #333';
        el.style.borderLeft   = '1px solid #333';
        return;
    }

    el.style.borderTop    = wallBorderStyle(cellData.top);
    el.style.borderRight  = wallBorderStyle(cellData.right);
    el.style.borderBottom = wallBorderStyle(cellData.bottom);
    el.style.borderLeft   = wallBorderStyle(cellData.left);
}

function wallBorderStyle(wallType: number): string {
    if (wallType === 0) return '1px solid #333';
    const color = WALL_COLOR_MAP[wallType] ?? '#ffffff';
    return `4px solid ${color}`;
}

function appendWallLabels(el: HTMLElement, cellData: CellData): void {
    const positions: Array<[number, string, string]> = [
        [cellData.top,    'wall-label-top',    'top'],
        [cellData.right,  'wall-label-right',  'right'],
        [cellData.bottom, 'wall-label-bottom', 'bottom'],
        [cellData.left,   'wall-label-left',   'left'],
    ];
    for (const [wallType, cls, _pos] of positions) {
        if (wallType === 0) continue;
        const code = WALL_CODE_MAP[wallType];
        if (!code) continue;
        const span = document.createElement('span');
        span.className = `cell-wall-label ${cls}`;
        span.textContent = code;
        el.appendChild(span);
    }
}

function getRiempimentoIndex(groundType: number): number {
    // Riempimento1=8, Riempimento2=17, Riempimento3=18, ..., Riempimento13=28
    if (groundType === 8) return 1;
    return groundType - 15; // 17→2, 18→3, ..., 28→13
}

// ---------------------------------------------------------------------------
// Hover highlight
// ---------------------------------------------------------------------------

export function setHoverHighlight(el: HTMLElement): void {
    el.classList.add('editor-cell-hover');
}

export function clearHoverHighlight(el: HTMLElement): void {
    el.classList.remove('editor-cell-hover');
}
