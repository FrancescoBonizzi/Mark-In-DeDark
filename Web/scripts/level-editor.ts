/**
 * level-editor.ts — Mark In DeDark
 *
 * Converte un file .mark in formato ASCII editabile, o genera una griglia vuota.
 * Uso:
 *   npx tsx scripts/level-editor.ts <livello.mark> [--legend]
 *   npx tsx scripts/level-editor.ts --new [--pack=N] [--dirty=N]
 *   npx tsx scripts/level-editor.ts --packs
 *
 * Output di default: header + mappa + annotazioni (necessarie per round-trip).
 * Con --legend: aggiunge legenda visiva e statistiche in cima.
 * Con --new: genera griglia vuota (25×45 NonCella), pronta per il disegno.
 * Con --packs: mostra i texture pack disponibili e gli alias terra/fill usabili.
 * Con --help: guida completa per creare livelli da zero (anche per AI).
 *
 * ROUND-TRIP: l'output di questo script è input diretto di ascii-to-mark.ts.
 * Convenzione ownership muri:
 *   - Separatore verticale  tra C e C+1  → RIGHT wall di col C  (se C esiste)
 *   - Separatore orizzontale tra R e R+1 → BOTTOM wall di row R (se R esiste)
 *   - Se la cella primaria non esiste → LEFT / TOP della cella secondaria
 */

import { readFileSync, readdirSync } from 'fs';
import { resolve, dirname } from 'path';
import { fileURLToPath } from 'url';

// ---------------------------------------------------------------------------
// Costanti
// ---------------------------------------------------------------------------

const ROWS = 25;
const COLS = 45;

const SCRIPT_DIR = dirname(fileURLToPath(import.meta.url));
const SPRITESHEET_DIR = resolve(SCRIPT_DIR, '../public/assets/images/spriteSheets');

// ---------------------------------------------------------------------------
// Alias terra/fill (per sezione TERRA — leggibili da umani)
// ---------------------------------------------------------------------------

/** Mappa ground ID → alias leggibile (es. 6 → 'terra2', 22 → 'fill7'). */
const GROUND_ID_TO_ALIAS: Record<number, string> = {
    // Terra walkable (default terra1=1, mai scritto)
    6: 'terra2', 9: 'terra3', 10: 'terra4', 11: 'terra5',
    12: 'terra6', 13: 'terra7', 14: 'terra8', 15: 'terra9', 16: 'terra10',
    // Riempimento fill (default fill1=8, mai scritto)
    17: 'fill2', 18: 'fill3', 19: 'fill4', 20: 'fill5', 21: 'fill6',
    22: 'fill7', 23: 'fill8', 24: 'fill9', 25: 'fill10', 26: 'fill11',
    27: 'fill12', 28: 'fill13',
};

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


type MuriEntry = { r: number, c: number, side: 'TOP' | 'RIGHT' | 'BOTTOM' | 'LEFT', id: number };

