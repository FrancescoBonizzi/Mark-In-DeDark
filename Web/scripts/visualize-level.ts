/**
 * Visualizzatore livelli .mark — Mark In DeDark
 *
 * Uso:
 *   npx tsx scripts/visualize-level.ts <percorso/al/livello.mark>
 *
 * Produce una mappa ASCII 2D del livello con:
 *   - Contenuto di ogni cella (pavimento, oggetti, nemici)
 *   - Muri strutturali come bordi │─ tra le celle
 *   - Statistiche finali
 */

import { readFileSync } from 'fs';
import { resolve, basename } from 'path';

// ---------------------------------------------------------------------------
// Costanti
// ---------------------------------------------------------------------------

const ROWS = 25;
const COLS = 45;

// Muri che bloccano il passaggio fisicamente (non solo decorativi)
const STRUCTURAL_WALL_IDS = new Set([1, 2, 3, 4, 5, 6, 12, 13, 14, 15]);

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

    if (lines.length < 3) {
        return { cells, packId: 1, dirty: 0 };
    }

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

        cells[row]![col] = {
            ground, object, objData,
            wallTop, wallRight, wallBottom, wallLeft,
            stringCode,
        };
    }

    return { cells, packId, dirty };
}

// ---------------------------------------------------------------------------
// Caratteri cella e separatori
// ---------------------------------------------------------------------------

function cellChar(cell: Cell | null): string {
    if (!cell || cell.ground === 0) return ' ';

    // Oggetto (object=0 è "cancella", va ignorato)
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

    // Terreno
    switch (cell.ground) {
        case 7:  return '░'; // Buio
        case 3:  return 'x'; // TrappolaMorteIstantanea
        case 4:  return '^'; // TrappolaTemporizzata
        case 5:  return '~'; // TriggerGoblin
        // Riempimento (sfondo non calpestabile)
        case 8: case 17: case 18: case 19: case 20: case 21: case 22:
        case 23: case 24: case 25: case 26: case 27: case 28:
            return '▓';
        default:
            // Terra (calpestabile) — evidenzia celle con testo narrativo
            return cell.stringCode ? '!' : '·';
    }
}

function isStructural(wallId: number): boolean {
    return STRUCTURAL_WALL_IDS.has(wallId);
}

/** Carattere separatore verticale tra colonna c e c+1 nella stessa riga. */
function rightSep(left: Cell | null, right: Cell | null): string {
    const l = left?.wallRight ?? 0;
    const r = right?.wallLeft ?? 0;
    if (isStructural(l) || isStructural(r)) return '│';
    // Decal visivo ma non bloccante
    if (l !== 0 || r !== 0) return '╎';
    return ' ';
}

/** Riga separatrice orizzontale tra row r e r+1.
 *  Restituisce una stringa di larghezza (COLS + COLS-1) = 89 char. */
function buildWallRow(upper: (Cell | null)[], lower: (Cell | null)[]): string {
    let line = '';
    for (let c = 0; c < COLS; c++) {
        const u = upper[c];
        const l = lower[c];
        const ub = u?.wallBottom ?? 0;
        const lt = l?.wallTop ?? 0;
        if (isStructural(ub) || isStructural(lt)) {
            line += '─';
        } else if (ub !== 0 || lt !== 0) {
            line += '╌'; // decal visivo
        } else {
            line += ' ';
        }

        if (c < COLS - 1) {
            // Angolo di intersezione: c'è almeno un muro adiacente?
            const uR = upper[c + 1];
            const lR = lower[c + 1];
            const hHere  = isStructural(ub) || isStructural(lt);
            const hRight = isStructural(uR?.wallBottom ?? 0) || isStructural(lR?.wallTop ?? 0);
            const vAbove = isStructural(u?.wallRight ?? 0) || isStructural(uR?.wallLeft ?? 0);
            const vBelow = isStructural(l?.wallRight ?? 0) || isStructural(lR?.wallLeft ?? 0);
            line += (hHere || hRight || vAbove || vBelow) ? '+' : ' ';
        }
    }
    return line;
}

// ---------------------------------------------------------------------------
// Rendering
// ---------------------------------------------------------------------------

