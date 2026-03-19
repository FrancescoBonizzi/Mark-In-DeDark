import { Container } from 'pixi.js';
import { GroundTypes, LinkDirection, ObjectTypes } from './enums';
import { CELL_SIZE, GameCell } from './GameCell';
import type { GridBundle } from './GridTypes';
import type { LevelConfig } from './LevelConfig';
import type { LevelCustomization } from './LevelCustomization';
import { createPillarsForCell } from './PillarFactory';
import { removeDuplicatePillars, removeDuplicateWalls } from './GameGridCleaner';
import type { IWall } from './IWall';
import type { Pillar } from './Pillar';
import type { WallInteractiveDoor } from './WallInteractiveDoor';
import { Z_GROUND, zForRiempimento } from './ZIndexCalculator';

export const GRID_COLS = 45;
export const GRID_ROWS = 25;
export const GRID_MAX_HEIGHT = GRID_ROWS * CELL_SIZE; // 4800

export interface EnemyPlacement {
    objectType: ObjectTypes;
    col: number;
    row: number;
    stringCode: string;
    additionalData: string;
}

export interface CandlePlacement {
    col: number;
    row: number;
    count: number;
}

export interface TrapPlacement {
    groundType: GroundTypes;
    col: number;
    row: number;
}

export interface PatrolWaypoint {
    col: number;
    row: number;
    waypointIndex: number;
}

export interface PatrolPath {
    letter: string;
    patrolNum: number;
    waypoints: { col: number; row: number }[];
}

export class GameGrid {
    readonly gridContainer: Container;

    /** cells[col][row] */
    private readonly _cells: (GameCell | null)[][];
    private readonly _bundle: GridBundle;
    private readonly _config: LevelConfig;
    private readonly _customization: LevelCustomization;

    allWalls: IWall[] = [];
    allPillars: Pillar[] = [];

    entranceDoor: WallInteractiveDoor | null = null;
    exitDoor: WallInteractiveDoor | null = null;

    playerStartCol = 0;
    playerStartRow = 0;
    exitCol = 0;
    exitRow = 0;

    enemyPlacements: EnemyPlacement[] = [];
    candlePlacements: CandlePlacement[] = [];
    trapPlacements: TrapPlacement[] = [];
    patrolPaths: PatrolPath[] = [];

    private readonly _pathCache = new Map<string, GameCell[]>();

    constructor(bundle: GridBundle, config: LevelConfig, customization: LevelCustomization) {
        this._bundle = bundle;
        this._config = config;
        this._customization = customization;

        this.gridContainer = new Container();
        this.gridContainer.sortableChildren = true;

        // Initialize empty cell array [col][row]
        this._cells = Array.from({ length: bundle.columns }, () =>
            new Array<GameCell | null>(bundle.rows).fill(null)
        );

        this._load();
    }

    private _load(): void {
        this._createCells();
        this._buildGraph();
        this._initializePillars();
        this._cleanDuplicates();
        this._collectWallsAndPillars();
        this._addToContainer();
    }

    private _createCells(): void {
        const { grid, rows, columns } = this._bundle;

        for (let col = 0; col < columns; col++) {
            for (let row = 0; row < rows; row++) {
                const data = grid[col]?.[row];
                if (!data || data.groundType === GroundTypes.NonCella) continue;

                const cell = new GameCell(col, row);
                this._cells[col]![row] = cell;

                // Assign ground sprite based on GroundType
                this._assignGroundSprite(cell, data.groundType, row);

                // Assign dirty overlay sprite
                this._assignDirtySprite(cell);

                // Initialize walls
                cell.initializeWalls(data, this._config, this._customization);

                // Store object placements
                if (data.objectType !== null) {
                    this._storeObjectPlacement(col, row, data.objectType, data.objectAdditionalData, data.stringCode);
                }

                // Store trap placements
                this._storeTrapPlacement(col, row, data.groundType);
            }
        }
    }

