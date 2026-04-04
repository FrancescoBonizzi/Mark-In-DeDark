import './leveleditor/leveleditor.css';
import { EditorAction, TEXT_MODE_SENTINEL } from './leveleditor/editorTypes';
import {
    createEmptyState,
    isDirty,
} from './leveleditor/editorState';
import type { EditorState } from './leveleditor/editorState';
import { renderGrid, updateCell } from './leveleditor/editorGrid';
import { createSidebar } from './leveleditor/editorSidebar';
import { attachInteraction } from './leveleditor/editorInteraction';
import { openFile, saveFile, confirmIfDirty } from './leveleditor/editorFileIo';

let cleanup: (() => void) | null = null;

// ---------------------------------------------------------------------------
// Blocco mobile
// ---------------------------------------------------------------------------

function isMobileDevice(): boolean {
    const touchOnly = window.matchMedia('(pointer: coarse)').matches;
    const smallScreen = window.innerWidth < 1024;
    return touchOnly || smallScreen;
}

// ---------------------------------------------------------------------------
// Entry point
// ---------------------------------------------------------------------------

export function renderLevelEditorPage(container: HTMLElement): void {
    if (isMobileDevice()) {
        container.innerHTML = `
<div class="editor-mobile-block">
    <h2>Level Editor</h2>
    <p>Il Level Editor richiede un mouse e uno schermo desktop (almeno 1024px).</p>
    <p>Aprilo da un computer.</p>
</div>`;
        return;
    }

    let state: EditorState = createEmptyState();

    // Contenitore pagina
    container.innerHTML = '';
    const page = document.createElement('div');
    page.className = 'level-editor-page';
    container.appendChild(page);

    // Toolbar
    const toolbar = buildToolbar(
        state,
        // onNew
        async () => {
            if (!confirmIfDirty(state)) return;
            state = createEmptyState();
            rebuildGrid();
            updateSingletonButtons(state);
            updateToolbar();
        },
        // onOpen
        async () => {
            if (!confirmIfDirty(state)) return;
            const result = await openFile();
            if (!result) return;
            state = result.state;
            rebuildGrid();
            updateSingletonButtons(state);
            updateToolbar();
        },
        // onSave
        () => {
            saveFile(state);
            updateToolbar();
        }
    );
    page.appendChild(toolbar);

    // Body: sidebar + canvas
    const body = document.createElement('div');
    body.className = 'editor-body';
    page.appendChild(body);

    // Sidebar
    const { element: sidebarEl, updateSingletonButtons } = createSidebar(state, {
        onGroundSelected: (value) => {
            state.selectedGround = value;
            state.currentAction = EditorAction.Ground;
        },
        onWallSelected: (value) => {
            state.selectedWall = value;
            state.currentAction = EditorAction.Wall;
        },
        onObjectSelected: (value) => {
            if (value === TEXT_MODE_SENTINEL) {
                state.currentAction = EditorAction.Text;
                return;
            }
            state.selectedObject = value;
            // Nemici e oggetti: imposta azione Object/Enemy
            const isEnemy = [5, 6, 7, 8, 10, 11].includes(value);
            state.currentAction = isEnemy ? EditorAction.Enemy : EditorAction.Object;
        },
        onActionChanged: (action) => {
            state.currentAction = action;
        },
    });
    body.appendChild(sidebarEl);

    // Canvas scroll wrapper
    const canvasScroll = document.createElement('div');
    canvasScroll.className = 'editor-canvas-scroll';
    body.appendChild(canvasScroll);

    // Griglia
    let cellElements: HTMLElement[][] = [];

    function rebuildGrid(): void {
        if (cleanup) {
            cleanup();
            cleanup = null;
        }
        cellElements = renderGrid(canvasScroll, state);
        const gridEl = canvasScroll.querySelector<HTMLElement>('.editor-grid')!;
        cleanup = attachInteraction(
            gridEl,
            state,
            (col, row) => {
                const el = cellElements[col]?.[row];
                if (el) {
                    updateCell(el, state.grid.grid[col]?.[row] ?? null);
                }
                updateDirtyIndicator();
            },
            updateSingletonButtons
        );
    }

    function updateDirtyIndicator(): void {
        const indicator = toolbar.querySelector<HTMLElement>('.dirty-indicator');
        if (indicator) {
            indicator.textContent = isDirty(state) ? '●' : '';
        }
    }

    function updateToolbar(): void {
        // Aggiorna nome file nella toolbar
        const filenameInput = toolbar.querySelector<HTMLInputElement>('.toolbar-filename');
        if (filenameInput) {
            filenameInput.value = state.currentFilename;
        }
        // Aggiorna dirty
        const indicator = toolbar.querySelector<HTMLElement>('.dirty-indicator');
        if (indicator) {
            indicator.textContent = isDirty(state) ? '●' : '';
        }
        // Aggiorna pack e dirty probability
        const packInput = toolbar.querySelector<HTMLInputElement>('.toolbar-pack');
        if (packInput) packInput.value = String(state.grid.envTexturePackId);
        const dirtyInput = toolbar.querySelector<HTMLInputElement>('.toolbar-dirty');
        if (dirtyInput) dirtyInput.value = String(state.grid.dirtyProbabilityPct);
    }

    rebuildGrid();
}

