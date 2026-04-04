/**
 * ascii-to-mark.ts — Mark In DeDark
 *
 * Converte un file ASCII (output di level-editor.ts) in formato .mark.
 * Uso:
 *   npx tsx scripts/ascii-to-mark.ts <livello.ascii> [-o output.mark]
 *
 * Se -o non specificato, stampa su stdout.
 *
 * CONVENZIONE MURI (stessa di level-editor.ts):
 *   - Separatore verticale  tra C e C+1  → RIGHT di col C  (se C esiste, altrimenti LEFT di C+1)
 *   - Separatore orizzontale tra R e R+1 → BOTTOM di row R (se R esiste, altrimenti TOP di R+1)
 *   - Sezione MURI: override espliciti per ID non deducibili dal carattere
 *
 * DEFAULTS quando le annotazioni sono assenti:
 *   · → Terra1 (ground=1)   ▓ → Riempimento1 (ground=8)
 *   │ / ─ → occludente (ID=1)   : / . → decal (ID=7)
 *   candele qty=1   waypoints=''   stringCode=''
 */

import { readFileSync, writeFileSync } from 'fs';
import { resolve } from 'path';

// ---------------------------------------------------------------------------
// Costanti
// ---------------------------------------------------------------------------

const ROWS = 25;
const COLS = 45;

// ---------------------------------------------------------------------------
// Alias terra/fill → ground ID
// ---------------------------------------------------------------------------

/** Mappa alias leggibile → ground ID (es. 'terra2' → 6, 'fill7' → 22). */
const ALIAS_TO_GROUND_ID: Record<string, number> = {
    terra1: 1,  terra2: 6,  terra3: 9,  terra4: 10, terra5: 11,
    terra6: 12, terra7: 13, terra8: 14, terra9: 15, terra10: 16,
    fill1: 8,   fill2: 17,  fill3: 18,  fill4: 19,  fill5: 20,
    fill6: 21,  fill7: 22,  fill8: 23,  fill9: 24,  fill10: 25,
    fill11: 26, fill12: 27, fill13: 28,
};

function resolveGroundAlias(val: string): number {
    const alias = val.trim().toLowerCase();
    const id = ALIAS_TO_GROUND_ID[alias];
    if (id !== undefined) return id;
    // Fallback numerico (es. "1", "6") per robustezza
    const n = parseInt(val, 10);
    return isNaN(n) ? 1 : n;
}

// ---------------------------------------------------------------------------
// Struttura dati
// ---------------------------------------------------------------------------

interface Cell {
    ground: number;
    object: number | null;
    objData: string;
    wallTop: number;
    wallRight: number;
    wallBottom: number;
    wallLeft: number;
    stringCode: string;
}

function emptyCell(): Cell {
    return { ground: 1, object: null, objData: '', wallTop: 0, wallRight: 0, wallBottom: 0, wallLeft: 0, stringCode: '' };
}

// ---------------------------------------------------------------------------
// Tabelle di conversione
// ---------------------------------------------------------------------------

/** Char cella → { ground, object } */
function charToCell(ch: string): { ground: number, object: number | null } | null {
    switch (ch) {
        case ' ':  return null;                      // cella nulla
        case '·':  return { ground: 1, object: null };
        case '!':  return { ground: 1, object: null }; // stesso di ·, stringCode da TESTI
        case '@':  return { ground: 1, object: 1 };
        case 'K':  return { ground: 1, object: 2 };
        case 'E':  return { ground: 1, object: 3 };
        case 'c':  return { ground: 1, object: 4 };
        case 'Z':  return { ground: 1, object: 5 };
        case 'S':  return { ground: 1, object: 6 };
        case 'G':  return { ground: 1, object: 7 };
        case 'T':  return { ground: 1, object: 8 };
        case 'H':  return { ground: 1, object: 10 };
        case 'B':  return { ground: 1, object: 11 };
        case '*':  return { ground: 1, object: 12 };
        case '#':  return { ground: 1, object: 13 };
        case '=':  return { ground: 1, object: 14 };
        case '+':  return { ground: 1, object: 15 };
        case 'x':  return { ground: 3, object: null };
        case '^':  return { ground: 4, object: null };
        case '~':  return { ground: 5, object: null };
        case '░':  return { ground: 7, object: null };
        case '▓':  return { ground: 8, object: null };
        default:   return null;
    }
}

