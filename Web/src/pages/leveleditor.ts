import './leveleditor/leveleditor.css';
import { EditorAction, TEXT_MODE_SENTINEL } from './leveleditor/editorTypes';
import {
    createEmptyState,
    createStateFromBundle,
    isDirty,
} from './leveleditor/editorState';
import type { EditorState } from './leveleditor/editorState';
import { parseLevel } from '../grid/GridLoader';
import { serializeMark } from '../grid/GridSerializer';
import { renderGrid, updateCell } from './leveleditor/editorGrid';
import { createSidebar } from './leveleditor/editorSidebar';
import { attachInteraction } from './leveleditor/editorInteraction';
import { openFile, saveFile, confirmIfDirty } from './leveleditor/editorFileIo';
import { PREVIEW_SESSION_KEY, PREVIEW_FOLDER } from '../assets/LevelLoader';

let cleanup: (() => void) | null = null;
let autosaveInterval: ReturnType<typeof setInterval> | null = null;

const AUTOSAVE_KEY = 'editor-autosave';
const AUTOSAVE_FILENAME_KEY = 'editor-autosave-filename';
const AUTOSAVE_TIMESTAMP_KEY = 'editor-autosave-timestamp';
const AUTOSAVE_INTERVAL_MS = 30_000;

// ---------------------------------------------------------------------------
// Blocco mobile
// ---------------------------------------------------------------------------

function isMobileDevice(): boolean {
    const touchOnly = window.matchMedia('(pointer: coarse)').matches;
    const smallScreen = window.innerWidth < 1024;
    return touchOnly || smallScreen;
}

// ---------------------------------------------------------------------------
// Autosave helpers
// ---------------------------------------------------------------------------

function saveToAutosave(state: EditorState): void {
    try {
        sessionStorage.setItem(AUTOSAVE_KEY, serializeMark(state.grid));
        sessionStorage.setItem(AUTOSAVE_FILENAME_KEY, state.currentFilename);
        sessionStorage.setItem(AUTOSAVE_TIMESTAMP_KEY, new Date().toISOString());
    } catch {
        // sessionStorage piena o non disponibile: ignora silenziosamente
    }
}

function clearAutosave(): void {
    sessionStorage.removeItem(AUTOSAVE_KEY);
    sessionStorage.removeItem(AUTOSAVE_FILENAME_KEY);
    sessionStorage.removeItem(AUTOSAVE_TIMESTAMP_KEY);
}

function loadAutosaveIfAvailable(): EditorState | null {
    const saved = sessionStorage.getItem(AUTOSAVE_KEY);
    if (!saved) return null;
    const filename = sessionStorage.getItem(AUTOSAVE_FILENAME_KEY) ?? 'autosave.mark';
    const timestamp = sessionStorage.getItem(AUTOSAVE_TIMESTAMP_KEY);

    const when = timestamp ? new Date(timestamp).toLocaleTimeString('it-IT') : 'ora sconosciuta';
    const ok = window.confirm(
        `Trovato un autosalvataggio del ${when} (file: ${filename}).\n\nRipristinare?`
    );
    if (!ok) return null;

    try {
        const bundle = parseLevel(saved);
        return createStateFromBundle(bundle, filename);
    } catch {
        window.alert('Errore nel ripristinare l\'autosalvataggio. Verrà avviato un livello vuoto.');
        clearAutosave();
        return null;
    }
}

function formatTime(date: Date): string {
    return date.toLocaleTimeString('it-IT', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
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

    // Tenta ripristino autosave
    let state: EditorState = loadAutosaveIfAvailable() ?? createEmptyState();

    container.innerHTML = '';
    const page = document.createElement('div');
    page.className = 'level-editor-page';
    container.appendChild(page);

    // Toolbar
    const { toolbar, setAutosaveStatus } = buildToolbar(
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
        },
        // onTestInGame
        () => {
            sessionStorage.setItem(PREVIEW_SESSION_KEY, serializeMark(state.grid));
            window.location.hash = `#/game/${PREVIEW_FOLDER}/0`;
        }
    );
    page.appendChild(toolbar);

    // Body: sidebar + canvas
    const body = document.createElement('div');
    body.className = 'editor-body';
    page.appendChild(body);

    // Sidebar
    const { element: sidebarEl, updateSingletonButtons, switchToTab } = createSidebar(state, {
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
            updateSingletonButtons,
            switchToTab
        );
    }

    function updateDirtyIndicator(): void {
        const indicator = toolbar.querySelector<HTMLElement>('.dirty-indicator');
        if (indicator) {
            indicator.textContent = isDirty(state) ? '●' : '';
        }
    }

    function updateToolbar(): void {
        const filenameInput = toolbar.querySelector<HTMLInputElement>('.toolbar-filename');
        if (filenameInput) filenameInput.value = state.currentFilename;

        const indicator = toolbar.querySelector<HTMLElement>('.dirty-indicator');
        if (indicator) indicator.textContent = isDirty(state) ? '●' : '';

        const packInput = toolbar.querySelector<HTMLInputElement>('.toolbar-pack');
        if (packInput) packInput.value = String(state.grid.envTexturePackId);

        const dirtyInput = toolbar.querySelector<HTMLInputElement>('.toolbar-dirty');
        if (dirtyInput) dirtyInput.value = String(state.grid.dirtyProbabilityPct);
    }

    rebuildGrid();

    // Autosave ogni 30s
    autosaveInterval = setInterval(() => {
        saveToAutosave(state);
        setAutosaveStatus(`Autosalvataggio: ${formatTime(new Date())}`);
    }, AUTOSAVE_INTERVAL_MS);
}

// ---------------------------------------------------------------------------
// Toolbar
// ---------------------------------------------------------------------------

function buildToolbar(
    state: EditorState,
    onNew: () => void,
    onOpen: () => void,
    onSave: () => void,
    onTestInGame: () => void
): { toolbar: HTMLElement; setAutosaveStatus: (msg: string) => void } {
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

    // Pulsante "Testa in gioco"
    const btnTest = makeBtn('Testa in gioco', 'primary');
    btnTest.title = 'Salva in sessionStorage e avvia il livello nel gioco';
    btnTest.addEventListener('click', onTestInGame);
    toolbar.appendChild(btnTest);

    toolbar.appendChild(makeSep());

    // Autosave status
    const autosaveStatus = document.createElement('span');
    autosaveStatus.className = 'autosave-status';
    autosaveStatus.textContent = 'Autosalvataggio: mai';
    toolbar.appendChild(autosaveStatus);

    toolbar.appendChild(makeSep());

    // Shortcut help
    const help = document.createElement('span');
    help.style.color = '#666';
    help.style.fontSize = '11px';
    help.textContent = 'F=Terra  W=Muro  D=NoMuro  C=Cancella  R=Fill  ↑↓←→=Muro lato (wall mode)';
    toolbar.appendChild(help);

    function setAutosaveStatus(msg: string): void {
        autosaveStatus.textContent = msg;
    }

    return { toolbar, setAutosaveStatus };
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
    if (autosaveInterval !== null) {
        clearInterval(autosaveInterval);
        autosaveInterval = null;
    }
}
