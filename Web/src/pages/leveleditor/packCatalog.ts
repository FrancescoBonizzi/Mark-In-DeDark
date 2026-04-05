/**
 * packCatalog.ts — Disponibilità texture e preview per pack ambiente.
 *
 * Tutti i dati provengono da generatedSpritepacks.ts, generato a build-time
 * da scripts/generate-spritepack-manifest.ts scansionando gli spritesheet JSON.
 * Non modificare i dati qui — rigenera il manifesto.
 */

import {
    PACK_IDS,
    SPRITEPACK_MANIFEST,
    RIEMPIMENTO_FRAMES,
    type FrameCoord,
} from '../../generatedSpritepacks';

// ---------------------------------------------------------------------------
// Mappatura valore editor → indice nel pack (0-based)
// ---------------------------------------------------------------------------

/** groundType editor → indice cella (0-based) nel pack */
const GROUND_TO_IDX: Readonly<Record<number, number>> = {
    1:  0,   // Terra1
    6:  1,   // Terra2
    9:  2,   // Terra3
    10: 3,   // Terra4
    11: 4,   // Terra5
    12: 5,   // Terra6
    13: 6,   // Terra7
    14: 7,   // Terra8
    15: 8,   // Terra9
    16: 9,   // Terra10
};

/** wallType editor → indice decal (0-based) nel pack */
const WALL_TO_DECAL_IDX: Readonly<Record<number, number>> = {
    7:  0,   // Decal1
    8:  1,   // Decal2
    9:  2,   // Decal3
    10: 3,   // Decal4
    11: 4,   // Decal5
    16: 5,   // Decal6
    17: 6,   // Decal7
    18: 7,   // Decal8
};

/** wallType editor → indice muro speciale (0-based) nel pack */
const WALL_TO_SPECWALL_IDX: Readonly<Record<number, number>> = {
    4:  0,   // M1
    12: 1,   // M2
    13: 2,   // M3
    14: 3,   // M4
    15: 4,   // M5
};

/** Label Terra per indice cella (0-based) */
const CELL_IDX_TO_LABEL: Readonly<Record<number, string>> = {
    0: 'Terra1', 1: 'Terra2', 2: 'Terra3', 3: 'Terra4', 4: 'Terra5',
    5: 'Terra6', 6: 'Terra7', 7: 'Terra8', 8: 'Terra9', 9: 'Terra10',
};

// ---------------------------------------------------------------------------
// Interfaccia pubblica
// ---------------------------------------------------------------------------

export interface PackAvailability {
    /** Set di groundType values disponibili (frame real) per questo pack */
    availableGrounds: ReadonlySet<number>;
    /** Set di wallType values per decal con frame real in questo pack */
    availableDecals: ReadonlySet<number>;
    /** Set di wallType values per muri speciali (M1-M5) con frame real */
    availableSpecialWalls: ReadonlySet<number>;
}

/**
 * Restituisce la disponibilità di ground, decal e muri speciali per il pack.
 * Considera "disponibile" solo i frame marcati `real: true` nel manifesto.
 */
export function getPackAvailability(packId: number): PackAvailability {
    const pd = SPRITEPACK_MANIFEST[packId];

    const availableGrounds = new Set<number>();
    if (pd) {
        for (const [wallStr, idx] of Object.entries(GROUND_TO_IDX)) {
            if (pd.cells[idx]?.real) availableGrounds.add(Number(wallStr));
        }
    }

    const availableDecals = new Set<number>();
    if (pd) {
        for (const [wallStr, idx] of Object.entries(WALL_TO_DECAL_IDX)) {
            if (pd.decals[idx]?.real) availableDecals.add(Number(wallStr));
        }
    }

    const availableSpecialWalls = new Set<number>();
    if (pd) {
        for (const [wallStr, idx] of Object.entries(WALL_TO_SPECWALL_IDX)) {
            if (pd.specwalls[idx]?.real) availableSpecialWalls.add(Number(wallStr));
        }
    }

    return { availableGrounds, availableDecals, availableSpecialWalls };
}

// ---------------------------------------------------------------------------
// Preview canvas
// ---------------------------------------------------------------------------

