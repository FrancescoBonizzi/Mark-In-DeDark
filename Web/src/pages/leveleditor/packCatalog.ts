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

    return { availableGrounds, availableDecals };
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

    // Raccoglie i canvas da disegnare
    const canvases: Array<{ canvas: HTMLCanvasElement; frame: CellFrame }> = [];

    for (const frame of frames) {
        const canvas = document.createElement('canvas');
        canvas.width  = PREVIEW_CELL_SIZE;
        canvas.height = PREVIEW_CELL_SIZE;
        canvas.className = 'pack-preview-cell';
        canvas.title = `SS${frame.ss} (${frame.x},${frame.y})`;
        container.appendChild(canvas);
        canvases.push({ canvas, frame });
    }

    // Raggruppa per spritesheet e carica in parallelo
    const neededSS = new Set(frames.map(f => f.ss));

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
            // fallback: rettangolo colorato se l'immagine non carica
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
