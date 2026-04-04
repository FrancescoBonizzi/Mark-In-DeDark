/**
 * Visualizzatore livelli .mark — Mark In DeDark
 *
 * Uso:
 *   npx tsx scripts/visualize-level.ts <percorso/al/livello.mark>
 *   npx tsx scripts/visualize-level.ts <percorso/al/livello.mark> --legend
 *
 * Output standard:
 *   - Mappa ASCII 2D (1 char/cella + separatori muro)
 *   - Statistiche
 *   - Sezione ANNOTAZIONI con i dati non codificabili nella mappa
 *     (testi, waypoint, quantità candele, varianti terreno)
 *
 * Con --legend: aggiunge la legenda completa di tutti i caratteri e convenzioni.
 *
 * REVERSIBILITÀ: mappa + annotazioni sono sufficienti per ricostruire il .mark.
 * Convenzione ownership muri:
 *   - Separatore verticale  tra C e C+1  → RIGHT wall di col C
 *   - Separatore orizzontale tra R e R+1 → BOTTOM wall di row R
 */

import { readFileSync } from 'fs';
import { resolve } from 'path';

// ---------------------------------------------------------------------------
// Costanti
// ---------------------------------------------------------------------------

const ROWS = 25;
const COLS = 45;

// ---------------------------------------------------------------------------
// Tabelle nomi (per annotazioni)
// ---------------------------------------------------------------------------

const GROUND_NAMES: Record<number, string> = {
    0: 'NonCella',
    1: 'Terra1', 6: 'Terra2', 9: 'Terra3', 10: 'Terra4', 11: 'Terra5',
    12: 'Terra6', 13: 'Terra7', 14: 'Terra8', 15: 'Terra9', 16: 'Terra10',
    3: 'TrappolaMorte', 4: 'TrappolaTimer', 5: 'TriggerGoblin', 7: 'Buio',
    8: 'Riempimento1', 17: 'Riempimento2', 18: 'Riempimento3', 19: 'Riempimento4',
    20: 'Riempimento5', 21: 'Riempimento6', 22: 'Riempimento7', 23: 'Riempimento8',
    24: 'Riempimento9', 25: 'Riempimento10', 26: 'Riempimento11', 27: 'Riempimento12',
    28: 'Riempimento13',
};

const OBJECT_NAMES: Record<number, string> = {
    0: 'Cancella', 1: 'Giocatore', 2: 'Chiave', 3: 'Uscita', 4: 'Candele',
    5: 'Zombie', 6: 'Scheletro', 7: 'Goblin', 8: 'Taurus', 10: 'Ghost',
    11: 'Bubu', 12: 'Brazier', 13: 'OstNonPass1', 14: 'OstNonPass2', 15: 'OstPass1',
};

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
// Caratteri celle
// ---------------------------------------------------------------------------

/**
 * Restituisce il singolo carattere che rappresenta il contenuto di una cella.
 *
 * Priorità: oggetto > terreno speciale > terreno normale
 * I Terra variant e i Riempimento variant vengono normalizzati a un singolo char
 * e dettagliati nella sezione ANNOTAZIONI.
 */
function cellChar(cell: Cell | null): string {
    if (!cell || cell.ground === 0) return ' ';

    // Oggetto (object=0 è "Cancella", trattato come assente)
    if (cell.object !== null && cell.object !== 0) {
        switch (cell.object) {
            case 1:  return '@'; // Giocatore
            case 2:  return 'K'; // Chiave
            case 3:  return 'E'; // Uscita
            case 4:  return 'c'; // Candele
            case 5:  return 'Z'; // Zombie
            case 6:  return 'S'; // Scheletro
            case 7:  return 'G'; // Goblin
            case 8:  return 'T'; // Taurus
            case 10: return 'H'; // Ghost
            case 11: return 'B'; // Bubu
            case 12: return '*'; // Brazier/Lucina
            case 13: return '#'; // OggettoNonPassabile1
            case 14: return '='; // OggettoNonPassabile2
            case 15: return '+'; // OggettoPassabile1
            default: return '?';
        }
    }

    // Terreno speciale
    switch (cell.ground) {
        case 3:  return 'x'; // TrappolaMorteIstantanea
        case 4:  return '^'; // TrappolaTemporizzata
        case 5:  return '~'; // TriggerGoblin
        case 7:  return '░'; // Buio
    }

    // Riempimento (non calpestabile)
    if (cell.ground === 8 || (cell.ground >= 17 && cell.ground <= 28)) {
        return '▓';
    }

    // Terra (calpestabile) — evidenzia celle con testo narrativo
    if (cell.stringCode) return '!';
    return '·';
}

