/**
 * visualize-level.ts — Mark In DeDark
 *
 * Converte un file .mark in formato ASCII editabile.
 * Uso:
 *   npx tsx scripts/visualize-level.ts <livello.mark> [--legend]
 *
 * Output di default: header + mappa + annotazioni (necessarie per round-trip).
 * Con --legend: aggiunge legenda visiva e statistiche in cima.
 *
 * ROUND-TRIP: l'output di questo script è input diretto di ascii-to-mark.ts.
 * Convenzione ownership muri:
 *   - Separatore verticale  tra C e C+1  → RIGHT wall di col C  (se C esiste)
 *   - Separatore orizzontale tra R e R+1 → BOTTOM wall di row R (se R esiste)
 *   - Se la cella primaria non esiste → LEFT / TOP della cella secondaria
 */

import { readFileSync } from 'fs';
import { resolve } from 'path';

// ---------------------------------------------------------------------------
// Costanti
// ---------------------------------------------------------------------------

const ROWS = 25;
const COLS = 45;

// ---------------------------------------------------------------------------
// Tabelle nomi (per annotazioni e legenda)
// ---------------------------------------------------------------------------


const ENEMY_TYPES = new Set([5, 6, 7, 8, 10, 11]);

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

// ---------------------------------------------------------------------------
// Parsing
// ---------------------------------------------------------------------------

function parseLevel(text: string): { cells: (Cell | null)[][], packId: number, dirty: number } {
    const lines = text.replace(/\r/g, '').split('\n')
        .map(l => l.trim())
        .filter(l => l.length > 0);

    const cells: (Cell | null)[][] = Array.from({ length: ROWS }, () =>
        new Array<Cell | null>(COLS).fill(null)
    );

    if (lines.length < 3) return { cells, packId: 1, dirty: 0 };

    const envParts = lines[2]!.split('|');
    const packId = parseInt(envParts[0] ?? '1', 10);
    const dirty = parseInt(envParts[1] ?? '0', 10);

    for (let i = 3; i < lines.length; i++) {
        const parts = lines[i]!.split('|');
        if (parts.length < 9) continue;

        const row = parseInt(parts[0]!, 10);
        const col = parseInt(parts[1]!, 10);
        if (isNaN(row) || isNaN(col) || row < 0 || row >= ROWS || col < 0 || col >= COLS) continue;

        const ground = parseInt(parts[2]!, 10);
        if (isNaN(ground)) continue;

        const objectStr = parts[3]!.trim();
        const object: number | null = objectStr !== '' ? parseInt(objectStr, 10) : null;
        const objData = parts[4]!.trim();
        const wallTop    = parseInt(parts[5]!, 10) || 0;
        const wallRight  = parseInt(parts[6]!, 10) || 0;
        const wallBottom = parseInt(parts[7]!, 10) || 0;
        const wallLeft   = parseInt(parts[8]!, 10) || 0;
        const stringCode = (parts[9] ?? '').trim();

        cells[row]![col] = { ground, object, objData, wallTop, wallRight, wallBottom, wallLeft, stringCode };
    }

    return { cells, packId, dirty };
}

// ---------------------------------------------------------------------------
// Caratteri muri
// ---------------------------------------------------------------------------

function vWallChar(wallId: number): string {
    switch (wallId) {
        case 0:                                             return ' ';
        case 1: case 4: case 12: case 13: case 14: case 15: return '│';
        case 2:                                             return '╎';
        case 3:                                             return '║';
        case 5:                                             return 'i';
        case 6:                                             return 'o';
        default:                                            return ':'; // decal
    }
}

function hWallChar(wallId: number): string {
    switch (wallId) {
        case 0:                                             return ' ';
        case 1: case 4: case 12: case 13: case 14: case 15: return '─';
        case 2:                                             return '╌';
        case 3:                                             return '═';
        case 5:                                             return 'i';
        case 6:                                             return 'o';
        default:                                            return '.'; // decal
    }
}