    private _assignGroundSprite(cell: GameCell, groundType: GroundTypes, _row: number): void {
        const map = this._config.groundTypeToTexture;

        switch (groundType) {
            case GroundTypes.Buio:
                cell.setGroundSprite(this._config.darkCellTexture, Z_GROUND);
                cell.isUnLinked = true;
                break;

            case GroundTypes.Terra1:
            case GroundTypes.Terra2:
            case GroundTypes.Terra3:
            case GroundTypes.Terra4:
            case GroundTypes.Terra5:
            case GroundTypes.Terra6:
            case GroundTypes.Terra7:
            case GroundTypes.Terra8:
            case GroundTypes.Terra9:
            case GroundTypes.Terra10: {
                const tex = map.get(groundType) ?? this._config.groundTextures[0];
                if (!tex) break;
                cell.setGroundSprite(tex, Z_GROUND);
                break;
            }

            case GroundTypes.Riempimento1:
            case GroundTypes.Riempimento2:
            case GroundTypes.Riempimento3:
            case GroundTypes.Riempimento4:
            case GroundTypes.Riempimento5:
            case GroundTypes.Riempimento6:
            case GroundTypes.Riempimento7:
            case GroundTypes.Riempimento8:
            case GroundTypes.Riempimento9:
            case GroundTypes.Riempimento10:
            case GroundTypes.Riempimento11:
            case GroundTypes.Riempimento12:
            case GroundTypes.Riempimento13: {
                const tex = map.get(groundType);
                if (!tex) break;
                // Riempimento shifted up 48px (matches C# Origin.Y += 48), z based on Y position
                cell.setGroundSprite(tex, zForRiempimento(cell.y), -48);
                cell.isUnLinked = true;
                break;
            }

            case GroundTypes.TrappolaMorteIstantanea:
            case GroundTypes.TrappolaTemporizzata:
            case GroundTypes.TriggerGoblin: {
                const trapTex = this._config.groundTextures[Math.floor(Math.random() * this._config.groundTextures.length)];
                if (trapTex) cell.setGroundSprite(trapTex, Z_GROUND);
                break;
            }

            default:
                break;
        }
    }

    private _dirtyTextureIndex = 0;

    private _assignDirtySprite(cell: GameCell): void {
        const { dirtyTextures } = this._config;
        if (dirtyTextures.length === 0) return;
        if (!cell.groundSprite || cell.isUnLinked) return;
        if (Math.random() > this._bundle.dirtyProbabilityPct / 100) return;

        const tex = dirtyTextures[this._dirtyTextureIndex % dirtyTextures.length]!;
        this._dirtyTextureIndex++;
        cell.setDirtySprite(tex);
    }

    private _storeObjectPlacement(
        col: number,
        row: number,
        objectType: number,
        additionalData: string,
        stringCode: string,
    ): void {
        switch (objectType as ObjectTypes) {
            case ObjectTypes.Giocatore:
                this.playerStartCol = col;
                this.playerStartRow = row;
                break;

            case ObjectTypes.Uscita:
                this.exitCol = col;
                this.exitRow = row;
                break;

            case ObjectTypes.Candele: {
                const count = additionalData !== '' ? Number(additionalData) : 1;
                this.candlePlacements.push({ col, row, count });
                break;
            }

            case ObjectTypes.Zombie:
            case ObjectTypes.Scheletro:
            case ObjectTypes.Goblin:
            case ObjectTypes.Taurus:
            case ObjectTypes.Ghost:
            case ObjectTypes.Bubu:
            case ObjectTypes.Lucina:
                this.enemyPlacements.push({
                    objectType: objectType as ObjectTypes,
                    col,
                    row,
                    stringCode,
                    additionalData,
                });
                this._parsePatrolPath(col, row, additionalData);
                break;

            case ObjectTypes.OggettoNonPassabile1:
            case ObjectTypes.OggettoNonPassabile2:
            case ObjectTypes.OggettoPassabile1:
                // Stored as enemy placements for now
                this.enemyPlacements.push({
                    objectType: objectType as ObjectTypes,
                    col,
                    row,
                    stringCode,
                    additionalData,
                });
                break;

            default:
                break;
        }
    }