// ---------------------------------------------------------------------------
// Caratteri muri — separatori verticali e orizzontali
//
// Ogni tipo di muro ha un char specifico.
// Muro variant (4, 12–15): funzionalmente = Occludente, mostrati come │/─
// Decal (7–11, 16–18):     visivi, non bloccanti, mostrati come :/·
// ---------------------------------------------------------------------------

/**
 * Char per un muro verticale (separatore tra due colonne adiacenti).
 * Usato come: │ ╎ ║ i o : (spazio)
 */
function vWallChar(wallId: number): string {
    switch (wallId) {
        case 0:                                     return ' '; // Niente
        case 1: case 4: case 12: case 13:
        case 14: case 15:                           return '│'; // Occludente / Muro variant
        case 2:                                     return '╎'; // Fragile
        case 3:                                     return '║'; // Porta
        case 5:                                     return 'i'; // Ingresso
        case 6:                                     return 'o'; // Uscita
        default:                                    return ':'; // Decal (7–11, 16–18)
    }
}

/**
 * Char per un muro orizzontale (separatore tra due righe adiacenti).
 * Usato come: ─ ╌ ═ i o . (spazio)
 */
function hWallChar(wallId: number): string {
    switch (wallId) {
        case 0:                                     return ' '; // Niente
        case 1: case 4: case 12: case 13:
        case 14: case 15:                           return '─'; // Occludente / Muro variant
        case 2:                                     return '╌'; // Fragile
        case 3:                                     return '═'; // Porta
        case 5:                                     return 'i'; // Ingresso
        case 6:                                     return 'o'; // Uscita
        default:                                    return '.'; // Decal (7–11, 16–18)
    }
}

/**
 * Separatore verticale tra col C e C+1 nella riga R.
 * Convenzione: il muro della cella SINISTRA (RIGHT wall) ha priorità.
 * Se entrambi 0, usa il muro della cella DESTRA (LEFT wall).
 */
function rightSep(left: Cell | null, right: Cell | null): string {
    const lw = left?.wallRight ?? 0;
    const rw = right?.wallLeft ?? 0;
    return vWallChar(lw !== 0 ? lw : rw);
}

/**
 * Riga separatrice orizzontale tra row R e R+1.
 * Ritorna una stringa di lunghezza (COLS*2 - 1) = 89 char.
 * Ogni posizione pari = wall char per la colonna c.
 * Ogni posizione dispari = char di intersezione (+/spazio).
 * Convenzione: il muro della riga SUPERIORE (BOTTOM wall) ha priorità.
 */
function buildWallRow(upper: (Cell | null)[], lower: (Cell | null)[]): string {
    const chars: string[] = [];
    for (let c = 0; c < COLS; c++) {
        const ub = upper[c]?.wallBottom ?? 0;
        const lt = lower[c]?.wallTop ?? 0;
        chars.push(hWallChar(ub !== 0 ? ub : lt));

        if (c < COLS - 1) {
            // Angolo di intersezione: c'è almeno un muro adiacente?
            const hHere  = (upper[c]?.wallBottom ?? 0) !== 0 || (lower[c]?.wallTop ?? 0) !== 0;
            const hRight = (upper[c+1]?.wallBottom ?? 0) !== 0 || (lower[c+1]?.wallTop ?? 0) !== 0;
            const vAbove = (upper[c]?.wallRight ?? 0) !== 0 || (upper[c+1]?.wallLeft ?? 0) !== 0;
            const vBelow = (lower[c]?.wallRight ?? 0) !== 0 || (lower[c+1]?.wallLeft ?? 0) !== 0;
            chars.push(hHere || hRight || vAbove || vBelow ? '+' : ' ');
        }
    }
    return chars.join('');
}

