/**
 * editorTypes.ts — Lookup maps runtime per il Level Editor.
 *
 * I `const enum` di enums.ts sono cancellati a compile time:
 * questi array/mappe forniscono i valori numerici a runtime per
 * iterare nei radio button e fare lookup O(1) nel rendering.
 */

export const enum EditorAction {
    Ground = 0,
    Wall = 1,
    Object = 2,
    Enemy = 3,
    Text = 4,
}

/** Valore sentinella per il bottone "Testo" nella sidebar Oggetti */
export const TEXT_MODE_SENTINEL = -1;

// ---------------------------------------------------------------------------
// Ground entries
// ---------------------------------------------------------------------------

export interface GroundEntry {
    value: number;
    label: string;
    color: string;
}

// Colori da App.xaml (ColoreTerra*) + convenzioni C#
export const GROUND_ENTRIES: GroundEntry[] = [
    { value: 0,  label: 'NonCella',               color: '#000000' },
    { value: 1,  label: 'Terra1',                 color: '#6E2233' },
    { value: 6,  label: 'Terra2',                 color: '#831d1c' },
    { value: 9,  label: 'Terra3',                 color: '#940000' },
    { value: 10, label: 'Terra4',                 color: '#960018' },
    { value: 11, label: 'Terra5',                 color: '#B20000' },
    { value: 12, label: 'Terra6',                 color: '#D21F1B' },
    { value: 13, label: 'Terra7',                 color: '#DC143C' },
    { value: 14, label: 'Terra8',                 color: '#E30022' },
    { value: 15, label: 'Terra9',                 color: '#FF4040' },
    { value: 16, label: 'Terra10',                color: '#CA411F' },
    { value: 7,  label: 'Buio',                   color: '#808080' },
    { value: 3,  label: 'TrappolaMorteIstantanea', color: '#FF00FF' },
    { value: 4,  label: 'TrappolaTemporizzata',   color: '#00FFFF' },
    { value: 5,  label: 'Trappola Goblin ⚡',       color: '#3a8f3a' },
    { value: 8,  label: 'Riempimento1',           color: '#000000' },
    { value: 17, label: 'Riempimento2',           color: '#000000' },
    { value: 18, label: 'Riempimento3',           color: '#000000' },
    { value: 19, label: 'Riempimento4',           color: '#000000' },
    { value: 20, label: 'Riempimento5',           color: '#000000' },
    { value: 21, label: 'Riempimento6',           color: '#000000' },
    { value: 22, label: 'Riempimento7',           color: '#000000' },
    { value: 23, label: 'Riempimento8',           color: '#000000' },
    { value: 24, label: 'Riempimento9',           color: '#000000' },
    { value: 25, label: 'Riempimento10',          color: '#000000' },
    { value: 26, label: 'Riempimento11',          color: '#000000' },
    { value: 27, label: 'Riempimento12',          color: '#000000' },
    { value: 28, label: 'Riempimento13',          color: '#000000' },
];

export const GROUND_COLOR_MAP: Record<number, string> = Object.fromEntries(
    GROUND_ENTRIES.map(e => [e.value, e.color])
);

// ---------------------------------------------------------------------------
// Wall entries
// ---------------------------------------------------------------------------

export interface WallEntry {
    value: number;
    label: string;
    code: string;
    color: string;
}

export const WALL_ENTRIES: WallEntry[] = [
    { value: 0,  label: 'No muro',    code: '',   color: '#555555' },
    { value: 1,  label: 'Occludente', code: 'M0', color: '#008000' },
    { value: 4,  label: 'Muro1',      code: 'M1', color: '#008000' },
    { value: 12, label: 'Muro2',      code: 'M2', color: '#008000' },
    { value: 13, label: 'Muro3',      code: 'M3', color: '#008000' },
    { value: 14, label: 'Muro4',      code: 'M4', color: '#008000' },
    { value: 15, label: 'Muro5',      code: 'M5', color: '#008000' },
    { value: 2,  label: 'Fragile',    code: 'MB', color: '#FF0000' },
    { value: 3,  label: 'Porta',      code: 'P',  color: '#0000FF' },
    { value: 5,  label: 'Ingresso',   code: 'I',  color: '#006868' },
    { value: 6,  label: 'Uscita',     code: 'U',  color: '#006868' },
    { value: 7,  label: 'Decal1',     code: 'D1', color: '#EE82EE' },
    { value: 8,  label: 'Decal2',     code: 'D2', color: '#EE82EE' },
    { value: 9,  label: 'Decal3',     code: 'D3', color: '#EE82EE' },
    { value: 10, label: 'Decal4',     code: 'D4', color: '#EE82EE' },
    { value: 11, label: 'Decal5',     code: 'D5', color: '#EE82EE' },
    { value: 16, label: 'Decal6',     code: 'D6', color: '#EE82EE' },
    { value: 17, label: 'Decal7',     code: 'D7', color: '#EE82EE' },
    { value: 18, label: 'Decal8',     code: 'D8', color: '#EE82EE' },
];