    private _storeTrapPlacement(col: number, row: number, groundType: GroundTypes): void {
        if (
            groundType === GroundTypes.TrappolaMorteIstantanea ||
            groundType === GroundTypes.TrappolaTemporizzata ||
            groundType === GroundTypes.TriggerGoblin
        ) {
            this.trapPlacements.push({ groundType, col, row });
        }
    }

    /**
     * Parse patrol waypoint from stringCode format: "letter-patrolNum-waypointNum"
     * e.g. "A-1-0" means letter A, patrol group 1, waypoint 0
     */
    private _parsePatrolPath(col: number, row: number, stringCode: string): void {
        if (!stringCode) return;
        const parts = stringCode.split('-');
        if (parts.length < 3) return;

        const letter = parts[0]!;
        const patrolNum = Number(parts[1]);
        const waypointIndex = Number(parts[2]);

        const key = `${letter}-${patrolNum}`;
        let path = this.patrolPaths.find(p => `${p.letter}-${p.patrolNum}` === key);
        if (!path) {
            path = { letter, patrolNum, waypoints: [] };
            this.patrolPaths.push(path);
        }

        // Insert in sorted order by waypointIndex
        const entry = { col, row };
        path.waypoints[waypointIndex] = entry;
    }

    private _buildGraph(): void {
        const { rows, columns } = this._bundle;

        for (let col = 0; col < columns; col++) {
            for (let row = 0; row < rows; row++) {
                const cell = this._cells[col]?.[row];
                if (!cell || cell.isUnLinked) continue;

                const north = this._cells[col]?.[row - 1] ?? null;
                const south = this._cells[col]?.[row + 1] ?? null;
                const east = this._cells[col + 1]?.[row] ?? null;
                const west = this._cells[col - 1]?.[row] ?? null;

                // Bidirectional: link only if both sides allow passage and neither is unlinked
                const northLinked = cell.northPassable && north !== null && !north.isUnLinked && north.southPassable;
                const southLinked = cell.southPassable && south !== null && !south.isUnLinked && south.northPassable;
                const eastLinked = cell.eastPassable && east !== null && !east.isUnLinked && east.westPassable;
                const westLinked = cell.westPassable && west !== null && !west.isUnLinked && west.eastPassable;

                cell.links.set(LinkDirection.NorthLink, northLinked ? north : null);
                cell.links.set(LinkDirection.SouthLink, southLinked ? south : null);
                cell.links.set(LinkDirection.EastLink, eastLinked ? east : null);
                cell.links.set(LinkDirection.WestLink, westLinked ? west : null);

                // Correct passability flags (match C# GridAsGraph behavior):
                // If cell thinks it's passable but no valid link was established,
                // mark it as impassable so PillarFactory rules fire correctly.
                if (cell.northPassable && !northLinked) cell.northPassable = false;
                if (cell.southPassable && !southLinked) cell.southPassable = false;
                if (cell.eastPassable && !eastLinked) cell.eastPassable = false;
                if (cell.westPassable && !westLinked) cell.westPassable = false;

                // Propagate destroyable flags to neighbours
                if (cell.northDestroyable && north) north.southDestroyable = true;
                if (cell.southDestroyable && south) south.northDestroyable = true;
                if (cell.eastDestroyable && east) east.westDestroyable = true;
                if (cell.westDestroyable && west) west.eastDestroyable = true;
            }
        }
    }

    private _initializePillars(): void {
        const { rows, columns } = this._bundle;

        for (let col = 0; col < columns; col++) {
            for (let row = 0; row < rows; row++) {
                const cell = this._cells[col]?.[row];
                if (!cell || cell.isUnLinked) continue;

                const pillars = createPillarsForCell(col, row, cell, this._cells, this._config);

                for (const pillar of pillars) {
                    cell.pillars.set(pillar.position, pillar);
                }
            }
        }
    }

    private _cleanDuplicates(): void {
        // Collect all walls and pillars
        const allWalls: IWall[] = [];
        const allPillars: Pillar[] = [];

        for (const col of this._cells) {
            for (const cell of col) {
                if (!cell) continue;
                for (const wall of cell.walls.values()) allWalls.push(wall);
                for (const pillar of cell.pillars.values()) allPillars.push(pillar);
            }
        }

        this.allWalls = removeDuplicateWalls(allWalls);
        this.allPillars = removeDuplicatePillars(allPillars);
    }