// ---------------------------------------------------------------------------
// Righello colonne
// ---------------------------------------------------------------------------

function buildColumnRuler(): string[] {
    // Ogni col c occupa la posizione 2*c nel contenuto riga (cell + separator, -1 sull'ultima)
    const tens = new Array<string>(COLS * 2 - 1).fill(' ');
    const ones = new Array<string>(COLS * 2 - 1).fill(' ');
    for (let c = 0; c < COLS; c++) {
        tens[c * 2] = Math.floor(c / 10) > 0 ? Math.floor(c / 10).toString() : (c === 0 ? '0' : ' ');
        ones[c * 2] = (c % 10).toString();
    }
    const prefix = '     '; // 5 spazi = allineamento con "NNN: "
    return [prefix + tens.join(''), prefix + ones.join('')];
}

// ---------------------------------------------------------------------------
// Annotazioni — dati non codificabili nella mappa
// ---------------------------------------------------------------------------

/**
 * Genera la sezione ANNOTAZIONI che contiene tutti i dati necessari per la
 * conversione inversa ASCII → .mark che non rientrano nella mappa visiva.
 */
function buildAnnotations(cells: (Cell | null)[][]): string[] {
    const texts:    string[] = [];
    const candleQs: string[] = [];
    const waypts:   string[] = [];
    const terraSub: string[] = [];
    const fillSub:  string[] = [];

    // Varianti Terra ≠ Terra1 (ground 6, 9–16)
    const TERRA_NON_DEFAULT = new Set([6, 9, 10, 11, 12, 13, 14, 15, 16]);
    // Varianti Riempimento ≠ Riempimento1 (ground 17–28)
    const FILL_NON_DEFAULT  = new Set([17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28]);

    for (let r = 0; r < ROWS; r++) {
        for (let c = 0; c < COLS; c++) {
            const cell = cells[r]![c];
            if (!cell) continue;

            // Testi narrativi
            if (cell.stringCode) {
                texts.push(`  ${r},${c}: ${cell.stringCode}`);
            }

            // Quantità candele ≠ 1
            if (cell.object === 4) {
                const n = cell.objData !== '' ? parseInt(cell.objData, 10) : 1;
                const qty = isNaN(n) ? 1 : n;
                if (qty !== 1) {
                    candleQs.push(`  ${r},${c}: ${qty}`);
                }
            }

            // Waypoint nemici
            if (cell.object !== null && ENEMY_TYPES.has(cell.object) && cell.objData) {
                const name = OBJECT_NAMES[cell.object] ?? String(cell.object);
                waypts.push(`  ${r},${c}: ${cell.objData}  (${name})`);
            }

            // Terra variant non default
            if (TERRA_NON_DEFAULT.has(cell.ground)) {
                const name = GROUND_NAMES[cell.ground] ?? String(cell.ground);
                terraSub.push(`  ${r},${c}: ground=${cell.ground}  (${name})`);
            }

            // Riempimento variant non default
            if (FILL_NON_DEFAULT.has(cell.ground)) {
                const name = GROUND_NAMES[cell.ground] ?? String(cell.ground);
                fillSub.push(`  ${r},${c}: ground=${cell.ground}  (${name})`);
            }
        }
    }

    const out: string[] = [];

    if (texts.length > 0) {
        out.push('TESTI (stringCode) — trigger narrativi:');
        out.push(...texts);
    }
    if (candleQs.length > 0) {
        out.push('CANDELE_QTY — quantità pacchi candele (solo se ≠ 1):');
        out.push(...candleQs);
    }
    if (waypts.length > 0) {
        out.push('WAYPOINTS — percorsi di patruglia nemici:');
        out.push(...waypts);
    }
    if (terraSub.length > 0) {
        out.push('TERRA_VARIANT — varianti pavimento (solo se ≠ Terra1=1, impatto visivo):');
        out.push(...terraSub);
    }
    if (fillSub.length > 0) {
        out.push('FILL_VARIANT — varianti riempimento (solo se ≠ Riempimento1=8, impatto visivo):');
        out.push(...fillSub);
    }

    return out;
}