/** Default wall ID per carattere separatore verticale. */
function vCharToId(ch: string): number {
    switch (ch) {
        case ' ': return 0;
        case '│': return 1;
        case '╎': return 2;
        case '║': return 3;
        case 'i': return 5;
        case 'o': return 6;
        case ':': return 7;
        default:  return 0;
    }
}

/** Default wall ID per carattere separatore orizzontale. */
function hCharToId(ch: string): number {
    switch (ch) {
        case ' ': return 0;
        case '─': return 1;
        case '╌': return 2;
        case '═': return 3;
        case 'i': return 5;
        case 'o': return 6;
        case '.': return 7;
        default:  return 0;
    }
}

// ---------------------------------------------------------------------------
// Parsing del formato ASCII
// ---------------------------------------------------------------------------

interface ParsedAscii {
    packId: number;
    dirty: number;
    cells: (Cell | null)[][];
}

function parseAscii(text: string): ParsedAscii {
    const rawLines = text.replace(/\r/g, '').split('\n');

    // Griglia: null = cella non presente (ground=0)
    const cells: (Cell | null)[][] = Array.from({ length: ROWS }, () =>
        new Array<Cell | null>(COLS).fill(null)
    );

    let packId = 1;
    let dirty = 0;

    // ── Annotazioni ─────────────────────────────────────────────────────────
    const testi    = new Map<string, string>();      // 'r,c' → stringCode
    const candele  = new Map<string, string>();      // 'r,c' → objData (raw string)
    const cancella = new Set<string>();              // 'r,c' → object=0
    const waypts   = new Map<string, string>();      // 'r,c' → waypoints
    const terraOvr = new Map<string, number>();      // 'r,c' → groundId
    const muriOvr  = new Map<string, number>();      // 'r,c,SIDE' → wallId

    // ── Parsing linee ────────────────────────────────────────────────────────
    type Section = 'NONE' | 'TESTI' | 'CANDELE' | 'CANCELLA' | 'WAYPOINTS' | 'TERRA' | 'MURI';
    let section: Section = 'NONE';

    // Mappa righe ASCII: indice riga → indice nella griglia
    // Le righe cella matchano /^\s*(\d+):\s/
    // Le righe muro matchano /^ {5}/  (cinque spazi iniziali)
    // Per processare i muri, dobbiamo sapere quale coppia di righe griglia separano.

    // Raccogliamo le righe cella e le righe muro con il loro ordine
    type CellLine  = { type: 'cell',  gridRow: number, content: string };
    type WallLine  = { type: 'wall',  afterGridRow: number, content: string };
    type MapLine   = CellLine | WallLine;

    const mapLines: MapLine[] = [];
    let lastGridRow = -1;
    let foundHeader = false;

    for (const raw of rawLines) {
        // Header
        const headerMatch = raw.match(/^pack=(\d+)\s+dirty=(\d+)/);
        if (headerMatch) {
            packId = parseInt(headerMatch[1]!, 10);
            dirty  = parseInt(headerMatch[2]!, 10);
            foundHeader = true;
            section = 'NONE';
            continue;
        }

        // Sezioni annotazioni
        if (raw.trim() === 'TESTI:')     { section = 'TESTI';     continue; }
        if (raw.trim() === 'CANDELE:')   { section = 'CANDELE';   continue; }
        if (raw.trim() === 'CANCELLA:')  { section = 'CANCELLA';  continue; }
        if (raw.trim() === 'WAYPOINTS:') { section = 'WAYPOINTS'; continue; }
        if (raw.trim() === 'TERRA:')     { section = 'TERRA';     continue; }
        if (raw.trim() === 'MURI:')      { section = 'MURI';      continue; }

        // Voci annotazioni
        if (section !== 'NONE') {
            const entryMatch = raw.match(/^\s{2}(\d+),(\d+)(?:,(TOP|RIGHT|BOTTOM|LEFT))?:\s*(.+)$/);
            if (entryMatch) {
                const er = parseInt(entryMatch[1]!, 10);
                const ec = parseInt(entryMatch[2]!, 10);
                const side = entryMatch[3];
                const val  = entryMatch[4]!.trim();
                const key = `${er},${ec}`;
                switch (section) {
                    case 'TESTI':     testi.set(key, val); break;
                    case 'CANDELE':   candele.set(key, val); break;
                    case 'CANCELLA':  cancella.add(key); break;
                    case 'WAYPOINTS': waypts.set(key, val); break;
                    case 'TERRA':     terraOvr.set(key, resolveGroundAlias(val)); break;
                    case 'MURI':
                        if (side) muriOvr.set(`${key},${side}`, parseInt(val, 10));
                        break;
                }
            }
            continue;
        }

        // Skip linee non mappa se header non trovato (legenda, statistiche, ecc.)
        if (!foundHeader) continue;

        // Riga cella: `NNN: ...`
        const cellMatch = raw.match(/^(\s*\d+): (.*)$/);
        if (cellMatch) {
            const gridRow = parseInt(cellMatch[1]!.trim(), 10);
            if (gridRow >= 0 && gridRow < ROWS) {
                mapLines.push({ type: 'cell', gridRow, content: cellMatch[2]! });
                lastGridRow = gridRow;
            }
            continue;
        }

        // Riga muri: inizia con 5 spazi
        if (raw.startsWith('     ') && lastGridRow >= 0) {
            mapLines.push({ type: 'wall', afterGridRow: lastGridRow, content: raw.slice(5) });
            continue;
        }
    }

    // ── Prima passata: costruisci la griglia celle ───────────────────────────
    for (const ml of mapLines) {
        if (ml.type !== 'cell') continue;
        const { gridRow, content } = ml;
        // Ogni cella è alla posizione 2*c nel content (separatori verticali a 2*c+1)
        for (let c = 0; c < COLS; c++) {
            const ch = content[c * 2] ?? ' ';
            const parsed = charToCell(ch);
            if (parsed === null) {
                cells[gridRow]![c] = null; // cella nulla
            } else {
                const cell = emptyCell();
                cell.ground = parsed.ground;
                cell.object = parsed.object;
                cells[gridRow]![c] = cell;
            }
        }
    }

    // ── Funzione helper: esiste la cella (ground != 0)? ──────────────────────
    const cellExists = (r: number, c: number): boolean => {
        if (r < 0 || r >= ROWS || c < 0 || c >= COLS) return false;
        const cell = cells[r]![c] ?? null;
        return cell !== null && cell.ground !== 0;
    };

    const getCell = (r: number, c: number): Cell | null => {
        if (r < 0 || r >= ROWS || c < 0 || c >= COLS) return null;
        return cells[r]![c] ?? null;
    };

    const setWall = (r: number, c: number, side: 'TOP' | 'RIGHT' | 'BOTTOM' | 'LEFT', id: number) => {
        const cell = getCell(r, c);
        if (!cell) return;
        switch (side) {
            case 'TOP':    cell.wallTop    = id; break;
            case 'RIGHT':  cell.wallRight  = id; break;
            case 'BOTTOM': cell.wallBottom = id; break;
            case 'LEFT':   cell.wallLeft   = id; break;
        }
    };

    // ── Seconda passata: assegna muri dai separatori ─────────────────────────
    for (const ml of mapLines) {
        if (ml.type === 'cell') {
            const { gridRow: r, content } = ml;
            // Separatori verticali: posizioni dispari 2*c+1 tra celle c e c+1
            for (let c = 0; c < COLS - 1; c++) {
                const ch = content[c * 2 + 1] ?? ' ';
                const wallId = vCharToId(ch);
                if (wallId === 0) continue;
                // Assegna a RIGHT di c se c esiste, altrimenti LEFT di c+1
                if (cellExists(r, c)) {
                    setWall(r, c, 'RIGHT', wallId);
                } else if (cellExists(r, c + 1)) {
                    setWall(r, c + 1, 'LEFT', wallId);
                }
            }
        } else {
            const { afterGridRow: r, content } = ml;
            // Separatori orizzontali: posizioni pari 2*c
            for (let c = 0; c < COLS; c++) {
                const ch = content[c * 2] ?? ' ';
                const wallId = hCharToId(ch);
                if (wallId === 0) continue;
                // Assegna a BOTTOM di r se r esiste, altrimenti TOP di r+1
                if (cellExists(r, c)) {
                    setWall(r, c, 'BOTTOM', wallId);
                } else if (cellExists(r + 1, c)) {
                    setWall(r + 1, c, 'TOP', wallId);
                }
            }
        }
    }

    // ── Applica override MURI ────────────────────────────────────────────────
    Array.from(muriOvr.entries()).forEach(([key, wallId]) => {
        const [rStr, cStr, side] = key.split(',') as [string, string, string];
        const r = parseInt(rStr, 10);
        const c = parseInt(cStr, 10);
        setWall(r, c, side as 'TOP' | 'RIGHT' | 'BOTTOM' | 'LEFT', wallId);
    });

    // ── Applica override TERRA ────────────────────────────────────────────────
    Array.from(terraOvr.entries()).forEach(([key, groundId]) => {
        const [rStr, cStr] = key.split(',') as [string, string];
        const r = parseInt(rStr, 10);
        const c = parseInt(cStr, 10);
        const cell = getCell(r, c);
        if (cell) cell.ground = groundId;
    });

    // ── Applica TESTI ─────────────────────────────────────────────────────────
    Array.from(testi.entries()).forEach(([key, code]) => {
        const [rStr, cStr] = key.split(',') as [string, string];
        const r = parseInt(rStr, 10);
        const c = parseInt(cStr, 10);
        const cell = getCell(r, c);
        if (cell) cell.stringCode = code;
    });

    // ── Applica CANDELE ───────────────────────────────────────────────────────
    Array.from(candele.entries()).forEach(([key, objData]) => {
        const [rStr, cStr] = key.split(',') as [string, string];
        const r = parseInt(rStr, 10);
        const c = parseInt(cStr, 10);
        const cell = getCell(r, c);
        if (cell) cell.objData = objData;
    });

    // ── Applica CANCELLA ──────────────────────────────────────────────────────
    Array.from(cancella.values()).forEach((key) => {
        const [rStr, cStr] = key.split(',') as [string, string];
        const r = parseInt(rStr, 10);
        const c = parseInt(cStr, 10);
        const cell = getCell(r, c);
        if (cell) cell.object = 0;
    });

    // ── Applica WAYPOINTS ─────────────────────────────────────────────────────
    Array.from(waypts.entries()).forEach(([key, wp]) => {
        const [rStr, cStr] = key.split(',') as [string, string];
        const r = parseInt(rStr, 10);
        const c = parseInt(cStr, 10);
        const cell = getCell(r, c);
        if (cell) cell.objData = wp;
    });

    return { packId, dirty, cells };
}