export const WALL_COLOR_MAP: Record<number, string> = Object.fromEntries(
    WALL_ENTRIES.map(e => [e.value, e.color])
);

export const WALL_CODE_MAP: Record<number, string> = Object.fromEntries(
    WALL_ENTRIES.map(e => [e.value, e.code])
);

// ---------------------------------------------------------------------------
// Object entries (tab "Oggetti" — non nemici)
// ---------------------------------------------------------------------------

export interface ObjectEntry {
    value: number;
    label: string;
    emoji: string;
}

export const OBJECT_ENTRIES: ObjectEntry[] = [
    { value: -1, label: 'Testo (A)',           emoji: 'A' },
    { value: 0,  label: 'Cancella',           emoji: '✕' },
    { value: 1,  label: 'Giocatore',          emoji: '🧍' },
    { value: 2,  label: 'Chiave',             emoji: '🗝' },
    { value: 3,  label: 'Uscita',             emoji: '🚪' },
    { value: 4,  label: 'Candele',            emoji: '🕯' },
    { value: 12, label: 'Lucina',             emoji: '🔥' },
    { value: 13, label: 'OggettoNonPass.1',   emoji: '🪨' },
    { value: 14, label: 'OggettoNonPass.2',   emoji: '🏛' },
    { value: 15, label: 'OggettoPass.1',      emoji: '🏺' },
];

// ---------------------------------------------------------------------------
// Enemy entries (tab "Nemici")
// ---------------------------------------------------------------------------

export const ENEMY_ENTRIES: ObjectEntry[] = [
    { value: 5,  label: 'Zombie',   emoji: '🧟' },
    { value: 6,  label: 'Scheletro', emoji: '💀' },
    { value: 7,  label: 'Goblin',   emoji: '👺' },
    { value: 8,  label: 'Taurus',   emoji: '🐂' },
    { value: 10, label: 'Ghost',    emoji: '👻' },
    { value: 11, label: 'Bubu',     emoji: '👾' },
];

// Emoji usata nella cella per ogni ObjectType
export const OBJECT_LABEL_MAP: Record<number, string> = {
    1:  '🧍',
    2:  '🗝',
    3:  '🚪',
    4:  '🕯',
    5:  '🧟',
    6:  '💀',
    7:  '👺',
    8:  '🐂',
    10: '👻',
    11: '👾',
    12: '🔥',
    13: '🪨',
    14: '🏛',
    15: '🏺',
};

// Singleton: un solo esemplare per livello (Giocatore=1, Chiave=2, Uscita=3, Goblin=7)
export const SINGLETON_OBJECTS: Set<number> = new Set([1, 2, 3, 7]);

// Nemici: richiedono prompt per il path identifier
export const ENEMY_OBJECT_VALUES: Set<number> = new Set([5, 6, 7, 8, 10, 11]);

// Tipi che richiedono prompt (candele=4 per quantità; nemici per path)
export const NEEDS_PROMPT_OBJECTS: Set<number> = new Set([4, 5, 6, 7, 8, 10, 11]);

// Prefisso waypoint per tipo nemico (usato per auto-generazione ID)
export const ENEMY_PREFIX_MAP: Record<number, string> = {
    5:  'Z',  // Zombie
    6:  'S',  // Scheletro
    7:  'G',  // Goblin
    8:  'T',  // Taurus
    10: 'H',  // Ghost
    11: 'B',  // Bubu
};

export function isRiempimento(groundType: number): boolean {
    return groundType === 8
        || (groundType >= 17 && groundType <= 28);
}