// ---------------------------------------------------------------------------
// Statistiche
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
// Rendering principale
// ---------------------------------------------------------------------------

function buildLegend(): string[] {
    return [
        '┌─ LEGENDA CELLE ──────────────────────────────────────────────────────────────┐',
        '│ Oggetti:   @ Giocatore   K Chiave   E Uscita   c Candele   ! TriggerTesto   │',
        '│ Nemici:    Z Zombie   S Scheletro   G Goblin   T Taurus   H Ghost   B Bubu  │',
        '│ Ambiente:  * Brazier   # OstNonPass1   = OstNonPass2   + OstPass1           │',
        '│ Terreno:   · Pavimento(Terra*)   ░ Buio   ▓ Riempimento*   spazio=NonCella  │',
        '│ Trappole:  x TrappolaMorte   ^ TrappolaTimer   ~ TriggerGoblin              │',
        '│ (*variante esatta nelle ANNOTAZIONI; impatto solo visivo)                   │',
        '├─ LEGENDA SEPARATORI (muri) ─────────────────────────────────────────────────┤',
        '│ Verticali:   │=Occludente  ╎=Fragile  ║=Porta  i=Ingresso  o=Uscita  :=Decal│',
        '│ Orizzontali: ─=Occludente  ╌=Fragile  ═=Porta  i=Ingresso  o=Uscita  .=Decal│',
        '│ Muro variant (4,12–15): mostrato come │/─ — funzionalmente = Occludente     │',
        '│ + = angolo di intersezione tra muri adiacenti                               │',
        '├─ CONVENZIONE OWNERSHIP MURI ────────────────────────────────────────────────┤',
        '│ Separatore verticale  tra col C e C+1  → RIGHT wall di col C               │',
        '│ Separatore orizzontale tra row R e R+1 → BOTTOM wall di row R               │',
        '│ (se entrambi i lati hanno un muro, vince sinistro/superiore)                │',
        '└─────────────────────────────────────────────────────────────────────────────┘',
    ];
}

function renderLevel(
    cells: (Cell | null)[][],
    packId: number,
    dirty: number,
    showLegend: boolean,
): string {
    const out: string[] = [];

    // ── Header (necessario per conversione inversa) ──────────────────────────
    out.push(`pack=${packId} dirty=${dirty}`);
    out.push('');

    // ── Legenda + statistiche (opzionali) ───────────────────────────────────
    if (showLegend) {
        out.push(...buildLegend());
        out.push('');
        out.push('STATISTICHE:');
        out.push(...buildStats(cells));
        out.push('');
    }

    // ── Righello colonne ────────────────────────────────────────────────────
    const [ruler1, ruler2] = buildColumnRuler();
    out.push(ruler1!);
    out.push(ruler2!);
    out.push('');

    // ── Mappa ───────────────────────────────────────────────────────────────
    for (let r = 0; r < ROWS; r++) {
        let line = `${String(r).padStart(3)}: `;
        for (let c = 0; c < COLS; c++) {
            line += cellChar(cells[r]![c]!);
            if (c < COLS - 1) line += rightSep(cells[r]![c]!, cells[r]![c + 1]!);
        }
        out.push(line);

        if (r < ROWS - 1) {
            out.push('     ' + buildWallRow(cells[r]!, cells[r + 1]!));
        }
    }

    // ── Annotazioni (solo con --legend, per ispezione umana) ───────────────
    if (showLegend) {
        const annotations = buildAnnotations(cells);
        if (annotations.length > 0) {
            out.push('');
            out.push('ANNOTAZIONI:');
            out.push('');
            out.push(...annotations);
        }
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
