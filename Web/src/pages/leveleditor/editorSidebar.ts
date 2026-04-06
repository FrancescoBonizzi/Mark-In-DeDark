import {
    EditorAction,
    GROUND_ENTRIES,
    WALL_ENTRIES,
    OBJECT_ENTRIES,
    ENEMY_ENTRIES,
} from './editorTypes';
import type { EditorState } from './editorState';
import type { PackAvailability } from './packCatalog';
import { renderPackPreview } from './packCatalog';

export interface SidebarCallbacks {
    onGroundSelected: (value: number) => void;
    onWallSelected: (value: number) => void;
    onObjectSelected: (value: number) => void;
    onActionChanged: (action: EditorAction) => void;
}

export interface SidebarOptions {
    assetsRoot: string;
    initialPackId: number;
}

/**
 * Crea la sidebar con 4 tab: Terreno, Muri, Oggetti, Nemici.
 * Ritorna elemento DOM, updateSingletonButtons, switchToTab.
 */
export function createSidebar(
    state: EditorState,
    callbacks: SidebarCallbacks,
    options: SidebarOptions
): {
    element: HTMLElement;
    updateSingletonButtons: (s: EditorState) => void;
    switchToTab: (tabId: string) => void;
    updatePackFilter: (avail: PackAvailability) => void;
    updatePreview: (packId: number) => void;
} {
    const sidebar = document.createElement('aside');
    sidebar.className = 'editor-sidebar';

    // --- Preview striscia texture pack ---
    const previewStrip = document.createElement('div');
    previewStrip.className = 'pack-preview-strip';

    const previewLabel = document.createElement('div');
    previewLabel.className = 'pack-preview-label';
    previewLabel.textContent = `Pack ${options.initialPackId}`;

    const previewCells = document.createElement('div');
    previewCells.className = 'pack-preview-cells';

    previewStrip.appendChild(previewLabel);
    previewStrip.appendChild(previewCells);
    sidebar.appendChild(previewStrip);

    renderPackPreview(options.initialPackId, previewCells, options.assetsRoot);

    // --- Tab header ---
    const tabsBar = document.createElement('div');
    tabsBar.className = 'editor-tabs';

    const tabDefs = [
        { id: 'ground', label: 'Terra',   action: EditorAction.Ground },
        { id: 'wall',   label: 'Muri',    action: EditorAction.Wall   },
        { id: 'obj',    label: 'Oggetti', action: EditorAction.Object },
        { id: 'enemy',  label: 'Nemici',  action: EditorAction.Enemy  },
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
        const isSelected = entry.value === state.selectedObject &&
            state.currentAction !== EditorAction.Enemy &&
            state.currentAction !== EditorAction.Wall;
        const { radio, label } = makeToolRadio(
            'editor-tool-obj',
            String(entry.value),
            `${entry.emoji} ${entry.label}`,
            '#333',
            isSelected
        );
        radio.id = `obj-radio-${entry.value}`;
        label.htmlFor = radio.id;
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

    // Selettore gruppo waypoint
    const groupControl = document.createElement('div');
    groupControl.className = 'waypoint-group-control';

    const groupLabel = document.createElement('span');
    groupLabel.textContent = 'Gruppo waypoint: ';

    const groupDisplay = document.createElement('strong');
    groupDisplay.className = 'waypoint-group-display';
    groupDisplay.textContent = String(state.currentWaypointGroup);

    const btnMinus = document.createElement('button');
    btnMinus.textContent = '−';
    btnMinus.className = 'waypoint-btn';
    btnMinus.title = 'Gruppo precedente';
    btnMinus.addEventListener('click', () => {
        if (state.currentWaypointGroup > 1) {
            state.currentWaypointGroup--;
            groupDisplay.textContent = String(state.currentWaypointGroup);
        }
    });

    const btnPlus = document.createElement('button');
    btnPlus.textContent = '+';
    btnPlus.className = 'waypoint-btn';
    btnPlus.title = 'Nuovo gruppo';
    btnPlus.addEventListener('click', () => {
        state.currentWaypointGroup++;
        groupDisplay.textContent = String(state.currentWaypointGroup);
    });

    groupControl.appendChild(groupLabel);
    groupControl.appendChild(btnMinus);
    groupControl.appendChild(groupDisplay);
    groupControl.appendChild(btnPlus);

    const groupHelp = document.createElement('div');
    groupHelp.className = 'waypoint-group-help';
    groupHelp.textContent = 'Il waypoint viene auto-generato al click. Modifica nel prompt se necessario.';

    const goblinTriggerHint = document.createElement('div');
    goblinTriggerHint.className = 'waypoint-group-help';
    goblinTriggerHint.style.marginTop = '6px';
    goblinTriggerHint.textContent = '⚡ Per far spawnare un Goblin usa "Trappola Goblin" nel tab Terra.';

    enemyPanel.appendChild(groupControl);
    enemyPanel.appendChild(groupHelp);
    enemyPanel.appendChild(goblinTriggerHint);

    ENEMY_ENTRIES.forEach(entry => {
        const isSelected = entry.value === state.selectedObject &&
            state.currentAction === EditorAction.Enemy;
        const { radio, label } = makeToolRadio(
            'editor-tool-obj', // stesso gruppo degli oggetti → mutuamente esclusivi
            String(entry.value),
            `${entry.emoji} ${entry.label}`,
            '#333',
            isSelected
        );
        radio.id = `obj-radio-${entry.value}`;
        label.htmlFor = radio.id;
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
    function switchToTab(tabId: string): void {
        const idx = tabDefs.findIndex(d => d.id === tabId);
        if (idx < 0) return;
        tabBtns.forEach(b => b.classList.remove('active'));
        tabPanels.forEach(p => p.classList.remove('active'));
        tabBtns[idx]!.classList.add('active');
        tabPanels[idx]!.classList.add('active');
        // Aggiorna gruppo display quando si passa alla tab nemici
        if (tabId === 'enemy') {
            groupDisplay.textContent = String(state.currentWaypointGroup);
        }
    }

    tabBtns.forEach((btn, idx) => {
        btn.addEventListener('click', () => {
            switchToTab(tabDefs[idx]!.id);
            callbacks.onActionChanged(tabDefs[idx]!.action);
        });
    });

    // --- updateSingletonButtons ---
    function updateSingletonButtons(s: EditorState): void {
        const allEntries = [...OBJECT_ENTRIES, ...ENEMY_ENTRIES];
        allEntries.forEach(entry => {
            const radio = sidebar.querySelector<HTMLInputElement>(`#obj-radio-${entry.value}`);
            if (!radio) return;
            radio.disabled = s.singletonPlaced.has(entry.value);
        });
    }

    // --- updatePackFilter: nasconde ground/wall non disponibili per il pack ---
    function updatePackFilter(avail: PackAvailability): void {
        // Ground: NonCella (0) e Riempimento (8, 17-28) sempre visibili
        GROUND_ENTRIES.forEach(entry => {
            if (entry.value === 0 || entry.value === 8 || entry.value >= 17) return;
            const radio = sidebar.querySelector<HTMLInputElement>(
                `input[name="editor-tool-ground"][value="${entry.value}"]`
            );
            if (!radio) return;
            const visible = avail.availableGrounds.has(entry.value);
            radio.disabled = !visible;
            const label = radio.nextElementSibling as HTMLElement | null;
            if (label) label.style.display = visible ? '' : 'none';
        });

        // Wall: decal e muri speciali filtrati per pack
        const DECAL_VALUES = new Set([7, 8, 9, 10, 11, 16, 17, 18]);
        const SPECWALL_VALUES = new Set([4, 12, 13, 14, 15]);

        WALL_ENTRIES.forEach(entry => {
            let visible: boolean;
            if (DECAL_VALUES.has(entry.value)) {
                visible = avail.availableDecals.has(entry.value);
            } else if (SPECWALL_VALUES.has(entry.value)) {
                visible = avail.availableSpecialWalls.has(entry.value);
            } else {
                return; // Occludente, Fragile, Porta, Ingresso, Uscita: sempre visibili
            }
            const radio = sidebar.querySelector<HTMLInputElement>(
                `input[name="editor-tool-wall"][value="${entry.value}"]`
            );
            if (!radio) return;
            radio.disabled = !visible;
            const label = radio.nextElementSibling as HTMLElement | null;
            if (label) label.style.display = visible ? '' : 'none';
        });
    }

    function updatePreview(packId: number): void {
        previewLabel.textContent = `Pack ${packId}`;
        renderPackPreview(packId, previewCells, options.assetsRoot);
    }

    return { element: sidebar, updateSingletonButtons, switchToTab, updatePackFilter, updatePreview };
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
