/**
 * Validatore di livelli .mark — Mark In DeDark
 *
 * Uso:
 *   npx tsx scripts/validate-level.ts <percorso/al/livello.mark>
 *
 * Restituisce exit code 0 se il livello è valido, 1 se ci sono errori.
 * Gli avvertimenti (warning) non alterano l'exit code.
 */

import { readFileSync } from 'fs';
import { resolve } from 'path';

// ---------------------------------------------------------------------------
// Enum values (ricopiati dai sorgenti per mantenere lo script autonomo)
// ---------------------------------------------------------------------------

const VALID_GROUND_TYPES = new Set([
    0,  // NonCella
    1,  // Terra1
    3,  // TrappolaMorteIstantanea
    4,  // TrappolaTemporizzata
    5,  // TriggerGoblin
    6,  // Terra2
    7,  // Buio
    8,  // Riempimento1
    9,  // Terra3
    10, // Terra4
    11, // Terra5
    12, // Terra6
    13, // Terra7
    14, // Terra8
    15, // Terra9
    16, // Terra10
    17, // Riempimento2
    18, // Riempimento3
    19, // Riempimento4
    20, // Riempimento5
    21, // Riempimento6
    22, // Riempimento7
    23, // Riempimento8
    24, // Riempimento9
    25, // Riempimento10
    26, // Riempimento11
    27, // Riempimento12
    28, // Riempimento13
]);

// Terreni su cui il giocatore può stare (Terra + Buio + Trappole + TriggerGoblin)
const WALKABLE_GROUND_TYPES = new Set([1, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14, 15, 16]);

// Terreni di riempimento (non calpestabili)
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

// Nemici che supportano waypoint
const ENEMY_TYPES_WITH_WAYPOINT = new Set([5, 6, 7, 8, 10, 11]);
const ENEMY_TYPES = new Set([5, 6, 7, 8, 10, 11]);

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
// Strutture dati
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