function buildAnnotations(cells: (Cell | null)[][]): string[] {
    const testi:    string[] = [];
    const candele:  string[] = [];
    const cancella: string[] = [];
    const waypts:   string[] = [];
    const terra:    string[] = [];
    const muriMap = new Map<string, MuriEntry>();

    const addMuri = (r: number, c: number, side: 'TOP' | 'RIGHT' | 'BOTTOM' | 'LEFT', id: number) => {
        const key = `${String(r).padStart(3)},${String(c).padStart(3)},${side}`;
        muriMap.set(key, { r, c, side, id });
    };

    // ── Separatori verticali ────────────────────────────────────────────────
    // Per ogni separatore tra col C e C+1: il convertitore assegna il defaultId
    // del char a RIGHT di C (se esiste) oppure a LEFT di C+1.
    // Usiamo MURI per: ID non-default, muri sul lato secondario, zero-out del primario.
    for (let r = 0; r < ROWS; r++) {
        for (let c = 0; c < COLS - 1; c++) {
            const leftCell = cells[r]![c] ?? null;
            const rightCell = cells[r]![c + 1] ?? null;
            const lw = leftCell?.wallRight ?? 0;
            const rw = rightCell?.wallLeft ?? 0;
            if (lw === 0 && rw === 0) continue;

            const effectiveId = lw !== 0 ? lw : rw;
            const ch = vWallChar(effectiveId);
            const defaultId = vWallDefaultId(ch);

            if (exists(leftCell)) {
                // Il convertitore assegna defaultId a RIGHT di C
                if (lw !== defaultId) addMuri(r, c, 'RIGHT', lw);
                if (rw !== 0) addMuri(r, c + 1, 'LEFT', rw);
            } else if (exists(rightCell)) {
                // Il convertitore assegna defaultId a LEFT di C+1
                if (rw !== defaultId) addMuri(r, c + 1, 'LEFT', rw);
            }
        }
    }

    // ── Separatori orizzontali ──────────────────────────────────────────────
    // Per ogni separatore tra row R e R+1: il convertitore assegna il defaultId
    // del char a BOTTOM di R (se esiste) oppure a TOP di R+1.
    for (let r = 0; r < ROWS - 1; r++) {
        for (let c = 0; c < COLS; c++) {
            const upperCell = cells[r]![c] ?? null;
            const lowerCell = cells[r + 1]![c] ?? null;
            const ub = upperCell?.wallBottom ?? 0;
            const lt = lowerCell?.wallTop ?? 0;
            if (ub === 0 && lt === 0) continue;

            const effectiveId = ub !== 0 ? ub : lt;
            const ch = hWallChar(effectiveId);
            const defaultId = hWallDefaultId(ch);

            if (exists(upperCell)) {
                // Il convertitore assegna defaultId a BOTTOM di R
                if (ub !== defaultId) addMuri(r, c, 'BOTTOM', ub);
                if (lt !== 0) addMuri(r + 1, c, 'TOP', lt);
            } else if (exists(lowerCell)) {
                // Il convertitore assegna defaultId a TOP di R+1
                if (lt !== defaultId) addMuri(r + 1, c, 'TOP', lt);
            }
        }
    }

    // ── Muri di bordo ──────────────────────────────────────────────────────
    // TOP di row 0, BOTTOM di row ROWS-1, LEFT di col 0, RIGHT di col COLS-1
    // non hanno separatore nell'ASCII → vanno sempre in MURI.
    for (let c = 0; c < COLS; c++) {
        const topCell = cells[0]![c] ?? null;
        if (exists(topCell) && (topCell?.wallTop ?? 0) !== 0) {
            addMuri(0, c, 'TOP', topCell!.wallTop);
        }
        const botCell = cells[ROWS - 1]![c] ?? null;
        if (exists(botCell) && (botCell?.wallBottom ?? 0) !== 0) {
            addMuri(ROWS - 1, c, 'BOTTOM', botCell!.wallBottom);
        }
    }
    for (let r = 0; r < ROWS; r++) {
        const leftCell = cells[r]![0] ?? null;
        if (exists(leftCell) && (leftCell?.wallLeft ?? 0) !== 0) {
            addMuri(r, 0, 'LEFT', leftCell!.wallLeft);
        }
        const rightCell = cells[r]![COLS - 1] ?? null;
        if (exists(rightCell) && (rightCell?.wallRight ?? 0) !== 0) {
            addMuri(r, COLS - 1, 'RIGHT', rightCell!.wallRight);
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

            if (cell.object === 0) {
                cancella.push(`  ${r},${c}: 0`);
            }

            if (cell.object === 4 && cell.objData !== '') {
                candele.push(`  ${r},${c}: ${cell.objData}`);
            }

            if (cell.object !== null && ENEMY_TYPES.has(cell.object) && cell.objData) {
                waypts.push(`  ${r},${c}: ${cell.objData}`);
            }

            if (TERRA_NON_DEFAULT.has(cell.ground)) {
                const alias = GROUND_ID_TO_ALIAS[cell.ground] ?? `id${cell.ground}`;
                terra.push(`  ${r},${c}: ${alias}`);
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
    if (cancella.length > 0) {
        out.push('CANCELLA:');
        out.push(...cancella);
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
        '│ TERRA:      r,c: terra2..5  /  fill2..13  (varianti terreno/riempimento)    │',
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
// Help completo (--help)
// ---------------------------------------------------------------------------

function buildHelp(): string {
    return `
════════════════════════════════════════════════════════════════════════════════
  MARK IN DEDARK — Guida completa al level design (formato ASCII)
════════════════════════════════════════════════════════════════════════════════

WORKFLOW
────────
  1. Genera una griglia vuota:
       npx tsx scripts/level-editor.ts --new --pack=1 > livello.ascii

  2. Disegna il livello nel file .ascii (mappa + annotazioni opzionali).

  3. Converti in .mark:
       npx tsx scripts/ascii-to-mark.ts livello.ascii -o livello.mark

  4. Valida:
       npx tsx scripts/validate-level.ts livello.mark

  5. Per visualizzare un .mark esistente:
       npx tsx scripts/level-editor.ts livello.mark
       npx tsx scripts/level-editor.ts livello.mark --legend   (+ legenda e statistiche)

  6. Per vedere i texture pack disponibili:
       npx tsx scripts/level-editor.ts --packs


STRUTTURA DEL FILE .ascii
─────────────────────────
  pack=1 dirty=0            ← header obbligatorio (pack 1–8, dirty 0–100)

       0         1   ...    ← righello colonne (ignorato dal convertitore)
       0123456789...

    0: ·│·│·│· ▓ ▓         ← righe cella:  "NNN: <contenuto>"
       ─   ─                ← righe muri:   "     <contenuto>" (5 spazi)
    1: ...

  TESTI:                    ← sezioni annotazioni (solo se necessarie)
    r,c: stringCode
  CANDELE:
    r,c: qty
  CANCELLA:
    r,c: 0
  WAYPOINTS:
    r,c: Z-1-1
  TERRA:
    r,c: terra2
  MURI:
    r,c,BOTTOM: 4


REGOLE MAPPA — CELLE (posizioni pari nelle righe cella)
───────────────────────────────────────────────────────
  Char  Significato           Note
  ────  ──────────────────    ────────────────────────────────────────
  (sp)  NonCella / vuoto      Fuori dalla mappa, nessuna cella generata
   ·    Pavimento Terra1      Default calpestabile; override con TERRA:
   ░    Buio                  Calpestabile ma non illuminato
   ▓    Riempimento fill1     Non calpestabile; override variante con TERRA:
   x    TrappolaMorte         Pavimento — uccide istantaneamente
   ^    TrappolaTimer         Pavimento — uccide con ritardo
   ~    TriggerGoblin         Pavimento — attiva goblin all'ingresso
   @    Giocatore             ESATTAMENTE 1 per livello (obbligatorio)
   K    Chiave                Raccoglibile; sblocca l'uscita
   E    Uscita                Porta finale; almeno 1 (obbligatorio)
   c    Candele               Luce raccoglibile; qty default=1, override con CANDELE:
   Z    Zombie                Nemico lento; segue il giocatore
   S    Scheletro             Nemico pattuglia; waypoint in WAYPOINTS:
   G    Goblin                Nemico veloce; attivato da TriggerGoblin (~)
   T    Taurus                Nemico a carica; sfonda muri Fragile (╎/╌)
   H    Ghost                 Nemico che attacca solo chi si muove
   B    Bubu                  Nemico sabotatore; spegne le candele
   *    Brazier               Fonte di luce fissa (ambiente)
   #    OstNonPass1           Ostacolo non attraversabile (sprite 1)
   =    OstNonPass2           Ostacolo non attraversabile (sprite 2)
   +    OstPass1              Ostacolo attraversabile (sprite 1)
   !    TriggerTesto          Pavimento con stringCode; testo in TESTI:


REGOLE MAPPA — SEPARATORI VERTICALI (posizioni dispari nelle righe cella)
─────────────────────────────────────────────────────────────────────────
  Char  Muro                  Note
  ────  ──────────────────    ────────────────────
  (sp)  Nessun muro
   │    Occludente            Muro solido standard
   ╎    Fragile               Taurus lo sfonda con la carica
   ║    Porta                 Aperta dalla chiave
   i    Ingresso              Passaggio aperto senza porta
   o    Uscita                Passaggio di uscita
   :    Decal                 Decorazione visiva, non blocca


REGOLE MAPPA — SEPARATORI ORIZZONTALI (righe muri, 5 spazi di prefisso)
────────────────────────────────────────────────────────────────────────
  Char  Muro                  Note
  ────  ──────────────────    ────────────────────
  (sp)  Nessun muro
   ─    Occludente            Muro solido standard
   ╌    Fragile               Taurus lo sfonda con la carica
   ═    Porta                 Aperta dalla chiave
   i    Ingresso              Passaggio aperto senza porta
   o    Uscita                Passaggio di uscita
   .    Decal                 Decorazione visiva, non blocca

  Ownership: separatore tra riga R e R+1 → BOTTOM di R (o TOP di R+1 se R è NonCella).
             separatore tra col C e C+1  → RIGHT di C  (o LEFT di C+1  se C è NonCella).

  ATTENZIONE — fill (▓) è una cella esistente, NON è NonCella:
    Se vuoi TOP=1 su un pavimento in row 9, la cella in row 8 nella stessa colonna
    deve essere spazio (NonCella), non ▓. Con ▓ sopra, il separatore ─ finisce come
    BOTTOM del fill, non TOP del pavimento.
    Soluzione alternativa: usa MURI per forzare il muro sulla cella giusta.

  Posizionamento fisico nel wall row:
    La riga muro ha 5 spazi di prefisso. Il char alla posizione (5 + c*2) corrisponde
    alla colonna c. Esempio: ─ sulla colonna 10 → posizione 25 del raw line.
    Le posizioni dispari del contenuto (5+1, 5+3, ...) sono ignorate dal convertitore.


ANNOTAZIONI — TESTI (trigger narrativi)
───────────────────────────────────────
  TESTI:
    r,c: <stringCode>

  Il testo viene mostrato quando Mark entra nella cella. La cella deve essere
  marcata ! nella mappa per distinguerla visivamente (ma · funziona uguale).
  I codici stringCode sono ID numerici lunghi (es. 637284382983500501).
  Usa codici nuovi e unici per ogni testo; non riusare codici già esistenti.
  Max 2–3 testi per livello. Posizionali PRIMA dell'oggetto/pericolo a cui si
  riferiscono, non sopra.


ANNOTAZIONI — CANDELE (quantità pacco)
───────────────────────────────────────
  CANDELE:
    r,c: 3

  Specifica quante candele contiene un pacco (default=1 se assente).
  Presente solo quando la cella ha un valore objData esplicito nel .mark originale.


ANNOTAZIONI — CANCELLA (azzeramento oggetto cella)
────────────────────────────────────────────────────
  CANCELLA:
    r,c: 0

  Marca una cella con object=0 (cancella l'oggetto di default del tile).
  Generato automaticamente dal visualizzatore; non serve scriverlo manualmente
  per livelli nuovi. Presente solo quando il .mark originale ha object=0.


ANNOTAZIONI — WAYPOINTS (percorsi di pattuglia nemici)
───────────────────────────────────────────────────────
  WAYPOINTS:
    r,c: Z-1-1
    r,c: Z-1-2

  Formato: PREFIX-GRUPPO-INDICE
    PREFIX   = lettera del nemico (Z=Zombie, S=Scheletro, G=Goblin, T=Taurus,
                                   H=Ghost, B=Bubu)
    GRUPPO   = numero del gruppo pattuglia (es. 1, 2, 3)
    INDICE   = posizione nel percorso (parte da 1, senza buchi)

  Il nemico percorre in loop: punto 1 → punto 2 → … → punto N → punto 1.
  Ogni punto del gruppo deve stare su una cella calpestabile.
  Regola: ogni gruppo deve avere almeno INDICE 1; gli indici devono essere
  consecutivi senza buchi (1,2,3 — non 1,3).

  Esempio — due zombie che pattugliano stanze diverse:
    7,10: Z-1-1   ← Zombie gruppo 1, punto 1
    7,15: Z-1-2   ← Zombie gruppo 1, punto 2
    12,5: Z-2-1   ← Zombie gruppo 2, punto 1
    12,9: Z-2-2   ← Zombie gruppo 2, punto 2


ANNOTAZIONI — TERRA (varianti pavimento/riempimento)
─────────────────────────────────────────────────────
  TERRA:
    r,c: terra2
    r,c: fill7

  Alias terra:  terra1 (default·) terra2 terra3 terra4 terra5
                terra6–terra10 solo con pack=2
  Alias fill:   fill1 (default▓)  fill2 … fill13

  Tutte le varianti terra sono calpestabili ma visivamente diverse.
  Le varianti fill sono non-calpestabili e si comportano come muri decorativi.
  Usa --packs per sapere quante varianti sono disponibili nel tuo pack.


ANNOTAZIONI — MURI (override ID muri non-standard)
────────────────────────────────────────────────────
  MURI:
    r,c,BOTTOM: 4      ← override ID muro sul lato BOTTOM della cella (r,c)
    r,c,RIGHT: 13      ← override ID muro sul lato RIGHT della cella (r,c)

  Lati validi: TOP, RIGHT, BOTTOM, LEFT
  IDs non deducibili dal carattere (varianti visive):
    4,12,13,14,15 → mostrati come │/─ (occludente), ma texture diversa
    7,8,9,10,11   → mostrati come :/. (decal),      ma texture diversa
  In genere non serve scrivere MURI manualmente: viene generato dal visualizzatore.
  Casi generati automaticamente:
    · ID non default (4,12–15 su │/─; 8–11 su :/.)
    · Muri sul lato secondario (es. TOP quando il primario BOTTOM già esiste)
    · Muri di bordo: TOP di riga 0, BOTTOM di riga 24, LEFT di col 0, RIGHT di col 44
      (non hanno separatore nell'ASCII e vengono sempre messi in MURI)
  Quando usare MURI manualmente nei livelli nuovi:
    · Quando il separatore finirebbe sulla cella sbagliata per via dell'ownership
      (es. fill adiacente al pavimento → il separatore va sul fill, non sul pavimento)
    · Per Ingresso/Uscita su lato LEFT/RIGHT in corridoi orizzontali


REGOLE DI VALIDAZIONE (npx tsx scripts/validate-level.ts)
──────────────────────────────────────────────────────────
  OBBLIGATORIO (errori bloccanti):
    ✗  Esattamente 1 Giocatore (@) su cella calpestabile
    ✗  Almeno 1 Uscita (E)
    ✗  pack 1–8, dirty 0–100

  FORTEMENTE CONSIGLIATO (avvisi):
    ⚠  Almeno 1 Chiave (K) — senza chiave l'uscita non si apre
    ⚠  Almeno 1 Candele (c) — senza luce il gioco è ingiocabile
    ⚠  Nemici a distanza Manhattan ≥ 3 dallo spawn del giocatore
    ⚠  Bordi mappa con Riempimento o muri Occludente (evita uscite dalla mappa)
    ⚠  Waypoint: indici consecutivi, nessun buco, nessun duplicato


LINEE GUIDA DI DESIGN
──────────────────────
  La mappa è 25 righe × 45 colonne. Usa tutto lo spazio: stanze collegate
  da corridoi, non una sola stanza grande.

  Pack e dirty:
    pack=1–8 → tema visivo completo (pavimenti, muri, porte, decal)
    dirty=0  → pavimento pulito   dirty=10 → qualche macchia   dirty=30 → molto sporco

  Illuminazione:
    Piazza Brazier (*) e Candele (c) per guidare il giocatore verso luoghi chiave.
    Usa Buio (░) per zone di tensione dove il giocatore è vulnerabile.

  Nemici — posizionamento:
    Zombie (Z): in corridoi stretti, spazio limitato di manovra.
    Scheletro (S): pattuglia stanze, aggressivo, insegue.
    Goblin (G): in gruppo, trigger (~) su passaggi obbligati.
    Taurus (T): in corridoi larghi dove può caricare; muri Fragile davanti.
    Ghost (H): zona aperta; fermarsi lo fa passare oltre.
    Bubu (B): vicino alle candele che vuole spegnere.

  Testi narrativi:
    Voce di Mark — prima persona, tono adolescente lucido e sempre più cinico.
    Max 2–3 testi per livello. Posizionali appena prima del pericolo a cui si
    riferiscono. Non spiegare le meccaniche direttamente — suggerire in voce.


ESEMPIO LIVELLO MINIMO VALIDO
──────────────────────────────
  pack=1 dirty=0

       0         1         2
       012345678901234567890

    0: ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
       ─────────────────────
    1: ▓│@ · · · c · · K│E│▓
       ─────────────────────
    2: ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓

  (una stanza con bordi in Riempimento, giocatore, candela, chiave, uscita)

════════════════════════════════════════════════════════════════════════════════
`.trim();
}

// ---------------------------------------------------------------------------
// Griglia vuota (--new)
// ---------------------------------------------------------------------------

function renderEmptyLevel(packId: number, dirty: number): string {
    const out: string[] = [];
    out.push(`pack=${packId} dirty=${dirty}`);
    out.push('');
    const [ruler1, ruler2] = buildColumnRuler();
    out.push(ruler1!);
    out.push(ruler2!);
    out.push('');
    // 25 righe di NonCella (tutto spazio)
    for (let r = 0; r < ROWS; r++) {
        out.push(`${String(r).padStart(3)}: ${' '.repeat(COLS * 2 - 1)}`);
    }
    return out.join('\n');
}

// ---------------------------------------------------------------------------
// Info texture pack (--packs)
// ---------------------------------------------------------------------------

function renderPacks(): string {
    // Raccoglie tutti i frame name da tutti gli spritesheet JSON
    const allFrames = new Set<string>();
    try {
        const files = readdirSync(SPRITESHEET_DIR).filter(f => f.endsWith('.json'));
        for (const f of files) {
            const data = JSON.parse(readFileSync(resolve(SPRITESHEET_DIR, f), 'utf-8')) as { frames: Record<string, unknown> };
            Object.keys(data.frames).forEach(k => allFrames.add(k));
        }
    } catch (err) {
        return `Impossibile leggere gli spritesheet da: ${SPRITESHEET_DIR}\n${(err as Error).message}`;
    }

    const out: string[] = [
        'TEXTURE PACK DISPONIBILI',
        '(alias usabili nella sezione TERRA del file ASCII)',
        '',
    ];

    for (let p = 1; p <= 8; p++) {
        // Conta cell textures (Grounds/paccoN/cellM — senza _normal/_spec)
        const cellCount = Array.from(allFrames)
            .filter(f => /^Grounds\/pacco\d+\/cell\d+$/.test(f) && f.startsWith(`Grounds/pacco${p}/`))
            .length;
        // Conta fill textures (Grounds/paccoN/Riempimento/riempimento_M — senza _normal)
        const fillCount = Array.from(allFrames)
            .filter(f => /^Grounds\/pacco\d+\/Riempimento\/riempimento_\d+$/.test(f) && f.startsWith(`Grounds/pacco${p}/`))
            .length;

        if (cellCount === 0 && fillCount === 0) continue;

        const terraAliases = cellCount > 0
            ? `terra1–terra${cellCount}`
            : '(nessuna)';
        const fillAliases = fillCount > 0
            ? `fill1–fill${fillCount}`
            : '(nessuna)';

        out.push(`  pack=${p}   pavimento: ${terraAliases.padEnd(20)}   riempimento: ${fillAliases}`);
    }

    out.push('');
    out.push('Nota: terra1 e fill1 sono i default (·  e  ▓) — non vanno scritti in TERRA.');
    out.push('      Le varianti terra6–terra10 esistono solo in pack=2.');

    return out.join('\n');
}

// ---------------------------------------------------------------------------
// Entrypoint
// ---------------------------------------------------------------------------

const args = process.argv.slice(2);
const showLegend = args.includes('--legend');
const showNew    = args.includes('--new');
const showPacks  = args.includes('--packs');
const showHelp   = args.includes('--help') || args.includes('-h');

// --help: guida completa per il level design
if (showHelp) {
    console.log(buildHelp());
    process.exit(0);
}

// --packs: mostra info texture pack e termina
if (showPacks) {
    console.log(renderPacks());
    process.exit(0);
}

// --new: genera griglia vuota
if (showNew) {
    const packArg = args.find(a => a.startsWith('--pack='));
    const dirtyArg = args.find(a => a.startsWith('--dirty='));
    const newPackId = packArg ? parseInt(packArg.split('=')[1]!, 10) : 1;
    const newDirty  = dirtyArg ? parseInt(dirtyArg.split('=')[1]!, 10) : 0;
    console.log(renderEmptyLevel(newPackId, newDirty));
    process.exit(0);
}

const positional = args.filter(a => !a.startsWith('--'));

if (positional.length === 0) {
    console.error('Uso:');
    console.error('  npx tsx scripts/level-editor.ts <livello.mark> [--legend]');
    console.error('  npx tsx scripts/level-editor.ts --new [--pack=N] [--dirty=N]');
    console.error('  npx tsx scripts/level-editor.ts --packs');
    console.error('  npx tsx scripts/level-editor.ts --help');
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
