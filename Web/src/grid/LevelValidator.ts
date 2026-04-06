/**
 * LevelValidator.ts — Validatore livelli .mark per browser.
 *
 * Stessa logica di scripts/validate-level.ts ma senza dipendenze Node.js.
 * Esporta validateLevel(text) usabile nel browser (Level Editor).
 */

// ---------------------------------------------------------------------------
// Costanti
// ---------------------------------------------------------------------------

const VALID_GROUND_TYPES = new Set([
    0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
    17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
]);

const WALKABLE_GROUND_TYPES = new Set([1, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14, 15, 16]);
const RIEMPIMENTO_GROUND_TYPES = new Set([8, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28]);

const GROUND_TYPE_NAMES: Record<number, string> = {
    0: 'NonCella', 1: 'Terra1', 3: 'TrappolaMorteIstantanea', 4: 'TrappolaTemporizzata',
    5: 'TriggerGoblin', 6: 'Terra2', 7: 'Buio', 8: 'Riempimento1', 9: 'Terra3',
    10: 'Terra4', 11: 'Terra5', 12: 'Terra6', 13: 'Terra7', 14: 'Terra8',
    15: 'Terra9', 16: 'Terra10', 17: 'Riempimento2', 18: 'Riempimento3',
    19: 'Riempimento4', 20: 'Riempimento5', 21: 'Riempimento6', 22: 'Riempimento7',
    23: 'Riempimento8', 24: 'Riempimento9', 25: 'Riempimento10', 26: 'Riempimento11',
    27: 'Riempimento12', 28: 'Riempimento13',
};

const VALID_OBJECT_TYPES = new Set([0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 12, 13, 14, 15]);

const OBJECT_TYPE_NAMES: Record<number, string> = {
    0: 'Cancella', 1: 'Giocatore', 2: 'Chiave', 3: 'Uscita', 4: 'Candele',
    5: 'Zombie', 6: 'Scheletro', 7: 'Goblin', 8: 'Taurus', 10: 'Ghost',
    11: 'Bubu', 12: 'Lucina', 13: 'OggettoNonPassabile1', 14: 'OggettoNonPassabile2',
    15: 'OggettoPassabile1',
};

const ENEMY_TYPES = new Set([5, 6, 7, 8, 10, 11]);
const ENEMY_TYPES_WITH_WAYPOINT = new Set([5, 6, 7, 8, 10, 11]);

const VALID_WALL_TYPES = new Set([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18]);

const WALL_TYPE_NAMES: Record<number, string> = {
    0: 'Niente', 1: 'Occludente', 2: 'Fragile', 3: 'Porta', 4: 'Muro1',
    5: 'Ingresso', 6: 'Uscita', 7: 'Decal1', 8: 'Decal2', 9: 'Decal3',
    10: 'Decal4', 11: 'Decal5', 12: 'Muro2', 13: 'Muro3', 14: 'Muro4',
    15: 'Muro5', 16: 'Decal6', 17: 'Decal7', 18: 'Decal8',
};

const EXPECTED_ROWS = 25;
const EXPECTED_COLS = 45;

// ---------------------------------------------------------------------------
// Strutture dati interne
// ---------------------------------------------------------------------------

interface CellEntry {
    lineNumber: number;
    row: number;
    col: number;
    groundType: number;
    objectType: number | null;
    objectAdditionalData: string;
    wallTop: number;
    wallRight: number;
    wallBottom: number;
    wallLeft: number;
}