/** Default wall ID che il convertitore assegna a un dato carattere separatore. */
function vWallDefaultId(ch: string): number {
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

function hWallDefaultId(ch: string): number {
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
// Caratteri celle
// ---------------------------------------------------------------------------

function cellChar(cell: Cell | null): string {
    if (!cell || cell.ground === 0) return ' ';

    if (cell.object !== null && cell.object !== 0) {
        switch (cell.object) {
            case 1:  return '@';
            case 2:  return 'K';
            case 3:  return 'E';
            case 4:  return 'c';
            case 5:  return 'Z';
            case 6:  return 'S';
            case 7:  return 'G';
            case 8:  return 'T';
            case 10: return 'H';
            case 11: return 'B';
            case 12: return '*';
            case 13: return '#';
            case 14: return '=';
            case 15: return '+';
            default: return '?';
        }
    }

    switch (cell.ground) {
        case 3:  return 'x';
        case 4:  return '^';
        case 5:  return '~';
        case 7:  return '░';
    }

    if (cell.ground === 8 || (cell.ground >= 17 && cell.ground <= 28)) return '▓';

    if (cell.stringCode) return '!';
    return '·';
}

// ---------------------------------------------------------------------------
// Separatori riga cella
// ---------------------------------------------------------------------------

function exists(cell: Cell | null): boolean {
    return cell !== null && cell.ground !== 0;
}

/**
 * Separatore verticale tra col C e C+1 nella riga R.
 * Usa RIGHT di C (primario) se non-zero, altrimenti LEFT di C+1.
 */
function vSep(left: Cell | null, right: Cell | null): string {
    const lw = left?.wallRight ?? 0;
    const rw = right?.wallLeft ?? 0;
    return vWallChar(lw !== 0 ? lw : rw);
}

/**
 * Riga muri orizzontali tra row R e R+1.
 * Usa BOTTOM di R (primario) se non-zero, altrimenti TOP di R+1.
 * Posizioni dispari = spazio (nessun marker di intersezione).
 */
function buildWallRow(upper: (Cell | null)[], lower: (Cell | null)[]): string {
    const chars: string[] = [];
    for (let c = 0; c < COLS; c++) {
        const ub = upper[c]?.wallBottom ?? 0;
        const lt = lower[c]?.wallTop ?? 0;
        chars.push(hWallChar(ub !== 0 ? ub : lt));
        if (c < COLS - 1) chars.push(' ');
    }
    return chars.join('');
}

// ---------------------------------------------------------------------------
// Righello colonne
// ---------------------------------------------------------------------------

function buildColumnRuler(): string[] {
    const tens = new Array<string>(COLS * 2 - 1).fill(' ');
    const ones = new Array<string>(COLS * 2 - 1).fill(' ');
    for (let c = 0; c < COLS; c++) {
        tens[c * 2] = Math.floor(c / 10) > 0 ? Math.floor(c / 10).toString() : (c === 0 ? '0' : ' ');
        ones[c * 2] = (c % 10).toString();
    }
    const prefix = '     ';
    return [prefix + tens.join(''), prefix + ones.join('')];
}

// ---------------------------------------------------------------------------
// Annotazioni
// ---------------------------------------------------------------------------

/**
 * Determina a quale cella/lato il convertitore assegnerebbe un muro verticale
 * tra col C e C+1 alla riga R.
 */
function vAssignment(
    cells: (Cell | null)[][],
    r: number, c: number
): { r: number, c: number, side: 'RIGHT' | 'LEFT' } {
    if (exists(cells[r]![c]!)) return { r, c, side: 'RIGHT' };
    return { r, c: c + 1, side: 'LEFT' };
}

/**
 * Determina a quale cella/lato il convertitore assegnerebbe un muro orizzontale
 * tra row R e R+1 alla col C.
 */
function hAssignment(
    cells: (Cell | null)[][],
    r: number, c: number
): { r: number, c: number, side: 'BOTTOM' | 'TOP' } {
    if (exists(cells[r]![c]!)) return { r, c, side: 'BOTTOM' };
    return { r: r + 1, c, side: 'TOP' };
}

type MuriEntry = { r: number, c: number, side: 'TOP' | 'RIGHT' | 'BOTTOM' | 'LEFT', id: number };

function buildAnnotations(cells: (Cell | null)[][]): string[] {
    const testi:    string[] = [];
    const candele:  string[] = [];
    const waypts:   string[] = [];
    const terra:    string[] = [];
    const muriMap = new Map<string, MuriEntry>();

    const addMuri = (r: number, c: number, side: 'TOP' | 'RIGHT' | 'BOTTOM' | 'LEFT', id: number) => {
        const key = `${String(r).padStart(3)},${String(c).padStart(3)},${side}`;
        muriMap.set(key, { r, c, side, id });
    };

    // ── Separatori verticali ────────────────────────────────────────────────
    for (let r = 0; r < ROWS; r++) {
        for (let c = 0; c < COLS - 1; c++) {
            const lw = cells[r]![c]?.wallRight ?? 0;
            const rw = cells[r]![c + 1]?.wallLeft ?? 0;
            const effectiveId = lw !== 0 ? lw : rw;
            if (effectiveId === 0) continue;
            const ch = vWallChar(effectiveId);
            if (effectiveId !== vWallDefaultId(ch)) {
                const a = vAssignment(cells, r, c);
                addMuri(a.r, a.c, a.side, effectiveId);
            }
        }
    }

    // ── Separatori orizzontali ──────────────────────────────────────────────
    for (let r = 0; r < ROWS - 1; r++) {
        for (let c = 0; c < COLS; c++) {
            const ub = cells[r]![c]?.wallBottom ?? 0;
            const lt = cells[r + 1]![c]?.wallTop ?? 0;
            const effectiveId = ub !== 0 ? ub : lt;
            if (effectiveId === 0) continue;
            const ch = hWallChar(effectiveId);
            if (effectiveId !== hWallDefaultId(ch)) {
                const a = hAssignment(cells, r, c);
                addMuri(a.r, a.c, a.side, effectiveId);
            }
        }
    }

    // ── Muri secondari nascosti ─────────────────────────────────────────────
    // TOP: nascosto se la cella sopra esiste e ha BOTTOM non-zero
    for (let r = 0; r < ROWS; r++) {
        for (let c = 0; c < COLS; c++) {
            const cell = cells[r]![c];
            if (!cell) continue;

            if (cell.wallTop !== 0 && r > 0) {
                const above = cells[r - 1]![c] ?? null;
                if (exists(above) && (above?.wallBottom ?? 0) !== 0) {
                    addMuri(r, c, 'TOP', cell.wallTop);
                }
            }

            if (cell.wallLeft !== 0 && c > 0) {
                const leftCell = cells[r]![c - 1] ?? null;
                if (exists(leftCell) && (leftCell?.wallRight ?? 0) !== 0) {
                    addMuri(r, c, 'LEFT', cell.wallLeft);
                }
            }
        }
    }

    // ── Celle ───────────────────────────────────────────────────────────────
    const TERRA_NON_DEFAULT = new Set([6, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28]);

    for (let r = 0; r < ROWS; r++) {
        for (let c = 0; c < COLS; c++) {
            const cell = cells[r]![c];
            if (!cell || cell.ground === 0) continue;

            if (cell.stringCode) {
                testi.push(`  ${r},${c}: ${cell.stringCode}`);
            }

            if (cell.object === 4) {
                const n = cell.objData !== '' ? parseInt(cell.objData, 10) : 1;
                const qty = isNaN(n) ? 1 : n;
                if (qty !== 1) candele.push(`  ${r},${c}: ${qty}`);
            }

            if (cell.object !== null && ENEMY_TYPES.has(cell.object) && cell.objData) {
                waypts.push(`  ${r},${c}: ${cell.objData}`);
            }

            if (TERRA_NON_DEFAULT.has(cell.ground)) {
                terra.push(`  ${r},${c}: ${cell.ground}`);
            }
        }
    }

    // ── Assembla output ─────────────────────────────────────────────────────
    const muriEntries = Array.from(muriMap.values()).sort((a, b) => {
        if (a.r !== b.r) return a.r - b.r;
        if (a.c !== b.c) return a.c - b.c;
        const sideOrder = { TOP: 0, RIGHT: 1, BOTTOM: 2, LEFT: 3 };
        return sideOrder[a.side] - sideOrder[b.side];
    });

    const out: string[] = [];

    if (testi.length > 0) {
        out.push('TESTI:');
        out.push(...testi);
    }
    if (candele.length > 0) {
        out.push('CANDELE:');
        out.push(...candele);
    }
    if (waypts.length > 0) {
        out.push('WAYPOINTS:');
        out.push(...waypts);
    }
    if (terra.length > 0) {
        out.push('TERRA:');
        out.push(...terra);
    }
    if (muriEntries.length > 0) {
        out.push('MURI:');
        for (const e of muriEntries) {
            out.push(`  ${e.r},${e.c},${e.side}: ${e.id}`);
        }
    }

    return out;
}

// ---------------------------------------------------------------------------
// Statistiche (solo con --legend)
// ---------------------------------------------------------------------------

function buildStats(cells: (Cell | null)[][]): string[] {
    let playerCount = 0, keyCount = 0, exitCount = 0;
    let candlePacks = 0, candleTotal = 0, brazierCount = 0, textTriggers = 0;
    const enemyCounts: Record<string, number> = {};

    for (let r = 0; r < ROWS; r++) {
        for (let c = 0; c < COLS; c++) {
            const cell = cells[r]![c];
            if (!cell) continue;
            if (cell.stringCode) textTriggers++;
            if (cell.object === null || cell.object === 0) continue;
            switch (cell.object) {
                case 1:  playerCount++; break;
                case 2:  keyCount++; break;
                case 3:  exitCount++; break;
                case 4: {
                    candlePacks++;
                    const n = cell.objData !== '' ? parseInt(cell.objData, 10) : 1;
                    candleTotal += isNaN(n) ? 1 : n;
                    break;
                }
                case 5:  enemyCounts['Zombie']    = (enemyCounts['Zombie']    ?? 0) + 1; break;
                case 6:  enemyCounts['Scheletro'] = (enemyCounts['Scheletro'] ?? 0) + 1; break;
                case 7:  enemyCounts['Goblin']    = (enemyCounts['Goblin']    ?? 0) + 1; break;
                case 8:  enemyCounts['Taurus']    = (enemyCounts['Taurus']    ?? 0) + 1; break;
                case 10: enemyCounts['Ghost']     = (enemyCounts['Ghost']     ?? 0) + 1; break;
                case 11: enemyCounts['Bubu']      = (enemyCounts['Bubu']      ?? 0) + 1; break;
                case 12: brazierCount++; break;
            }
        }
    }

    const enemyList = Object.entries(enemyCounts);
    return [
        `  Giocatore: ${playerCount}   Chiave: ${keyCount}   Uscita: ${exitCount}`,
        `  Pacchi candele: ${candlePacks}   Candele totali: ${candleTotal}`,
        `  Brazier: ${brazierCount}   Trigger testo: ${textTriggers}`,
        enemyList.length > 0
            ? `  Nemici: ${enemyList.map(([k, v]) => `${k}×${v}`).join('  ')}`
            : '  Nemici: nessuno',
    ];
}

// ---------------------------------------------------------------------------
// Legenda (solo con --legend)
// ---------------------------------------------------------------------------

function buildLegend(): string[] {
    return [
        '┌─ LEGENDA CELLE ──────────────────────────────────────────────────────────────┐',
        '│ Oggetti:   @ Giocatore   K Chiave   E Uscita   c Candele   ! TriggerTesto   │',
        '│ Nemici:    Z Zombie   S Scheletro   G Goblin   T Taurus   H Ghost   B Bubu  │',
        '│ Ambiente:  * Brazier   # OstNonPass1   = OstNonPass2   + OstPass1           │',
        '│ Terreno:   · Pavimento(Terra1)   ░ Buio   ▓ Riempimento   spazio=NonCella  │',
        '│ Trappole:  x TrappolaMorte   ^ TrappolaTimer   ~ TriggerGoblin              │',
        '│ (varianti terreno/fill indicate in sezione TERRA; varianti muri in MURI)    │',
        '├─ LEGENDA SEPARATORI (muri) ─────────────────────────────────────────────────┤',
        '│ Verticali:   │=Occludente  ╎=Fragile  ║=Porta  i=Ingresso  o=Uscita  :=Decal│',
        '│ Orizzontali: ─=Occludente  ╌=Fragile  ═=Porta  i=Ingresso  o=Uscita  .=Decal│',
        '│ Varianti ID (4,12–15) → char │/─; decal ID (7–11) → char :/. — override:MURI│',
        '├─ ANNOTAZIONI ───────────────────────────────────────────────────────────────┤',
        '│ TESTI:      r,c: stringCode   (trigger narrativi)                           │',
        '│ CANDELE:    r,c: qty          (solo se quantità ≠ 1)                        │',
        '│ WAYPOINTS:  r,c: waypoints    (percorsi nemici)                             │',
        '│ TERRA:      r,c: groundId     (varianti terra/fill non default)             │',
        '│ MURI:       r,c,LATO: wallId  (ID muro non deducibile dal carattere)        │',
        '└─────────────────────────────────────────────────────────────────────────────┘',
    ];
}

// ---------------------------------------------------------------------------
// Rendering principale
// ---------------------------------------------------------------------------

function renderLevel(
    cells: (Cell | null)[][],
    packId: number,
    dirty: number,
    showLegend: boolean,
): string {
    const out: string[] = [];

    // ── Legenda + statistiche (opzionali) ───────────────────────────────────
    if (showLegend) {
        out.push(...buildLegend());
        out.push('');
        out.push('STATISTICHE:');
        out.push(...buildStats(cells));
        out.push('');
        out.push('─'.repeat(80));
        out.push('');
    }

    // ── Header ──────────────────────────────────────────────────────────────
    out.push(`pack=${packId} dirty=${dirty}`);
    out.push('');

    // ── Righello colonne ────────────────────────────────────────────────────
    const [ruler1, ruler2] = buildColumnRuler();
    out.push(ruler1!);
    out.push(ruler2!);
    out.push('');

    // ── Mappa ───────────────────────────────────────────────────────────────
    for (let r = 0; r < ROWS; r++) {
        let line = `${String(r).padStart(3)}: `;
        for (let c = 0; c < COLS; c++) {
            line += cellChar(cells[r]![c] ?? null);
            if (c < COLS - 1) line += vSep(cells[r]![c] ?? null, cells[r]![c + 1] ?? null);
        }
        out.push(line);

        if (r < ROWS - 1) {
            const wallRow = buildWallRow(cells[r]!, cells[r + 1]!);
            // Ometti righe muri se tutte spazio
            if (wallRow.trim().length > 0) {
                out.push('     ' + wallRow);
            }
        }
    }

    // ── Annotazioni ─────────────────────────────────────────────────────────
    const annotations = buildAnnotations(cells);
    if (annotations.length > 0) {
        out.push('');
        out.push(...annotations);
    }

    return out.join('\n');
}

// ---------------------------------------------------------------------------
// Entrypoint
// ---------------------------------------------------------------------------

const args = process.argv.slice(2);
const showLegend = args.includes('--legend');
const positional = args.filter(a => !a.startsWith('--'));

if (positional.length === 0) {
    console.error('Uso: npx tsx scripts/visualize-level.ts <livello.mark> [--legend]');
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

const { cells, packId, dirty } = parseLevel(content);
console.log(renderLevel(cells, packId, dirty, showLegend));
