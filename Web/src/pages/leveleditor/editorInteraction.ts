import {
    EditorAction,
    SINGLETON_OBJECTS,
    ENEMY_OBJECT_VALUES,
    ENEMY_PREFIX_MAP,
} from './editorTypes';
import type { EditorState } from './editorState';
import {
    setGroundType,
    setWallType,
    setObjectType,
    setStringCode,
    WP_TOP, WP_RIGHT, WP_LEFT, WP_BOTTOM,
} from './editorState';
import { setHoverHighlight, clearHoverHighlight } from './editorGrid';

export type CellChangedCallback = (col: number, row: number) => void;

/**
 * Attacca tutti i listener di interazione mouse e keyboard alla griglia.
 * Ritorna una cleanup function da chiamare su `leave` della route.
 */
export function attachInteraction(
    gridEl: HTMLElement,
    state: EditorState,
    onCellChanged: CellChangedCallback,
    updateSingletonButtons: (s: EditorState) => void,
    switchToTab: (tabId: string) => void
): () => void {
    let isMouseDown = false;
    let lastHoveredEl: HTMLElement | null = null;

    // ---------------------------------------------------------------------------
    // Hit-testing: target → col/row
    // ---------------------------------------------------------------------------
    function getCellFromEvent(e: MouseEvent): { col: number; row: number; el: HTMLElement } | null {
        let target = e.target as HTMLElement | null;
        while (target && target !== gridEl) {
            if (target.dataset['col'] !== undefined && target.dataset['row'] !== undefined) {
                const col = Number(target.dataset['col']);
                const row = Number(target.dataset['row']);
                return { col, row, el: target };
            }
            target = target.parentElement;
        }
        return null;
    }

    // ---------------------------------------------------------------------------
    // Applica azione su cella
    // ---------------------------------------------------------------------------
    function applyAction(col: number, row: number): void {
        switch (state.currentAction) {
            case EditorAction.Ground:
                applyGround(col, row);
                break;
            case EditorAction.Object:
            case EditorAction.Enemy:
                applyObject(col, row);
                break;
            case EditorAction.Text:
                applyText(col, row);
                break;
            // Wall mode: azione su click non ha senso (si usa frecce)
        }
    }

    function applyGround(col: number, row: number): void {
        setGroundType(state, col, row, state.selectedGround);
        refreshCell(col, row);
    }

    function applyObject(col: number, row: number): void {
        const objType = state.selectedObject;

        // Cancella
        if (objType === 0) {
            setObjectType(state, col, row, null);
            refreshCell(col, row);
            updateSingletonButtons(state);
            return;
        }

        // Singleton già piazzato → ignora
        if (SINGLETON_OBJECTS.has(objType) && state.singletonPlaced.has(objType)) {
            return;
        }

        // Candele: chiedi quantità
        if (objType === 4) {
            const qty = window.prompt('Quantità candele:', '1');
            if (qty === null) return;
            setObjectType(state, col, row, objType, qty.trim() || '1');
        } else if (ENEMY_OBJECT_VALUES.has(objType)) {
            // Nemici: auto-genera waypoint ID con possibilità di modifica
            const autoId = autoGenerateWaypoint(objType);
            const prefix = ENEMY_PREFIX_MAP[objType] ?? String(objType);
            const path = window.prompt(
                `Waypoint auto-generato per ${prefix} (gruppo ${state.currentWaypointGroup}).\n` +
                `Modifica se necessario, oppure lascia vuoto per nessun waypoint:`,
                autoId
            );
            if (path === null) return;
            setObjectType(state, col, row, objType, path.trim());
        } else {
            setObjectType(state, col, row, objType, '');
        }

        refreshCell(col, row);
        updateSingletonButtons(state);

        // Singleton: dopo piazzamento torna a Ground
        if (SINGLETON_OBJECTS.has(objType)) {
            state.currentAction = EditorAction.Ground;
        }
    }

    function applyText(col: number, row: number): void {
        const currentCell = state.grid.grid[col]?.[row];
        const currentCode = currentCell?.stringCode ?? '';
        const code = window.prompt('Testo da mostrare in gioco (lascia vuoto per rimuovere):', currentCode);
        if (code === null) return;
        setStringCode(state, col, row, code.trim());
        refreshCell(col, row);
    }

    function applyWall(col: number, row: number, wallPosition: number): void {
        const ok = setWallType(state, col, row, state.selectedWall, wallPosition);
        if (ok) refreshCell(col, row);
    }

    function refreshCell(col: number, row: number): void {
        onCellChanged(col, row);
    }

    // ---------------------------------------------------------------------------
    // Auto-generazione waypoint
    // ---------------------------------------------------------------------------
    function autoGenerateWaypoint(objType: number): string {
        const prefix = ENEMY_PREFIX_MAP[objType];
        if (!prefix) return '';
        const group = state.currentWaypointGroup;
        const pattern = `${prefix}-${group}-`;
        let count = 0;
        for (let c = 0; c < state.grid.columns; c++) {
            for (let r = 0; r < state.grid.rows; r++) {
                const cell = state.grid.grid[c]?.[r];
                if (cell?.objectAdditionalData?.startsWith(pattern)) {
                    count++;
                }
            }
        }
        return `${prefix}-${group}-${count + 1}`;
    }

    // ---------------------------------------------------------------------------
    // Mouse events
    // ---------------------------------------------------------------------------
    function onMouseDown(e: MouseEvent): void {
        if (e.button !== 0) return;
        isMouseDown = true;
        const hit = getCellFromEvent(e);
        if (!hit) return;
        if (state.currentAction !== EditorAction.Wall) {
            applyAction(hit.col, hit.row);
        }
    }

    function onMouseMove(e: MouseEvent): void {
        const hit = getCellFromEvent(e);

        // Hover highlight
        if (lastHoveredEl && lastHoveredEl !== hit?.el) {
            clearHoverHighlight(lastHoveredEl);
        }
        if (hit) {
            setHoverHighlight(hit.el);
            lastHoveredEl = hit.el;
            state.hoveredCell = { col: hit.col, row: hit.row };
        } else {
            lastHoveredEl = null;
            state.hoveredCell = null;
        }

        // Drag painting (solo Ground mode)
        if (isMouseDown && hit && state.currentAction === EditorAction.Ground) {
            applyGround(hit.col, hit.row);
        }
    }

    function onMouseUp(): void {
        isMouseDown = false;
    }

    function onMouseLeave(): void {
        if (lastHoveredEl) {
            clearHoverHighlight(lastHoveredEl);
            lastHoveredEl = null;
        }
        state.hoveredCell = null;
        isMouseDown = false;
    }

    // ---------------------------------------------------------------------------
    // Keyboard events
    // ---------------------------------------------------------------------------
    function isInputFocused(): boolean {
        const active = document.activeElement;
        if (!active) return false;
        const tag = active.tagName.toLowerCase();
        return tag === 'input' || tag === 'textarea' || tag === 'select';
    }

    function onKeyDown(e: KeyboardEvent): void {
        if (isInputFocused()) return;

        switch (e.key.toLowerCase()) {
            case 'f':
                state.currentAction = EditorAction.Ground;
                state.selectedGround = 1; // Terra1
                syncGroundRadio(1);
                switchToTab('ground');
                return;
            case 'w':
                state.currentAction = EditorAction.Wall;
                state.selectedWall = 1; // Occludente
                syncWallRadio(1);
                switchToTab('wall');
                return;
            case 'd':
                state.currentAction = EditorAction.Wall;
                state.selectedWall = 0; // Niente
                syncWallRadio(0);
                switchToTab('wall');
                return;
            case 'c':
                state.currentAction = EditorAction.Object;
                state.selectedObject = 0; // Cancella
                syncObjRadio(0);
                switchToTab('obj');
                return;
            case 'r':
                state.currentAction = EditorAction.Ground;
                state.selectedGround = 8; // Riempimento1
                syncGroundRadio(8);
                switchToTab('ground');
                return;
        }

        // Frecce: piazza muro sul lato corrispondente (solo wall mode)
        if (state.currentAction !== EditorAction.Wall) return;
        if (!state.hoveredCell) return;

        const { col, row } = state.hoveredCell;
        let wallPos: number | null = null;

        switch (e.key) {
            case 'ArrowUp':    wallPos = WP_TOP;    break;
            case 'ArrowRight': wallPos = WP_RIGHT;  break;
            case 'ArrowLeft':  wallPos = WP_LEFT;   break;
            case 'ArrowDown':  wallPos = WP_BOTTOM; break;
        }

        if (wallPos !== null) {
            e.preventDefault();
            applyWall(col, row, wallPos);
        }
    }

    // ---------------------------------------------------------------------------
    // Sync radio buttons
    // ---------------------------------------------------------------------------
    function syncGroundRadio(value: number): void {
        const r = document.querySelector<HTMLInputElement>(`input[name="editor-tool-ground"][value="${value}"]`);
        if (r) r.checked = true;
    }

    function syncWallRadio(value: number): void {
        const r = document.querySelector<HTMLInputElement>(`input[name="editor-tool-wall"][value="${value}"]`);
        if (r) r.checked = true;
    }

    function syncObjRadio(value: number): void {
        const r = document.querySelector<HTMLInputElement>(`input[name="editor-tool-obj"][value="${value}"]`);
        if (r) r.checked = true;
    }

    // ---------------------------------------------------------------------------
    // Attach / detach
    // ---------------------------------------------------------------------------
    gridEl.addEventListener('mousedown', onMouseDown);
    gridEl.addEventListener('mousemove', onMouseMove);
    gridEl.addEventListener('mouseup', onMouseUp);
    gridEl.addEventListener('mouseleave', onMouseLeave);
    window.addEventListener('keydown', onKeyDown);

    const noCtx = (e: Event) => e.preventDefault();
    gridEl.addEventListener('contextmenu', noCtx);

    return function cleanup() {
        gridEl.removeEventListener('mousedown', onMouseDown);
        gridEl.removeEventListener('mousemove', onMouseMove);
        gridEl.removeEventListener('mouseup', onMouseUp);
        gridEl.removeEventListener('mouseleave', onMouseLeave);
        window.removeEventListener('keydown', onKeyDown);
        gridEl.removeEventListener('contextmenu', noCtx);
    };
}
