/**
 * packCatalog.ts — Disponibilità texture e preview per pack ambiente.
 *
 * Dati derivati dall'analisi degli spritesheet Spritesheet-0..5.json:
 * - Cells (Terra): tutti i pack hanno cell1-5; pack2 ha anche cell6-10
 * - Decals: tutti i pack hanno decal1-5; pack2 ha anche decal6-8
 * - SpecialWalls (M1-M5): tutti i pack hanno wall_special_1..5
 */

// ---------------------------------------------------------------------------
// Mappatura valore editor → numero nel pack
// ---------------------------------------------------------------------------

/** groundType editor → numero cella pack (cell1..10) */
const GROUND_TO_CELL: Readonly<Record<number, number>> = {
    1:  1,   // Terra1
    6:  2,   // Terra2
    9:  3,   // Terra3
    10: 4,   // Terra4
    11: 5,   // Terra5
    12: 6,   // Terra6  — solo pack2
    13: 7,   // Terra7  — solo pack2
    14: 8,   // Terra8  — solo pack2
    15: 9,   // Terra9  — solo pack2
    16: 10,  // Terra10 — solo pack2
};

/** Numero cella → label "Terra N" mostrata nella preview */
const CELL_TO_LABEL: Readonly<Record<number, string>> = {
    1: 'Terra1', 2: 'Terra2', 3: 'Terra3', 4: 'Terra4', 5: 'Terra5',
    6: 'Terra6', 7: 'Terra7', 8: 'Terra8', 9: 'Terra9', 10: 'Terra10',
};

