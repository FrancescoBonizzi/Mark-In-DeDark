/**
 * packCatalog.ts — Disponibilità texture per pack ambiente.
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
