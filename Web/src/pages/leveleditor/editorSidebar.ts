import {
    EditorAction,
    GROUND_ENTRIES,
    WALL_ENTRIES,
    OBJECT_ENTRIES,
    ENEMY_ENTRIES,
} from './editorTypes';
import type { EditorState } from './editorState';

export interface SidebarCallbacks {
    onGroundSelected: (value: number) => void;
    onWallSelected: (value: number) => void;
    onObjectSelected: (value: number) => void;
    onActionChanged: (action: EditorAction) => void;
}

/**
 * Crea la sidebar con 4 tab: Terreno, Muri, Oggetti, Nemici.
 * Ritorna un elemento DOM pronto da appendere e una funzione
 * `updateSingletonButtons` per disabilitare radio singleton.
 */
export function createSidebar(
    state: EditorState,
    callbacks: SidebarCallbacks
): {
    element: HTMLElement;
    updateSingletonButtons: (s: EditorState) => void;
} {
    const sidebar = document.createElement('aside');
    sidebar.className = 'editor-sidebar';

    // --- Tab header ---
    const tabsBar = document.createElement('div');
    tabsBar.className = 'editor-tabs';

    const tabDefs = [
        { id: 'ground', label: 'Terra',  action: EditorAction.Ground },
        { id: 'wall',   label: 'Muri',   action: EditorAction.Wall   },
        { id: 'obj',    label: 'Oggetti', action: EditorAction.Object },
        { id: 'enemy',  label: 'Nemici', action: EditorAction.Enemy  },
    ];

    const tabBtns: HTMLButtonElement[] = [];
    const tabPanels: HTMLElement[] = [];

    tabDefs.forEach((def, idx) => {
        const btn = document.createElement('button');
        btn.className = 'editor-tab-btn' + (idx === 0 ? ' active' : '');
        btn.textContent = def.label;
        btn.dataset['tab'] = def.id;
        tabsBar.appendChild(btn);
        tabBtns.push(btn);
    });

    sidebar.appendChild(tabsBar);

    // --- Pannello Terreno ---
    const groundPanel = document.createElement('div');
    groundPanel.className = 'editor-tab-panel active';
    groundPanel.dataset['panel'] = 'ground';

    GROUND_ENTRIES.forEach(entry => {
        const { radio, label } = makeToolRadio(
            'editor-tool-ground',
            String(entry.value),
            entry.label,
            entry.color,
            entry.value === state.selectedGround
        );
        radio.addEventListener('change', () => {
            if (radio.checked) {
                callbacks.onGroundSelected(entry.value);
            }
        });
        groundPanel.appendChild(radio);
        groundPanel.appendChild(label);
    });

    tabPanels.push(groundPanel);
    sidebar.appendChild(groundPanel);

    // --- Pannello Muri ---
    const wallPanel = document.createElement('div');
    wallPanel.className = 'editor-tab-panel';
    wallPanel.dataset['panel'] = 'wall';

    WALL_ENTRIES.forEach(entry => {
        const { radio, label } = makeToolRadio(
            'editor-tool-wall',
            String(entry.value),
            `${entry.code || '—'} ${entry.label}`,
            entry.color,
            entry.value === state.selectedWall
        );
        radio.addEventListener('change', () => {
            if (radio.checked) {
                callbacks.onWallSelected(entry.value);
            }
        });
        wallPanel.appendChild(radio);
        wallPanel.appendChild(label);
    });

    tabPanels.push(wallPanel);
    sidebar.appendChild(wallPanel);

    // --- Pannello Oggetti ---
    const objPanel = document.createElement('div');
    objPanel.className = 'editor-tab-panel';
    objPanel.dataset['panel'] = 'obj';

    OBJECT_ENTRIES.forEach(entry => {
        const { radio, label } = makeToolRadio(
            'editor-tool-obj',
            String(entry.value),
            `${entry.emoji} ${entry.label}`,
            '#333',
            false
        );
        radio.id = `obj-radio-${entry.value}`;
        radio.addEventListener('change', () => {
            if (radio.checked) {
                callbacks.onObjectSelected(entry.value);
            }
        });
        objPanel.appendChild(radio);
        objPanel.appendChild(label);
    });

    tabPanels.push(objPanel);
    sidebar.appendChild(objPanel);

    // --- Pannello Nemici ---
    const enemyPanel = document.createElement('div');
    enemyPanel.className = 'editor-tab-panel';
    enemyPanel.dataset['panel'] = 'enemy';

    ENEMY_ENTRIES.forEach(entry => {
        const { radio, label } = makeToolRadio(
            'editor-tool-obj', // stesso gruppo degli oggetti → mutuamente esclusivi
            String(entry.value),
            `${entry.emoji} ${entry.label}`,
            '#333',
            false
        );
        radio.id = `obj-radio-${entry.value}`;
        radio.addEventListener('change', () => {
            if (radio.checked) {
                callbacks.onObjectSelected(entry.value);
            }
        });
        enemyPanel.appendChild(radio);
        enemyPanel.appendChild(label);
    });

    tabPanels.push(enemyPanel);
    sidebar.appendChild(enemyPanel);

    // --- Tab switching ---
    tabBtns.forEach((btn, idx) => {
        btn.addEventListener('click', () => {
            tabBtns.forEach(b => b.classList.remove('active'));
            tabPanels.forEach(p => p.classList.remove('active'));
            btn.classList.add('active');
            tabPanels[idx]!.classList.add('active');
            // Aggiorna l'azione corrente in base al tab
            callbacks.onActionChanged(tabDefs[idx]!.action);
        });
    });

    // --- updateSingletonButtons ---
    function updateSingletonButtons(s: EditorState): void {
        // Oggetti e nemici
        const allEntries = [...OBJECT_ENTRIES, ...ENEMY_ENTRIES];
        allEntries.forEach(entry => {
            const radio = sidebar.querySelector<HTMLInputElement>(`#obj-radio-${entry.value}`);
            if (!radio) return;
            // Singleton: disabilita se già piazzato
            if (s.singletonPlaced.has(entry.value)) {
                radio.disabled = true;
            } else {
                radio.disabled = false;
            }
        });
    }

    return { element: sidebar, updateSingletonButtons };
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makeToolRadio(
    groupName: string,
    value: string,
    labelText: string,
    bgColor: string,
    checked: boolean
): { radio: HTMLInputElement; label: HTMLLabelElement } {
    const id = `${groupName}-${value}`;

    const radio = document.createElement('input');
    radio.type = 'radio';
    radio.name = groupName;
    radio.value = value;
    radio.id = id;
    radio.checked = checked;
    radio.className = 'tool-radio';

    const label = document.createElement('label');
    label.htmlFor = id;
    label.className = 'tool-label';
    label.style.backgroundColor = bgColor;

    const swatch = document.createElement('span');
    swatch.className = 'tool-color-swatch';
    swatch.style.backgroundColor = bgColor;

    label.appendChild(swatch);
    label.appendChild(document.createTextNode(labelText));

    return { radio, label };
}