const FRAME_SIZE = 192;
const PREVIEW_CELL_SIZE = 40;

const imageCache = new Map<string, HTMLImageElement>();

function loadImage(url: string): Promise<HTMLImageElement> {
    const cached = imageCache.get(url);
    if (cached) return Promise.resolve(cached);
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => { imageCache.set(url, img); resolve(img); };
        img.onerror = () => reject(new Error(`Impossibile caricare: ${url}`));
        img.src = url;
    });
}

/**
 * Renderizza la preview delle texture del pack nel container.
 * Sezioni: Terra | Riempimento | Decal | Muri speciali
 */
export function renderPackPreview(
    packId: number,
    container: HTMLElement,
    assetsRoot: string
): void {
    container.innerHTML = '';

    const pd = SPRITEPACK_MANIFEST[packId];
    if (!pd) return;

    const canvases: Array<{ canvas: HTMLCanvasElement; frame: FrameCoord }> = [];

    function makeItem(frame: FrameCoord, label: string): void {
        const wrapper = document.createElement('div');
        wrapper.className = 'pack-preview-item';

        const canvas = document.createElement('canvas');
        canvas.width  = PREVIEW_CELL_SIZE;
        canvas.height = PREVIEW_CELL_SIZE;
        canvas.className = 'pack-preview-cell';
        canvas.title = label;

        const lbl = document.createElement('span');
        lbl.className = 'pack-preview-item-label';
        lbl.textContent = label;

        wrapper.appendChild(canvas);
        wrapper.appendChild(lbl);
        container.appendChild(wrapper);
        canvases.push({ canvas, frame });
    }

    function addSep(): void {
        const sep = document.createElement('div');
        sep.className = 'pack-preview-sep';
        container.appendChild(sep);
    }

    // Terra — solo frame reali
    pd.cells.forEach((pf, idx) => {
        if (!pf.real) return;
        makeItem(pf.frame, CELL_IDX_TO_LABEL[idx] ?? `Cell${idx + 1}`);
    });

    // Riempimento — sempre presenti per tutti i pack
    addSep();
    RIEMPIMENTO_FRAMES.forEach((frame, idx) => {
        makeItem(frame, `Riemp${idx + 1}`);
    });

    // Decal — solo frame reali
    const realDecals = pd.decals.filter(pf => pf.real);
    if (realDecals.length > 0) {
        addSep();
        pd.decals.forEach((pf, idx) => {
            if (!pf.real) return;
            makeItem(pf.frame, `D${idx + 1}`);
        });
    }

    // Muri speciali — solo frame reali
    const realSpecwalls = pd.specwalls.filter(pf => pf.real);
    if (realSpecwalls.length > 0) {
        addSep();
        pd.specwalls.forEach((pf, idx) => {
            if (!pf.real) return;
            makeItem(pf.frame, `M${idx + 1}`);
        });
    }

    // Carica spritesheet e disegna
    const neededSS = new Set(canvases.map(c => c.frame.ss));
    neededSS.forEach(ssIdx => {
        const url = `${assetsRoot}assets/images/spriteSheets/Spritesheet-${ssIdx}.png`;
        loadImage(url).then(img => {
            for (const { canvas, frame } of canvases) {
                if (frame.ss !== ssIdx) continue;
                const ctx = canvas.getContext('2d');
                if (!ctx) continue;
                ctx.clearRect(0, 0, PREVIEW_CELL_SIZE, PREVIEW_CELL_SIZE);
                ctx.drawImage(img, frame.x, frame.y, FRAME_SIZE, FRAME_SIZE,
                                   0, 0, PREVIEW_CELL_SIZE, PREVIEW_CELL_SIZE);
            }
        }).catch(() => {
            for (const { canvas, frame } of canvases) {
                if (frame.ss !== ssIdx) continue;
                const ctx = canvas.getContext('2d');
                if (!ctx) continue;
                ctx.fillStyle = '#3a1020';
                ctx.fillRect(0, 0, PREVIEW_CELL_SIZE, PREVIEW_CELL_SIZE);
            }
        });
    });
}

/** Pack disponibili (da manifesto generato). */
export { PACK_IDS };