/** Intestazione colonne allineata al grid (1 char/cell + 1 char separatore). */
function buildColumnRuler(): string[] {
    // Posizione char di colonna c nel contenuto riga = c * 2
    // (prefix "NNN: " escluso)
    const line1 = Array<string>(COLS * 2 - 1).fill(' ');
    const line2 = Array<string>(COLS * 2 - 1).fill(' ');
    for (let c = 0; c < COLS; c++) {
        const pos = c * 2;
        const tens = Math.floor(c / 10);
        const ones = c % 10;
        line1[pos] = tens > 0 ? tens.toString() : (c === 0 ? '0' : ' ');
        line2[pos] = ones.toString();
    }
    const prefix = '     '; // 5 spazi = allineamento con "NNN: "
    return [prefix + line1.join(''), prefix + line2.join('')];
}

function renderLevel(cells: (Cell | null)[][], packId: number, dirty: number, filename: string): string {
    const out: string[] = [];

    // Header
    out.push(`File : ${filename}`);
    out.push(`Pack : pacco${packId}   Dirty: ${dirty}%`);
    out.push(`Griglia: ${ROWS} righe × ${COLS} colonne`);
    out.push('');

    // Legenda
    out.push('Legenda celle:');
    out.push('  @=Giocatore  K=Chiave  E=Uscita  c=Candele  !=TriggerTesto');
    out.push('  Z=Zombie  S=Scheletro  G=Goblin  T=Taurus  H=Ghost  B=Bubu');
    out.push('  *=Brazier  #==Ostacolo  ·=Pavimento  ░=Buio  ▓=Riempimento');
    out.push('  x=TrappolaMorte  ^=TrappolaTimer  ~=TriggerGoblin  spazio=null');
    out.push('Legenda muri:  │─ strutturali   ╎╌ decorativi   + intersezione');
    out.push('');

    // Righello colonne
    const [ruler1, ruler2] = buildColumnRuler();
    out.push(ruler1!);
    out.push(ruler2!);
    out.push('');

    // Grid
    for (let r = 0; r < ROWS; r++) {
        // Riga contenuto
        let line = `${String(r).padStart(3)}: `;
        for (let c = 0; c < COLS; c++) {
            line += cellChar(cells[r]![c]!);
            if (c < COLS - 1) {
                line += rightSep(cells[r]![c]!, cells[r]![c + 1]!);
            }
        }
        out.push(line);

        // Riga muri inferiori (tra r e r+1)
        if (r < ROWS - 1) {
            out.push('     ' + buildWallRow(cells[r]!, cells[r + 1]!));
        }
    }

    // Statistiche
    out.push('');
    out.push('─'.repeat(60));
    out.push('Statistiche:');

    let playerCount = 0, keyCount = 0, exitCount = 0, candlePacks = 0, candleTotal = 0;
    const enemyCounts: Record<string, number> = {};
    let brazierCount = 0, textTriggers = 0;

    for (let r = 0; r < ROWS; r++) {
        for (let c = 0; c < COLS; c++) {
            const cell = cells[r]![c];
            if (!cell) continue;
            if (cell.stringCode) textTriggers++;
            if (cell.object === null || cell.object === 0) continue;
            switch (cell.object) {
                case 1: playerCount++; break;
                case 2: keyCount++; break;
                case 3: exitCount++; break;
                case 4: {
                    candlePacks++;
                    const n = cell.objData ? parseInt(cell.objData, 10) : NaN;
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

    out.push(`  Giocatore: ${playerCount}   Chiave: ${keyCount}   Uscita: ${exitCount}`);
    out.push(`  Pacchi candele: ${candlePacks}   Candele totali: ${candleTotal}`);
    out.push(`  Brazier: ${brazierCount}   Trigger testo: ${textTriggers}`);

    const enemyList = Object.entries(enemyCounts);
    if (enemyList.length > 0) {
        out.push(`  Nemici: ${enemyList.map(([k, v]) => `${k}×${v}`).join('  ')}`);
    } else {
        out.push('  Nemici: nessuno');
    }

    return out.join('\n');
}

// ---------------------------------------------------------------------------
// Entrypoint
// ---------------------------------------------------------------------------

const args = process.argv.slice(2);
if (args.length === 0) {
    console.error('Uso: npx tsx scripts/visualize-level.ts <percorso/al/livello.mark>');
    process.exit(1);
}

const filePath = resolve(args[0]!);
let content: string;
try {
    content = readFileSync(filePath, 'utf-8');
} catch (err) {
    console.error(`Impossibile leggere il file: ${filePath}`);
    console.error((err as NodeJS.ErrnoException).message);
    process.exit(1);
}

const { cells, packId, dirty } = parseLevel(content);
console.log(renderLevel(cells, packId, dirty, basename(filePath)));