// ---------------------------------------------------------------------------
// Serializzazione → .mark
// ---------------------------------------------------------------------------

function serializeMark(packId: number, dirty: number, cells: (Cell | null)[][]): string {
    const lines: string[] = [
        'GameGridSerializerV4',
        '25|45',
        `${packId}|${dirty}`,
    ];

    for (let r = 0; r < ROWS; r++) {
        for (let c = 0; c < COLS; c++) {
            const cell = cells[r]![c];
            if (!cell || cell.ground === 0) continue;

            const objStr  = cell.object !== null ? String(cell.object) : '';
            const line = [
                r, c,
                cell.ground,
                objStr,
                cell.objData,
                cell.wallTop,
                cell.wallRight,
                cell.wallBottom,
                cell.wallLeft,
                cell.stringCode,
            ].join('|');

            lines.push(line);
        }
    }

    return lines.join('\n') + '\n';
}

// ---------------------------------------------------------------------------
// Entrypoint
// ---------------------------------------------------------------------------

const args = process.argv.slice(2);
const outIdx = args.indexOf('-o');
const outFile = outIdx >= 0 ? args[outIdx + 1] : undefined;
const positional = args.filter((a, i) => !a.startsWith('-') && args[i - 1] !== '-o');

if (positional.length === 0) {
    console.error('Uso: npx tsx scripts/ascii-to-mark.ts <livello.ascii> [-o output.mark]');
    process.exit(1);
}

const filePath = resolve(positional[0]!);
let content: string;
try {
    content = readFileSync(filePath, 'utf-8');
} catch (err) {
    console.error(`Impossibile leggere il file: ${filePath}`);
    console.error((err as NodeJS.ErrnoException).message);
    process.exit(1);
}

const { packId, dirty, cells } = parseAscii(content);
const markContent = serializeMark(packId, dirty, cells);

if (outFile) {
    const outPath = resolve(outFile);
    writeFileSync(outPath, markContent, 'utf-8');
    console.log(`Scritto: ${outPath}`);
} else {
    process.stdout.write(markContent);
}