interface ValidationResult {
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

function validateLevel(text: string): ValidationResult {
    const errors: string[] = [];
    const warnings: string[] = [];

    const addError = (msg: string) => errors.push(`  ERRORE: ${msg}`);
    const addWarning = (msg: string) => warnings.push(`  AVVISO: ${msg}`);

    const rawLines = text.replace(/\r/g, '').split('\n');

    // Rimuoviamo righe completamente vuote in fondo ma teniamo i numeri di riga originali
    const lines = rawLines.map((l, i) => ({ content: l.trim(), lineNumber: i + 1 }));
    const nonEmpty = lines.filter(l => l.content.length > 0);

    // ------------------------------------------------------------------
    // RIGA 1 — Header
    // ------------------------------------------------------------------
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

    // ------------------------------------------------------------------
    // RIGA 2 — Dimensioni
    // ------------------------------------------------------------------
    if (nonEmpty.length < 2) {
        addError('Il file termina dopo l\'header: mancano dimensioni e celle.');
        return { errors, warnings };
    }

    const dimLine = nonEmpty[1]!;
    const dimParts = dimLine.content.split('|');
    if (dimParts.length !== 2 || !isInt(dimParts[0]!) || !isInt(dimParts[1]!)) {
        addError(
            `Riga ${dimLine.lineNumber}: formato dimensioni non valido. ` +
            `Atteso "RIGHE|COLONNE", trovato: "${dimLine.content}".`
        );
        return { errors, warnings };
    }

    const rows = parseInt(dimParts[0]!, 10);
    const cols = parseInt(dimParts[1]!, 10);

    if (rows !== EXPECTED_ROWS || cols !== EXPECTED_COLS) {
        addError(
            `Riga ${dimLine.lineNumber}: dimensioni errate. ` +
            `Atteso ${EXPECTED_ROWS}|${EXPECTED_COLS}, trovato ${rows}|${cols}. ` +
            `Le dimensioni sono fisse nel serializzatore.`
        );
    }

    // ------------------------------------------------------------------
    // RIGA 3 — Configurazione ambiente
    // ------------------------------------------------------------------
    if (nonEmpty.length < 3) {
        addError('Il file termina dopo le dimensioni: mancano la configurazione ambiente e le celle.');
        return { errors, warnings };
    }

    const envLine = nonEmpty[2]!;
    const envParts = envLine.content.split('|');
    if (envParts.length !== 2 || !isInt(envParts[0]!) || !isInt(envParts[1]!)) {
        addError(
            `Riga ${envLine.lineNumber}: formato configurazione ambiente non valido. ` +
            `Atteso "ENV_TEXTURE_PACK_ID|DIRTY_PROBABILITY_PCT", trovato: "${envLine.content}".`
        );
    } else {
        const texturePack = parseInt(envParts[0]!, 10);
        const dirtyPct = parseInt(envParts[1]!, 10);

        if (texturePack < 1 || texturePack > 8) {
            addError(
                `Riga ${envLine.lineNumber}: ENV_TEXTURE_PACK_ID="${texturePack}" non valido. ` +
                `Deve essere compreso tra 1 e 8.`
            );
        }
        if (dirtyPct < 0 || dirtyPct > 100) {
            addError(
                `Riga ${envLine.lineNumber}: DIRTY_PROBABILITY_PCT="${dirtyPct}" non valido. ` +
                `Deve essere compreso tra 0 e 100.`
            );
        }
    }

    // ------------------------------------------------------------------
    // RIGHE CELLE
    // ------------------------------------------------------------------
    const cellLines = nonEmpty.slice(3);
    const parsedCells: CellEntry[] = [];
    const seenPositions = new Map<string, number>(); // "row,col" → lineNumber

    for (const { content, lineNumber } of cellLines) {
        const parts = content.split('|');

        // Ogni riga cella deve avere almeno 9 campi (ROW|COL|GROUND|OBJECT|OBJ_DATA|TOP|RIGHT|BOTTOM|LEFT)
        // Il 10° (STRING_CODE) è opzionale.
        if (parts.length < 9) {
            addError(
                `Riga ${lineNumber}: cella con numero di campi insufficiente ` +
                `(${parts.length} campi, minimo 9). Contenuto: "${content}".`
            );
            continue;
        }

        // Campi posizione
        const rowStr = parts[0]!.trim();
        const colStr = parts[1]!.trim();

        if (!isInt(rowStr) || !isInt(colStr)) {
            addError(
                `Riga ${lineNumber}: ROW o COL non sono interi validi ` +
                `(ROW="${rowStr}", COL="${colStr}").`
            );
            continue;
        }

        const row = parseInt(rowStr, 10);
        const col = parseInt(colStr, 10);

        if (row < 0 || row >= EXPECTED_ROWS) {
            addError(
                `Riga ${lineNumber}: ROW=${row} fuori dai limiti della mappa ` +
                `(valori validi: 0–${EXPECTED_ROWS - 1}).`
            );
            continue;
        }
        if (col < 0 || col >= EXPECTED_COLS) {
            addError(
                `Riga ${lineNumber}: COL=${col} fuori dai limiti della mappa ` +
                `(valori validi: 0–${EXPECTED_COLS - 1}).`
            );
            continue;
        }

        // Cella duplicata?
        const posKey = `${row},${col}`;
        if (seenPositions.has(posKey)) {
            addError(
                `Riga ${lineNumber}: posizione (ROW=${row}, COL=${col}) già definita ` +
                `alla riga ${seenPositions.get(posKey)!}. Ogni cella può comparire al massimo una volta.`
            );
            continue;
        }
        seenPositions.set(posKey, lineNumber);

        // GroundType
        const groundStr = parts[2]!.trim();
        if (!isInt(groundStr)) {
            addError(
                `Riga ${lineNumber}: GROUND="${groundStr}" non è un intero valido ` +
                `(cella ROW=${row}, COL=${col}).`
            );
            continue;
        }
        const groundType = parseInt(groundStr, 10);
        if (!VALID_GROUND_TYPES.has(groundType)) {
            addError(
                `Riga ${lineNumber}: GROUND=${groundType} non è un tipo di terreno riconosciuto ` +
                `(cella ROW=${row}, COL=${col}). ` +
                `Valori validi: ${[...VALID_GROUND_TYPES].sort((a, b) => a - b).join(', ')}.`
            );
            continue;
        }

        // ObjectType (opzionale)
        const objectStr = parts[3]!.trim();
        let objectType: number | null = null;
        if (objectStr !== '') {
            if (!isInt(objectStr)) {
                addError(
                    `Riga ${lineNumber}: OBJECT="${objectStr}" non è un intero valido ` +
                    `(cella ROW=${row}, COL=${col}).`
                );
                continue;
            }
            objectType = parseInt(objectStr, 10);
            if (!VALID_OBJECT_TYPES.has(objectType)) {
                addError(
                    `Riga ${lineNumber}: OBJECT=${objectType} non è un tipo oggetto riconosciuto ` +
                    `(cella ROW=${row}, COL=${col}). ` +
                    `Valori validi: ${[...VALID_OBJECT_TYPES].sort((a, b) => a - b).join(', ')}.`
                );
                continue;
            }
        }

        const objectAdditionalData = parts[4]!.trim();

        // Wall types
        const wallNames = ['TOP', 'RIGHT', 'BOTTOM', 'LEFT'];
        const wallValues: number[] = [];
        let wallError = false;
        for (let w = 0; w < 4; w++) {
            const wallStr = parts[5 + w]!.trim();
            if (!isInt(wallStr)) {
                addError(
                    `Riga ${lineNumber}: muro ${wallNames[w]}="${wallStr}" non è un intero valido ` +
                    `(cella ROW=${row}, COL=${col}).`
                );
                wallError = true;
                break;
            }
            const wallVal = parseInt(wallStr, 10);
            if (!VALID_WALL_TYPES.has(wallVal)) {
                addError(
                    `Riga ${lineNumber}: muro ${wallNames[w]}=${wallVal} ` +
                    `("${WALL_TYPE_NAMES[wallVal] ?? 'sconosciuto'}") non è un tipo di muro riconosciuto ` +
                    `(cella ROW=${row}, COL=${col}). ` +
                    `Valori validi: ${[...VALID_WALL_TYPES].sort((a, b) => a - b).join(', ')}.`
                );
                wallError = true;
                break;
            }
            wallValues.push(wallVal);
        }
        if (wallError) continue;

        parsedCells.push({
            lineNumber,
            row,
            col,
            groundType,
            objectType,
            objectAdditionalData,
            wallTop: wallValues[0]!,
            wallRight: wallValues[1]!,
            wallBottom: wallValues[2]!,
            wallLeft: wallValues[3]!,
        });
    }

    // ------------------------------------------------------------------
    // VALIDAZIONI SEMANTICHE
    // ------------------------------------------------------------------

    // Raccogliamo i dati rilevanti
    const players = parsedCells.filter(c => c.objectType === 1);
    const exits = parsedCells.filter(c => c.objectType === 3);
    const keys = parsedCells.filter(c => c.objectType === 2);
    const candles = parsedCells.filter(c => c.objectType === 4);
    const enemies = parsedCells.filter(c => c.objectType !== null && ENEMY_TYPES.has(c.objectType));

    // --- Giocatore ---
    if (players.length === 0) {
        addError(
            'Il livello non contiene il punto di spawn del giocatore (OBJECT=1, Giocatore). ' +
            'È obbligatorio: il gioco non può partire senza di esso.'
        );
    } else if (players.length > 1) {
        const positions = players.map(p => `(ROW=${p.row}, COL=${p.col}) riga ${p.lineNumber}`).join(', ');
        addError(
            `Il livello contiene ${players.length} spawn del giocatore — deve essercene esattamente 1. ` +
            `Trovati in: ${positions}.`
        );
    }

    // --- Giocatore su terreno percorribile ---
    for (const player of players) {
        if (!WALKABLE_GROUND_TYPES.has(player.groundType)) {
            const groundName = GROUND_TYPE_NAMES[player.groundType] ?? String(player.groundType);
            addError(
                `Riga ${player.lineNumber}: il Giocatore è su terreno "${groundName}" (GROUND=${player.groundType}) ` +
                `che non è percorribile. Lo spawn del giocatore deve essere su un tipo Terra (1, 6, 9–16), ` +
                `Buio (7) o simile.`
            );
        }
    }

    // --- Uscita ---
    if (exits.length === 0) {
        addError(
            'Il livello non contiene nessuna porta di uscita (OBJECT=3, Uscita). ' +
            'È obbligatoria: senza di essa il livello non può essere completato.'
        );
    }

    // --- Chiave ---
    if (keys.length === 0) {
        addWarning(
            'Il livello non contiene nessuna chiave (OBJECT=2, Chiave). ' +
            'Senza chiave l\'uscita non si apre mai e il livello è irrisolvibile. ' +
            'Aggiungi almeno una chiave.'
        );
    }

    // --- Candele ---
    if (candles.length === 0) {
        addWarning(
            'Il livello non contiene nessun pacco di candele (OBJECT=4, Candele). ' +
            'Il gioco è praticamente ingiocabile senza luce. ' +
            'Considera di aggiungere almeno 1–2 pacchi di candele.'
        );
    }

    // --- Dati candele: OBJ_DATA deve essere un intero positivo o vuoto ---
    for (const cell of candles) {
        const data = cell.objectAdditionalData;
        if (data !== '' && (!isInt(data) || parseInt(data, 10) <= 0)) {
            addError(
                `Riga ${cell.lineNumber}: il pacco di candele in (ROW=${cell.row}, COL=${cell.col}) ` +
                `ha OBJ_DATA="${data}" non valido. ` +
                `Deve essere un numero intero positivo (es. "3") oppure vuoto (default=1).`
            );
        }
    }

    // --- Distanza giocatore-nemici ---
    if (players.length === 1) {
        const player = players[0]!;
        for (const enemy of enemies) {
            const dist = manhattanDistance(player.row, player.col, enemy.row, enemy.col);
            const enemyName = OBJECT_TYPE_NAMES[enemy.objectType!] ?? String(enemy.objectType);
            if (dist < 3) {
                addWarning(
                    `Il nemico ${enemyName} in (ROW=${enemy.row}, COL=${enemy.col}) ` +
                    `è a distanza Manhattan ${dist} dal punto di spawn del giocatore ` +
                    `(ROW=${player.row}, COL=${player.col}). ` +
                    `Si raccomanda una distanza di almeno 3–4 celle per dare tempo al giocatore di orientarsi.`
                );
            }
        }
    }

    // --- Formato waypoint nemici ---
    // Pattern valido: PREFIX-GRUPPO-INDICE (es. Z-1-2, S-3-1)
    const waypointPattern = /^[A-Za-z]+-\d+-\d+$/;
    // Raccoglie tutti i waypoint per validazione di gruppo
    const waypointGroups = new Map<string, number[]>(); // "PREFIX-GRUPPO" → [indici trovati]

    for (const cell of parsedCells) {
        if (cell.objectType === null || !ENEMY_TYPES_WITH_WAYPOINT.has(cell.objectType)) continue;
        const data = cell.objectAdditionalData;
        if (data === '') continue; // waypoint opzionale — nessuna regola se assente

        if (!waypointPattern.test(data)) {
            const enemyName = OBJECT_TYPE_NAMES[cell.objectType] ?? String(cell.objectType);
            addError(
                `Riga ${cell.lineNumber}: il nemico ${enemyName} in (ROW=${cell.row}, COL=${cell.col}) ` +
                `ha OBJ_DATA="${data}" con formato waypoint non valido. ` +
                `Il formato atteso è "PREFIX-GRUPPO-INDICE" (es. "Z-1-2", "S-3-1").`
            );
            continue;
        }

        const parts = data.split('-');
        const prefix = parts[0]!;
        const group = parts[1]!;
        const idx = parseInt(parts[2]!, 10);
        const groupKey = `${prefix}-${group}`;

        if (!waypointGroups.has(groupKey)) {
            waypointGroups.set(groupKey, []);
        }
        waypointGroups.get(groupKey)!.push(idx);
    }

    // Per ogni gruppo waypoint: deve esistere almeno l'indice 1
    for (const [groupKey, indices] of waypointGroups) {
        if (!indices.includes(1)) {
            addError(
                `Il gruppo di waypoint "${groupKey}" non contiene il waypoint con indice 1. ` +
                `Ogni gruppo deve partire dall'indice 1 (trovati gli indici: ${indices.sort((a, b) => a - b).join(', ')}). ` +
                `Il percorso di patruglia potrebbe non funzionare correttamente.`
            );
        }
        // Avvisa se ci sono indici duplicati nello stesso gruppo
        const uniqueIndices = new Set(indices);
        if (uniqueIndices.size !== indices.length) {
            const dups = indices.filter((v, i, arr) => arr.indexOf(v) !== i);
            addWarning(
                `Il gruppo di waypoint "${groupKey}" ha indici duplicati: [${dups.join(', ')}]. ` +
                `Ogni waypoint in un gruppo deve avere un indice univoco.`
            );
        }
    }

    // --- Nemici su terreno percorribile ---
    for (const enemy of enemies) {
        const enemyName = OBJECT_TYPE_NAMES[enemy.objectType!] ?? String(enemy.objectType);
        if (!WALKABLE_GROUND_TYPES.has(enemy.groundType) && !RIEMPIMENTO_GROUND_TYPES.has(enemy.groundType)) {
            // NonCella
            addError(
                `Riga ${enemy.lineNumber}: il nemico ${enemyName} in (ROW=${enemy.row}, COL=${enemy.col}) ` +
                `è su una cella con GROUND=NonCella (0). I nemici devono essere su terreno percorribile.`
            );
        } else if (RIEMPIMENTO_GROUND_TYPES.has(enemy.groundType)) {
            addWarning(
                `Riga ${enemy.lineNumber}: il nemico ${enemyName} in (ROW=${enemy.row}, COL=${enemy.col}) ` +
                `è su terreno Riempimento (GROUND=${enemy.groundType}), che non è calpestabile. ` +
                `Sposta il nemico su un pavimento Terra.`
            );
        }
    }

    // --- Bordi mappa: avviso se celle di bordo sono aperte ---
    const borderCells = parsedCells.filter(c =>
        c.row === 0 || c.row === EXPECTED_ROWS - 1 ||
        c.col === 0 || c.col === EXPECTED_COLS - 1
    );

    for (const cell of borderCells) {
        const isRiempimento = RIEMPIMENTO_GROUND_TYPES.has(cell.groundType);
        const hasAnyWall = cell.wallTop !== 0 || cell.wallRight !== 0 || cell.wallBottom !== 0 || cell.wallLeft !== 0;
        if (!isRiempimento && !hasAnyWall) {
            addWarning(
                `Riga ${cell.lineNumber}: cella di bordo mappa in (ROW=${cell.row}, COL=${cell.col}) ` +
                `è percorribile e non ha muri su nessun lato. ` +
                `I bordi della mappa dovrebbero essere Riempimento o avere muri Occludente per evitare che il giocatore esca dalla mappa.`
            );
        }
    }

    return { errors, warnings };
}

// ---------------------------------------------------------------------------
// Entrypoint
// ---------------------------------------------------------------------------

const args = process.argv.slice(2);
if (args.length === 0) {
    console.error('Uso: npx tsx scripts/validate-level.ts <percorso/al/livello.mark>');
    process.exit(1);
}

const filePath = resolve(args[0]!);
let content: string;
try {
    content = readFileSync(filePath, 'utf-8');
} catch (err) {
    console.error(`Impossibile leggere il file: ${filePath}`);
    console.error((err as Error).message);
    process.exit(1);
}

console.log(`\nValidazione: ${filePath}\n`);

const { errors, warnings } = validateLevel(content);

if (warnings.length > 0) {
    console.log(`⚠  AVVISI (${warnings.length}):`);
    for (const w of warnings) console.log(w);
    console.log();
}

if (errors.length > 0) {
    console.log(`✗  ERRORI (${errors.length}):`);
    for (const e of errors) console.log(e);
    console.log();
    console.log('Il livello NON è valido.\n');
    process.exit(1);
} else {
    if (warnings.length === 0) {
        console.log('✓  Nessun problema trovato.');
    } else {
        console.log('✓  Nessun errore bloccante (vedi avvisi sopra).');
    }
    console.log('Il livello è valido.\n');
    process.exit(0);
}