/** Frame dei decal per pack (decal_1..N). Labels: D1..D8 */
const DECAL_FRAMES: Readonly<Record<number, CellFrame[]>> = {
    1: [{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
    2: [{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
    3: [{ss:1,x:777,y:389},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:1,x:1359,y:389},{ss:1,x:583,y:583}],
    4: [{ss:1,x:777,y:389},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:1,x:1359,y:389},{ss:1,x:583,y:583}],
    5: [{ss:2,x:195,y:1},{ss:2,x:583,y:1},{ss:2,x:971,y:1},{ss:2,x:1359,y:1},{ss:2,x:1747,y:1}],
    6: [{ss:2,x:195,y:1},{ss:2,x:583,y:1},{ss:0,x:675,y:1859},{ss:2,x:1359,y:1},{ss:2,x:1747,y:1}],
    7: [{ss:2,x:195,y:1},{ss:2,x:583,y:1},{ss:2,x:971,y:1},{ss:2,x:1359,y:1},{ss:2,x:1747,y:1}],
    8: [{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
};

/** Frame dei special wall per pack (wall_special_1..5). Labels: M1..M5 */
const SPECWALL_FRAMES: Readonly<Record<number, CellFrame[]>> = {
    1: [{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
    2: [{ss:1,x:1,y:1553},{ss:0,x:195,y:1353},{ss:1,x:389,y:1},{ss:1,x:777,y:1},{ss:1,x:1165,y:1}],
    3: [{ss:1,x:1553,y:777},{ss:1,x:971,y:971},{ss:0,x:195,y:735},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
    4: [{ss:1,x:1553,y:777},{ss:1,x:971,y:971},{ss:1,x:1553,y:1359},{ss:1,x:1553,y:1553},{ss:1,x:1747,y:1553}],
    5: [{ss:2,x:195,y:777},{ss:2,x:195,y:1165},{ss:0,x:413,y:1},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
    6: [{ss:2,x:195,y:777},{ss:2,x:195,y:1165},{ss:0,x:413,y:1},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
    7: [{ss:2,x:195,y:777},{ss:2,x:195,y:1165},{ss:0,x:413,y:1},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
    8: [{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
};

/**
 * Frame dei 13 Riempimento — identici per tutti i pack, tutti su SS0.
 * Ordine: riempimento_1..13 → ground values 8, 17..28.
 */
const RIEMP_FRAMES: ReadonlyArray<{ frame: CellFrame; label: string }> = [
    { frame: { ss: 0, x: 503,  y: 1665 }, label: 'Riemp1'  },
    { frame: { ss: 0, x: 777,  y: 713  }, label: 'Riemp2'  },
    { frame: { ss: 0, x: 583,  y: 1315 }, label: 'Riemp3'  },
    { frame: { ss: 0, x: 697,  y: 1639 }, label: 'Riemp4'  },
    { frame: { ss: 0, x: 869,  y: 1833 }, label: 'Riemp5'  },
    { frame: { ss: 0, x: 931,  y: 195  }, label: 'Riemp6'  },
    { frame: { ss: 0, x: 995,  y: 1    }, label: 'Riemp7'  },
    { frame: { ss: 0, x: 931,  y: 389  }, label: 'Riemp8'  },
    { frame: { ss: 0, x: 777,  y: 1113 }, label: 'Riemp9'  },
    { frame: { ss: 0, x: 737,  y: 195  }, label: 'Riemp10' },
    { frame: { ss: 0, x: 737,  y: 389  }, label: 'Riemp11' },
    { frame: { ss: 0, x: 583,  y: 1121 }, label: 'Riemp12' },
    { frame: { ss: 0, x: 737,  y: 919  }, label: 'Riemp13' },
];

/** wallType editor → numero decal pack (decal1..8) */
const WALL_TO_DECAL: Readonly<Record<number, number>> = {
    7:  1,   // Decal1
    8:  2,   // Decal2
    9:  3,   // Decal3
    10: 4,   // Decal4
    11: 5,   // Decal5
    16: 6,   // Decal6 — solo pack2
    17: 7,   // Decal7 — solo pack2
    18: 8,   // Decal8 — solo pack2
};

// ---------------------------------------------------------------------------
// Disponibilità per pack (da analisi spritesheet)
// ---------------------------------------------------------------------------

const PACK_MAX_CELLS: Readonly<Record<number, number>> = {
    1: 5, 2: 10, 3: 5, 4: 5, 5: 5, 6: 5, 7: 5, 8: 5,
};

const PACK_MAX_DECALS: Readonly<Record<number, number>> = {
    1: 5, 2: 8, 3: 5, 4: 5, 5: 5, 6: 5, 7: 5, 8: 5,
};

// ---------------------------------------------------------------------------
// Coordinate frame per la preview (da analisi diretta dei JSON spritesheet)
// Ogni entry: [ss_index, x, y] — tutti i frame sono 192×192px
// ---------------------------------------------------------------------------

interface CellFrame { ss: number; x: number; y: number }

/**
 * Frame placeholder usato dallo spritesheet quando un pack non ha quella texture.
 * Rilevabile confrontando le coordinate — stessa costante in tutti i pack.
 */
const PLACEHOLDER: CellFrame = { ss: 0, x: 195, y: 1353 };

function isPlaceholder(f: CellFrame): boolean {
    return f.ss === PLACEHOLDER.ss && f.x === PLACEHOLDER.x && f.y === PLACEHOLDER.y;
}

/** Indice specwall (0-based) → wall value editor (M1=4, M2=12, M3=13, M4=14, M5=15) */
const SPECWALL_IDX_TO_VALUE = [4, 12, 13, 14, 15] as const;

/** Coordinate dei frame cell1..cellN per ciascun pack (ordine: cell1, cell2, ...) */
const CELL_FRAMES: Readonly<Record<number, CellFrame[]>> = {
    1: [{ss:0,x:931,y:907},{ss:0,x:891,y:1631},{ss:0,x:1189,y:1},{ss:0,x:1125,y:907},{ss:0,x:195,y:1353}],
    2: [{ss:0,x:931,y:907},{ss:0,x:891,y:1631},{ss:0,x:1189,y:1},{ss:0,x:1125,y:907},{ss:1,x:195,y:389},{ss:1,x:195,y:971},{ss:1,x:195,y:1553},{ss:1,x:583,y:195},{ss:1,x:1165,y:195},{ss:1,x:1553,y:1}],
    3: [{ss:1,x:971,y:1359},{ss:1,x:1165,y:971},{ss:1,x:1747,y:971},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
    4: [{ss:1,x:971,y:1359},{ss:1,x:1165,y:971},{ss:1,x:1747,y:971},{ss:2,x:1,y:1},{ss:2,x:1,y:583}],
    5: [{ss:2,x:1,y:583},{ss:2,x:195,y:1359},{ss:2,x:389,y:195},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
    6: [{ss:2,x:1,y:583},{ss:2,x:195,y:1359},{ss:2,x:389,y:195},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
    7: [{ss:2,x:1,y:583},{ss:2,x:195,y:1359},{ss:2,x:389,y:195},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
    8: [{ss:2,x:1,y:583},{ss:2,x:195,y:1359},{ss:2,x:389,y:195},{ss:0,x:195,y:1353},{ss:0,x:195,y:1353}],
};

const FRAME_SIZE = 192;
const PREVIEW_CELL_SIZE = 40; // dimensione di render nella preview

// Cache delle immagini spritesheet già caricate
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

// ---------------------------------------------------------------------------
// Interfaccia pubblica
// ---------------------------------------------------------------------------

export interface PackAvailability {
    /** Set di groundType values disponibili per questo pack */
    availableGrounds: ReadonlySet<number>;
    /** Set di wallType values per decal disponibili per questo pack */
    availableDecals: ReadonlySet<number>;
    /** Set di wallType values per muri speciali (M1-M5) con texture reale in questo pack */
    availableSpecialWalls: ReadonlySet<number>;
}

/**
 * Restituisce la disponibilità di ground e decal per il pack indicato.
 * SpecialWalls (M1-M5) sono sempre disponibili per tutti i pack.
 */
export function getPackAvailability(packId: number): PackAvailability {
    const maxCells  = PACK_MAX_CELLS[packId]  ?? 5;
    const maxDecals = PACK_MAX_DECALS[packId] ?? 5;

    const availableGrounds = new Set<number>();
    for (const [groundStr, cellNum] of Object.entries(GROUND_TO_CELL)) {
        if (cellNum <= maxCells) {
            availableGrounds.add(Number(groundStr));
        }
    }

    const availableDecals = new Set<number>();
    for (const [wallStr, decalNum] of Object.entries(WALL_TO_DECAL)) {
        if (decalNum <= maxDecals) {
            availableDecals.add(Number(wallStr));
        }
    }

    // Muri speciali: disponibile solo se il frame non è il placeholder
    const availableSpecialWalls = new Set<number>();
    const specwallFrames = SPECWALL_FRAMES[packId] ?? [];
    specwallFrames.forEach((frame, idx) => {
        if (!isPlaceholder(frame)) {
            const wallValue = SPECWALL_IDX_TO_VALUE[idx];
            if (wallValue !== undefined) availableSpecialWalls.add(wallValue);
        }
    });

    return { availableGrounds, availableDecals, availableSpecialWalls };
}

/**
 * Renderizza la preview delle celle del pack nel container indicato.
 * Crea una riga di canvas 40×40 con le texture reali del pack.
 * @param packId   id del pack (1-8)
 * @param container elemento DOM che verrà riempito (innerHTML azzerato)
 * @param assetsRoot BASE_URL Vite (es. "/mark-in-dedark/")
 */
export function renderPackPreview(
    packId: number,
    container: HTMLElement,
    assetsRoot: string
): void {
    const frames = CELL_FRAMES[packId];
    if (!frames) { container.innerHTML = ''; return; }

    container.innerHTML = '';

    // Tutti i canvas da disegnare (Terra + Riempimento)
    const canvases: Array<{ canvas: HTMLCanvasElement; frame: CellFrame }> = [];

    function makeItem(frame: CellFrame, label: string): void {
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

    // --- Celle Terra ---
    frames.forEach((frame, idx) => {
        makeItem(frame, CELL_TO_LABEL[idx + 1] ?? `Cell${idx + 1}`);
    });

    function addSep(): void {
        const sep = document.createElement('div');
        sep.className = 'pack-preview-sep';
        container.appendChild(sep);
    }

    // --- Riempimento (uguali per tutti i pack) ---
    addSep();
    for (const { frame, label } of RIEMP_FRAMES) {
        makeItem(frame, label);
    }

    // --- Decal ---
    const decalFrames = DECAL_FRAMES[packId];
    if (decalFrames && decalFrames.length > 0) {
        addSep();
        decalFrames.forEach((frame, idx) => {
            makeItem(frame, `D${idx + 1}`);
        });
    }

    // --- Special Walls (M1-M5) ---
    const specwallFrames = SPECWALL_FRAMES[packId];
    if (specwallFrames && specwallFrames.length > 0) {
        addSep();
        specwallFrames.forEach((frame, idx) => {
            makeItem(frame, `M${idx + 1}`);
        });
    }

    // --- Carica spritesheet in parallelo e disegna ---
    const neededSS = new Set(canvases.map(c => c.frame.ss));

    neededSS.forEach(ssIdx => {
        const url = `${assetsRoot}assets/images/spriteSheets/Spritesheet-${ssIdx}.png`;
        loadImage(url).then(img => {
            for (const { canvas, frame } of canvases) {
                if (frame.ss !== ssIdx) continue;
                const ctx = canvas.getContext('2d');
                if (!ctx) continue;
                ctx.clearRect(0, 0, PREVIEW_CELL_SIZE, PREVIEW_CELL_SIZE);
                ctx.drawImage(
                    img,
                    frame.x, frame.y, FRAME_SIZE, FRAME_SIZE,
                    0, 0, PREVIEW_CELL_SIZE, PREVIEW_CELL_SIZE
                );
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