// ---------------------------------------------------------------------------
// Toolbar
// ---------------------------------------------------------------------------

function buildToolbar(
    state: EditorState,
    onNew: () => void,
    onOpen: () => void,
    onSave: () => void
): HTMLElement {
    const toolbar = document.createElement('div');
    toolbar.className = 'editor-toolbar';

    // Bottoni file
    const btnNew  = makeBtn('Nuovo',  'danger');
    const btnOpen = makeBtn('Apri');
    const btnSave = makeBtn('Salva');

    btnNew.addEventListener('click', () => void onNew());
    btnOpen.addEventListener('click', () => void onOpen());
    btnSave.addEventListener('click', onSave);

    toolbar.appendChild(btnNew);
    toolbar.appendChild(btnOpen);
    toolbar.appendChild(btnSave);

    // Dirty indicator
    const dirty = document.createElement('span');
    dirty.className = 'dirty-indicator';
    dirty.title = 'Modifiche non salvate';
    toolbar.appendChild(dirty);

    toolbar.appendChild(makeSep());

    // Nome file
    const lbFile = document.createElement('label');
    lbFile.textContent = 'File:';
    const filenameInput = document.createElement('input');
    filenameInput.type = 'text';
    filenameInput.className = 'toolbar-filename';
    filenameInput.value = state.currentFilename;
    filenameInput.addEventListener('change', () => {
        state.currentFilename = filenameInput.value;
    });
    toolbar.appendChild(lbFile);
    toolbar.appendChild(filenameInput);

    toolbar.appendChild(makeSep());

    // Texture pack
    const lbPack = document.createElement('label');
    lbPack.textContent = 'Pack:';
    const packInput = document.createElement('input');
    packInput.type = 'number';
    packInput.className = 'toolbar-pack';
    packInput.min = '1';
    packInput.max = '8';
    packInput.value = String(state.grid.envTexturePackId);
    packInput.addEventListener('change', () => {
        const v = parseInt(packInput.value, 10);
        if (!isNaN(v) && v >= 1 && v <= 8) {
            state.grid.envTexturePackId = v;
        }
    });
    toolbar.appendChild(lbPack);
    toolbar.appendChild(packInput);

    toolbar.appendChild(makeSep());

    // Dirty probability
    const lbDirtyPct = document.createElement('label');
    lbDirtyPct.textContent = 'Sporco %:';
    const dirtyInput = document.createElement('input');
    dirtyInput.type = 'number';
    dirtyInput.className = 'toolbar-dirty';
    dirtyInput.min = '0';
    dirtyInput.max = '100';
    dirtyInput.value = String(state.grid.dirtyProbabilityPct);
    dirtyInput.addEventListener('change', () => {
        const v = parseInt(dirtyInput.value, 10);
        if (!isNaN(v) && v >= 0 && v <= 100) {
            state.grid.dirtyProbabilityPct = v;
        }
    });
    toolbar.appendChild(lbDirtyPct);
    toolbar.appendChild(dirtyInput);

    toolbar.appendChild(makeSep());

    // Shortcut help
    const help = document.createElement('span');
    help.style.color = '#666';
    help.style.fontSize = '11px';
    help.textContent = 'F=Terra  W=Muro  D=NoMuro  C=Cancella  R=Fill  ↑↓←→=Muro lato (wall mode)';
    toolbar.appendChild(help);

    return toolbar;
}

function makeBtn(text: string, extraClass?: string): HTMLButtonElement {
    const btn = document.createElement('button');
    btn.className = 'btn' + (extraClass ? ` ${extraClass}` : '');
    btn.textContent = text;
    return btn;
}

function makeSep(): HTMLElement {
    const sep = document.createElement('span');
    sep.className = 'toolbar-sep';
    return sep;
}

// ---------------------------------------------------------------------------
// Leave hook (chiamato dal router)
// ---------------------------------------------------------------------------

export function destroyLevelEditor(): void {
    if (cleanup) {
        cleanup();
        cleanup = null;
    }
}