    private _collectWallsAndPillars(): void {
        // allWalls / allPillars already set in _cleanDuplicates
    }

    private _addToContainer(): void {
        for (const col of this._cells) {
            for (const cell of col) {
                if (!cell) continue;
                cell.addToContainer(this.gridContainer);
            }
        }

        // Add deduplicated walls and pillars
        for (const wall of this.allWalls) {
            wall.addToContainer(this.gridContainer);
        }
        for (const pillar of this.allPillars) {
            pillar.addToContainer(this.gridContainer);
        }
    }

    getCell(col: number, row: number): GameCell | null {
        return this._cells[col]?.[row] ?? null;
    }

    getCellAtPixel(x: number, y: number): GameCell | null {
        const col = Math.floor(x / CELL_SIZE);
        const row = Math.floor(y / CELL_SIZE);
        return this.getCell(col, row);
    }

    get playerStartCell(): GameCell | null {
        return this.getCell(this.playerStartCol, this.playerStartRow);
    }

    /** BFS pathfinding respecting walls */
    findPath(from: GameCell, to: GameCell): GameCell[] {
        return this._bfs(from, to, false);
    }

    /** BFS pathfinding ignoring walls */
    findPathIgnoringWalls(from: GameCell, to: GameCell): GameCell[] {
        return this._bfs(from, to, true);
    }

    private _bfs(from: GameCell, to: GameCell, ignoreWalls: boolean): GameCell[] {
        if (from === to) return [from];

        const visited = new Set<GameCell>();
        const parent = new Map<GameCell, GameCell | null>();
        const queue: GameCell[] = [from];
        visited.add(from);
        parent.set(from, null);

        while (queue.length > 0) {
            const current = queue.shift()!;
            if (current === to) break;

            for (const [dir, neighbour] of current.links) {
                if (!neighbour) continue;
                if (visited.has(neighbour)) continue;

                if (!ignoreWalls) {
                    // Check if passable in this direction
                    const passable = this._isPassable(current, dir);
                    if (!passable) continue;
                }

                visited.add(neighbour);
                parent.set(neighbour, current);
                queue.push(neighbour);
            }
        }

        if (!parent.has(to)) return [];

        // Reconstruct path
        const path: GameCell[] = [];
        let cur: GameCell | null = to;
        while (cur !== null) {
            path.unshift(cur);
            cur = parent.get(cur) ?? null;
        }
        return path;
    }

    private _isPassable(cell: GameCell, dir: LinkDirection): boolean {
        switch (dir) {
            case LinkDirection.NorthLink: return cell.northPassable;
            case LinkDirection.EastLink: return cell.eastPassable;
            case LinkDirection.SouthLink: return cell.southPassable;
            case LinkDirection.WestLink: return cell.westPassable;
        }
    }

    invalidatePathCache(): void {
        this._pathCache.clear();
    }

    update(deltaMS: number): void {
        for (const col of this._cells) {
            for (const cell of col) {
                if (!cell) continue;
                cell.updateWalls(deltaMS);
            }
        }
    }

    updateViewportCulling(
        cameraX: number,
        cameraY: number,
        cameraW: number,
        cameraH: number,
    ): void {
        const colMin = Math.max(0, Math.floor(cameraX / CELL_SIZE) - 1);
        const colMax = Math.min(this._bundle.columns - 1, Math.ceil((cameraX + cameraW) / CELL_SIZE) + 1);
        const rowMin = Math.max(0, Math.floor(cameraY / CELL_SIZE) - 1);
        const rowMax = Math.min(this._bundle.rows - 1, Math.ceil((cameraY + cameraH) / CELL_SIZE) + 1);

        for (let col = 0; col < this._bundle.columns; col++) {
            for (let row = 0; row < this._bundle.rows; row++) {
                const cell = this._cells[col]?.[row];
                if (!cell) continue;
                const visible = col >= colMin && col <= colMax && row >= rowMin && row <= rowMax;
                cell.setVisible(visible);
            }
        }
    }
}