export interface ValidationResult {
    errors: string[];
    warnings: string[];
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function isInt(s: string): boolean {
    return /^-?\d+$/.test(s.trim());
}

function manhattanDistance(r1: number, c1: number, r2: number, c2: number): number {
    return Math.abs(r1 - r2) + Math.abs(c1 - c2);
}

// ---------------------------------------------------------------------------
// Validazione principale
// ---------------------------------------------------------------------------

export function validateLevel(text: string): ValidationResult {
    const errors: string[] = [];
    const warnings: string[] = [];

    const addError = (msg: string) => errors.push(`ERRORE: ${msg}`);
    const addWarning = (msg: string) => warnings.push(`AVVISO: ${msg}`);

    const rawLines = text.replace(/\r/g, '').split('\n');
    const lines = rawLines.map((l, i) => ({ content: l.trim(), lineNumber: i + 1 }));
    const nonEmpty = lines.filter(l => l.content.length > 0);

    // Header
    if (nonEmpty.length === 0) {
        addError('Il file è vuoto.');
        return { errors, warnings };
    }

    const headerLine = nonEmpty[0]!;
    if (headerLine.content !== 'GameGridSerializerV4') {
        addError(
            `Riga ${headerLine.lineNumber}: header non valido. ` +
            `Trovato: "${headerLine.content}", atteso: "GameGridSerializerV4".`
        );
        return { errors, warnings };
    }

    // Dimensioni — salta la riga del nome se presente (non contiene '|')
    if (nonEmpty.length < 2) {
        addError('Mancano dimensioni e celle.');
        return { errors, warnings };
    }

    let nextIdx = 1;
    if (!nonEmpty[1]!.content.includes('|')) {
        nextIdx = 2; // nome presente, dimensioni sono alla riga successiva
    }

    if (nonEmpty.length <= nextIdx) {
        addError('Il file termina dopo il nome: mancano dimensioni e celle.');
        return { errors, warnings };
    }

    const dimLine = nonEmpty[nextIdx]!;
    nextIdx++;
    const dimParts = dimLine.content.split('|');
    if (dimParts.length !== 2 || !isInt(dimParts[0]!) || !isInt(dimParts[1]!)) {
        addError(`Riga ${dimLine.lineNumber}: formato dimensioni non valido. Atteso "RIGHE|COLONNE".`);
        return { errors, warnings };
    }

    const rows = parseInt(dimParts[0]!, 10);
    const cols = parseInt(dimParts[1]!, 10);

    if (rows !== EXPECTED_ROWS || cols !== EXPECTED_COLS) {
        addError(
            `Riga ${dimLine.lineNumber}: dimensioni errate. ` +
            `Atteso ${EXPECTED_ROWS}|${EXPECTED_COLS}, trovato ${rows}|${cols}.`
        );
    }

    // Configurazione ambiente
    if (nonEmpty.length <= nextIdx) {
        addError('Mancano configurazione ambiente e celle.');
        return { errors, warnings };
    }

    const envLine = nonEmpty[nextIdx]!;
    nextIdx++;
    const envParts = envLine.content.split('|');
    if (envParts.length !== 2 || !isInt(envParts[0]!) || !isInt(envParts[1]!)) {
        addError(`Riga ${envLine.lineNumber}: formato configurazione ambiente non valido.`);
    } else {
        const texturePack = parseInt(envParts[0]!, 10);
        const dirtyPct = parseInt(envParts[1]!, 10);
        if (texturePack < 1 || texturePack > 8) {
            addError(`Riga ${envLine.lineNumber}: ENV_TEXTURE_PACK_ID="${texturePack}" deve essere tra 1 e 8.`);
        }
        if (dirtyPct < 0 || dirtyPct > 100) {
            addError(`Riga ${envLine.lineNumber}: DIRTY_PROBABILITY_PCT="${dirtyPct}" deve essere tra 0 e 100.`);
        }
    }

    // Celle
    const cellLines = nonEmpty.slice(nextIdx);
    const parsedCells: CellEntry[] = [];
    const seenPositions = new Map<string, number>();

    for (const { content, lineNumber } of cellLines) {
        const parts = content.split('|');

        if (parts.length < 9) {
            addError(
                `Riga ${lineNumber}: cella con ${parts.length} campi (minimo 9). Contenuto: "${content}".`
            );
            continue;
        }

        const rowStr = parts[0]!.trim();
        const colStr = parts[1]!.trim();

        if (!isInt(rowStr) || !isInt(colStr)) {
            addError(`Riga ${lineNumber}: ROW o COL non interi (ROW="${rowStr}", COL="${colStr}").`);
            continue;
        }

        const row = parseInt(rowStr, 10);
        const col = parseInt(colStr, 10);

        if (row < 0 || row >= EXPECTED_ROWS) {
            addError(`Riga ${lineNumber}: ROW=${row} fuori dai limiti (0–${EXPECTED_ROWS - 1}).`);
            continue;
        }
        if (col < 0 || col >= EXPECTED_COLS) {
            addError(`Riga ${lineNumber}: COL=${col} fuori dai limiti (0–${EXPECTED_COLS - 1}).`);
            continue;
        }

        const posKey = `${row},${col}`;
        if (seenPositions.has(posKey)) {
            addError(
                `Riga ${lineNumber}: posizione (ROW=${row}, COL=${col}) già definita ` +
                `alla riga ${seenPositions.get(posKey)!}.`
            );
            continue;
        }
        seenPositions.set(posKey, lineNumber);

        const groundStr = parts[2]!.trim();
        if (!isInt(groundStr)) {
            addError(`Riga ${lineNumber}: GROUND="${groundStr}" non intero (cella ${row},${col}).`);
            continue;
        }
        const groundType = parseInt(groundStr, 10);
        if (!VALID_GROUND_TYPES.has(groundType)) {
            addError(
                `Riga ${lineNumber}: GROUND=${groundType} non riconosciuto ` +
                `(cella ${row},${col}).`
            );
            continue;
        }

        const objectStr = parts[3]!.trim();
        let objectType: number | null = null;
        if (objectStr !== '') {
            if (!isInt(objectStr)) {
                addError(`Riga ${lineNumber}: OBJECT="${objectStr}" non intero (cella ${row},${col}).`);
                continue;
            }
            objectType = parseInt(objectStr, 10);
            if (!VALID_OBJECT_TYPES.has(objectType)) {
                addError(
                    `Riga ${lineNumber}: OBJECT=${objectType} non riconosciuto (cella ${row},${col}).`
                );
                continue;
            }
        }

        const objectAdditionalData = parts[4]!.trim();

        const wallNames = ['TOP', 'RIGHT', 'BOTTOM', 'LEFT'];
        const wallValues: number[] = [];
        let wallError = false;
        for (let w = 0; w < 4; w++) {
            const wallStr = parts[5 + w]!.trim();
            if (!isInt(wallStr)) {
                addError(`Riga ${lineNumber}: muro ${wallNames[w]}="${wallStr}" non intero (cella ${row},${col}).`);
                wallError = true;
                break;
            }
            const wallVal = parseInt(wallStr, 10);
            if (!VALID_WALL_TYPES.has(wallVal)) {
                addError(
                    `Riga ${lineNumber}: muro ${wallNames[w]}=${wallVal} ` +
                    `("${WALL_TYPE_NAMES[wallVal] ?? 'sconosciuto'}") non riconosciuto (cella ${row},${col}).`
                );
                wallError = true;
                break;
            }
            wallValues.push(wallVal);
        }
        if (wallError) continue;

        parsedCells.push({
            lineNumber, row, col, groundType, objectType, objectAdditionalData,
            wallTop: wallValues[0]!, wallRight: wallValues[1]!,
            wallBottom: wallValues[2]!, wallLeft: wallValues[3]!,
        });
    }

    // --- Validazioni semantiche ---

    const players = parsedCells.filter(c => c.objectType === 1);
    const exits   = parsedCells.filter(c => c.objectType === 3);
    const keys    = parsedCells.filter(c => c.objectType === 2);
    const candles = parsedCells.filter(c => c.objectType === 4);
    const enemies = parsedCells.filter(c => c.objectType !== null && ENEMY_TYPES.has(c.objectType));

    // Giocatore
    if (players.length === 0) {
        addError('Manca lo spawn del giocatore (OBJECT=1). Il gioco non può partire.');
    } else if (players.length > 1) {
        const positions = players.map(p => `(${p.row},${p.col})`).join(', ');
        addError(`${players.length} spawn del giocatore — deve essercene esattamente 1. Trovati: ${positions}.`);
    }

    for (const player of players) {
        if (!WALKABLE_GROUND_TYPES.has(player.groundType)) {
            const groundName = GROUND_TYPE_NAMES[player.groundType] ?? String(player.groundType);
            addError(`Il Giocatore (riga ${player.lineNumber}) è su terreno "${groundName}" non percorribile.`);
        }
    }

    // Uscita
    if (exits.length === 0) {
        addError('Manca la porta di uscita (OBJECT=3). Il livello non può essere completato.');
    }

    // Chiave
    if (keys.length === 0) {
        addWarning('Nessuna chiave (OBJECT=2). Senza chiave l\'uscita non si apre mai.');
    }

    // Candele
    if (candles.length === 0) {
        addWarning('Nessun pacco di candele (OBJECT=4). Il gioco è praticamente ingiocabile senza luce.');
    }

    // Dati candele
    for (const cell of candles) {
        const data = cell.objectAdditionalData;
        if (data !== '' && (!isInt(data) || parseInt(data, 10) <= 0)) {
            addError(
                `Riga ${cell.lineNumber}: il pacco candele (${cell.row},${cell.col}) ` +
                `ha OBJ_DATA="${data}" non valido. Deve essere un intero positivo o vuoto.`
            );
        }
    }

    // Distanza giocatore-nemici
    if (players.length === 1) {
        const player = players[0]!;
        for (const enemy of enemies) {
            const dist = manhattanDistance(player.row, player.col, enemy.row, enemy.col);
            const enemyName = OBJECT_TYPE_NAMES[enemy.objectType!] ?? String(enemy.objectType);
            if (dist < 3) {
                addWarning(
                    `${enemyName} (${enemy.row},${enemy.col}) a distanza ${dist} dallo spawn ` +
                    `(${player.row},${player.col}). Raccomandati almeno 3–4 celle.`
                );
            }
        }
    }

    // Formato waypoint nemici
    const waypointPattern = /^[A-Za-z]+-\d+-\d+$/;
    const waypointGroups = new Map<string, number[]>();

    for (const cell of parsedCells) {
        if (cell.objectType === null || !ENEMY_TYPES_WITH_WAYPOINT.has(cell.objectType)) continue;
        const data = cell.objectAdditionalData;
        if (data === '') continue;

        if (!waypointPattern.test(data)) {
            const enemyName = OBJECT_TYPE_NAMES[cell.objectType] ?? String(cell.objectType);
            addError(
                `Riga ${cell.lineNumber}: ${enemyName} (${cell.row},${cell.col}) ` +
                `ha OBJ_DATA="${data}" formato waypoint non valido. ` +
                `Atteso: "PREFIX-GRUPPO-INDICE" (es. "Z-1-2").`
            );
            continue;
        }

        const parts = data.split('-');
        const prefix = parts[0]!;
        const group = parts[1]!;
        const idx = parseInt(parts[2]!, 10);
        const groupKey = `${prefix}-${group}`;

        if (!waypointGroups.has(groupKey)) waypointGroups.set(groupKey, []);
        waypointGroups.get(groupKey)!.push(idx);
    }

    for (const [groupKey, indices] of waypointGroups) {
        if (!indices.includes(1)) {
            addError(
                `Il gruppo waypoint "${groupKey}" non contiene l'indice 1. ` +
                `Trovati: [${indices.sort((a, b) => a - b).join(', ')}].`
            );
        }
        const uniqueIndices = new Set(indices);
        if (uniqueIndices.size !== indices.length) {
            const dups = indices.filter((v, i, arr) => arr.indexOf(v) !== i);
            addWarning(`Il gruppo waypoint "${groupKey}" ha indici duplicati: [${dups.join(', ')}].`);
        }
    }

    // Nemici su terreno percorribile
    for (const enemy of enemies) {
        const enemyName = OBJECT_TYPE_NAMES[enemy.objectType!] ?? String(enemy.objectType);
        if (!WALKABLE_GROUND_TYPES.has(enemy.groundType) && !RIEMPIMENTO_GROUND_TYPES.has(enemy.groundType)) {
            addError(
                `Riga ${enemy.lineNumber}: ${enemyName} (${enemy.row},${enemy.col}) ` +
                `è su NonCella. I nemici devono essere su terreno percorribile.`
            );
        } else if (RIEMPIMENTO_GROUND_TYPES.has(enemy.groundType)) {
            addWarning(
                `Riga ${enemy.lineNumber}: ${enemyName} (${enemy.row},${enemy.col}) ` +
                `è su Riempimento (GROUND=${enemy.groundType}). Sposta su un pavimento Terra.`
            );
        }
    }

    // Doppio muro: se cella A ha muro su un lato, la cella adiacente non deve averlo sul lato opposto
    const cellMap = new Map<string, CellEntry>();
    for (const c of parsedCells) cellMap.set(`${c.col},${c.row}`, c);

    const reportedPairs = new Set<string>();
    for (const cell of parsedCells) {
        const checks: Array<{ myWall: number; adjCol: number; adjRow: number; adjWallKey: 'wallTop'|'wallRight'|'wallBottom'|'wallLeft'; sideLabel: string }> = [
            { myWall: cell.wallTop,    adjCol: cell.col,     adjRow: cell.row - 1, adjWallKey: 'wallBottom', sideLabel: 'Top' },
            { myWall: cell.wallBottom, adjCol: cell.col,     adjRow: cell.row + 1, adjWallKey: 'wallTop',    sideLabel: 'Bottom' },
            { myWall: cell.wallLeft,   adjCol: cell.col - 1, adjRow: cell.row,     adjWallKey: 'wallRight',  sideLabel: 'Left' },
            { myWall: cell.wallRight,  adjCol: cell.col + 1, adjRow: cell.row,     adjWallKey: 'wallLeft',   sideLabel: 'Right' },
        ];
        for (const { myWall, adjCol, adjRow, adjWallKey, sideLabel } of checks) {
            if (myWall === 0) continue;
            const adj = cellMap.get(`${adjCol},${adjRow}`);
            if (!adj) continue;
            if (adj[adjWallKey] === 0) continue;
            const pairKey = [
                `${cell.col},${cell.row},${sideLabel}`,
                `${adjCol},${adjRow}`,
            ].sort().join('|');
            if (reportedPairs.has(pairKey)) continue;
            reportedPairs.add(pairKey);
            addWarning(
                `Muro doppio tra (${cell.row},${cell.col}) lato ${sideLabel} e ` +
                `(${adjRow},${adjCol}): stesso confine fisico definito due volte.`
            );
        }
    }

    // Bordi mappa
    const borderCells = parsedCells.filter(c =>
        c.row === 0 || c.row === EXPECTED_ROWS - 1 ||
        c.col === 0 || c.col === EXPECTED_COLS - 1
    );

    for (const cell of borderCells) {
        const isFill = RIEMPIMENTO_GROUND_TYPES.has(cell.groundType);
        const hasWall = cell.wallTop !== 0 || cell.wallRight !== 0 ||
                        cell.wallBottom !== 0 || cell.wallLeft !== 0;
        if (!isFill && !hasWall) {
            addWarning(
                `Cella di bordo (${cell.row},${cell.col}) percorribile senza muri. ` +
                `I bordi dovrebbero essere Riempimento o avere muri Occludente.`
            );
        }
    }

    return { errors, warnings };
}
