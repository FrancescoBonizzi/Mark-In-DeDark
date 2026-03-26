import { Container } from 'pixi.js';
import { GroundTypes, LinkDirection, ObjectTypes } from './enums';
import { CharacterActions } from '../interaction/CharacterActions';
import { CELL_SIZE, GameCell } from './GameCell';
import type { GridBundle } from './GridTypes';
import type { LevelConfig } from './LevelConfig';
import type { LevelCustomization } from './LevelCustomization';
import { createPillarsForCell } from './PillarFactory';
import { removeDuplicatePillars, removeDuplicateWalls } from './GameGridCleaner';
import type { IWall } from './IWall';
import type { Pillar } from './Pillar';
import { WallEsteticDoor } from './WallEsteticDoor';
import { WallInteractiveDoor } from './WallInteractiveDoor';
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
    /** Ground tiles and dirty decals — rendered by ShadowSystem for the lit ground pass. */
    readonly groundContainer: Container;
    /** Normal map sprites for ground tiles — used by ShadowSystem normal map pass. */
    readonly groundNormalContainer: Container;
    /** Walls, pillars, and Riempimento fill blocks — Y-sorted, rendered offline by ShadowSystem. */
    readonly objectsContainer: Container;
    /** Normal map sprites for walls, pillars, and Riempimento — used by ShadowSystem normal map pass. */
    readonly objectsNormalContainer: Container;

    /** cells[col][row] */
    private readonly _cells: (GameCell | null)[][];
    private readonly _bundle: GridBundle;
    private readonly _config: LevelConfig;
    private readonly _customization: LevelCustomization;

    allWalls: IWall[] = [];
    allPillars: Pillar[] = [];

    entranceDoor: WallEsteticDoor | null = null;
    exitDoor: WallEsteticDoor | null = null;

    playerStartCol = 0;
    playerStartRow = 0;
    exitCol = 0;
    exitRow = 0;
    keyCol = 0;
    keyRow = 0;

    enemyPlacements: EnemyPlacement[] = [];
    candlePlacements: CandlePlacement[] = [];
    trapPlacements: TrapPlacement[] = [];
    patrolPaths: PatrolPath[] = [];

    /** Maps cells that carry a text trigger (stringCode) to the raw code string. */
    readonly cellTriggerTexts = new Map<GameCell, string>();

    private readonly _pathCache = new Map<string, GameCell[]>();

    constructor(bundle: GridBundle, config: LevelConfig, customization: LevelCustomization) {
        this._bundle = bundle;
        this._config = config;
        this._customization = customization;

        this.groundContainer = new Container();
        this.groundNormalContainer = new Container();
        this.objectsContainer = new Container();
        this.objectsContainer.sortableChildren = true;
        this.objectsNormalContainer = new Container();
        this.objectsNormalContainer.sortableChildren = true;

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

                // Assign entrance/exit door references
                for (const wall of cell.walls.values()) {
                    if (wall instanceof WallEsteticDoor) {
                        if (cell.isEntrance) this.entranceDoor = wall;
                        if (cell.isExit) this.exitDoor = wall;
                    }
                }

                // Store object placements
                if (data.objectType !== null) {
                    this._storeObjectPlacement(col, row, data.objectType, data.objectAdditionalData, data.stringCode);
                }

                // Store cell text trigger (tutorial/ambient hint)
                if (data.stringCode) {
                    this.cellTriggerTexts.set(cell, data.stringCode);
                }

                // Store trap placements
                this._storeTrapPlacement(col, row, data.groundType);
            }
        }
    }

    private _assignGroundSprite(cell: GameCell, groundType: GroundTypes, _row: number): void {
        const map = this._config.groundTypeToTexture;
        const normalMap = this._config.groundTypeToNormalTexture;

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
                const normalTex = normalMap.get(groundType) ?? null;
                if (normalTex) cell.setGroundNormalSprite(normalTex, Z_GROUND);
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
                const normalTex = normalMap.get(groundType) ?? null;
                if (normalTex) cell.setGroundNormalSprite(normalTex, zForRiempimento(cell.y), -48);
                cell.isUnLinked = true;
                break;
            }

            case GroundTypes.TrappolaMorteIstantanea:
            case GroundTypes.TrappolaTemporizzata:
            case GroundTypes.TriggerGoblin: {
                const trapTex = this._config.groundTextures[Math.floor(Math.random() * this._config.groundTextures.length)];
                if (trapTex) cell.setGroundSprite(trapTex, Z_GROUND);
                cell.noDecals = true;
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
        if (!cell.groundSprite || cell.isUnLinked || cell.noDecals) return;
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

            case ObjectTypes.Chiave:
                this.keyCol = col;
                this.keyRow = row;
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

                const pillars = createPillarsForCell(col, row, cell, this._cells, this._config, this._customization.pillarOffset);

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
                cell.addToContainer(
                    this.groundContainer,
                    this.objectsContainer,
                    this.groundNormalContainer,
                    this.objectsNormalContainer,
                );
            }
        }

        // Walls and pillars go into objectsContainer (Y-sorted with Riempimento)
        for (const wall of this.allWalls) {
            wall.addToContainer(this.objectsContainer);
            wall.addNormalToContainer(this.objectsNormalContainer);
        }
        for (const pillar of this.allPillars) {
            pillar.addToContainer(this.objectsContainer);
            pillar.addNormalToContainer(this.objectsNormalContainer);
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

            if (ignoreWalls) {
                // Ghost pathfinding: traverse raw grid neighbors ignoring wall links
                for (const [dc, dr] of [[0,-1],[0,1],[1,0],[-1,0]] as [number,number][]) {
                    const neighbour = this._cells[current.col + dc]?.[current.row + dr] ?? null;
                    if (!neighbour || neighbour.isUnLinked) continue;
                    if (visited.has(neighbour)) continue;
                    visited.add(neighbour);
                    parent.set(neighbour, current);
                    queue.push(neighbour);
                }
            } else {
                for (const [dir, neighbour] of current.links) {
                    if (!neighbour) continue;
                    if (visited.has(neighbour)) continue;
                    if (!this._isPassable(current, dir)) continue;
                    visited.add(neighbour);
                    parent.set(neighbour, current);
                    queue.push(neighbour);
                }
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

    /** Converts a BFS cell path to a sequence of directional CharacterActions. */
    pathToActions(path: GameCell[]): CharacterActions[] {
        const actions: CharacterActions[] = [];
        for (let i = 0; i < path.length - 1; i++) {
            const a = path[i]!;
            const b = path[i + 1]!;
            const dc = b.col - a.col;
            const dr = b.row - a.row;
            if (dc === 1)       actions.push(CharacterActions.Right);
            else if (dc === -1) actions.push(CharacterActions.Left);
            else if (dr === 1)  actions.push(CharacterActions.Down);
            else if (dr === -1) actions.push(CharacterActions.Up);
        }
        return actions;
    }

    /** BFS path respecting walls, converted to CharacterActions[]. */
    findActionsToPath(from: GameCell, to: GameCell): CharacterActions[] {
        return this.pathToActions(this.findPath(from, to));
    }

    /** BFS path ignoring walls (Ghost), converted to CharacterActions[]. */
    findActionsToPathIgnoringWalls(from: GameCell, to: GameCell): CharacterActions[] {
        return this.pathToActions(this.findPathIgnoringWalls(from, to));
    }

    /** Returns true if the cell is passable in the given direction. */
    isPassableWithAction(cell: GameCell, action: CharacterActions): boolean {
        switch (action) {
            case CharacterActions.Up:    return cell.northPassable;
            case CharacterActions.Down:  return cell.southPassable;
            case CharacterActions.Right: return cell.eastPassable;
            case CharacterActions.Left:  return cell.westPassable;
            default: return false;
        }
    }

    /**
     * Breaks the wall in the given direction from the given cell (Taurus charge).
     * Removes the wall visually, updates graph links and passability, clears path cache.
     */
    breakWall(cell: GameCell, direction: CharacterActions): void {
        const dir = this._actionToLinkDir(direction);
        if (dir === null) return;

        const wall = cell.walls.get(dir);
        if (!wall) return;

        const neighbor = this._cellInDirection(cell, dir);
        if (!neighbor) return;

        const opposite = this._opposite(dir);
        cell.links.set(dir, neighbor);
        neighbor.links.set(opposite, cell);
        this._setPassability(cell, dir, true);
        this._setPassability(neighbor, opposite, true);
        cell.walls.delete(dir);

        this._pathCache.clear();
    }

    private _actionToLinkDir(action: CharacterActions): LinkDirection | null {
        switch (action) {
            case CharacterActions.Up:    return LinkDirection.NorthLink;
            case CharacterActions.Down:  return LinkDirection.SouthLink;
            case CharacterActions.Right: return LinkDirection.EastLink;
            case CharacterActions.Left:  return LinkDirection.WestLink;
            default: return null;
        }
    }

    /**
     * Opens the exit passage by restoring graph links and passability for the
     * WallEsteticDoor in the exit cell (port of C# behavior after key pickup).
     */
    openExitPassage(): void {
        const exitCell = this.getCell(this.exitCol, this.exitRow);
        if (!exitCell) return;

        for (const [dir, wall] of exitCell.walls) {
            if (!(wall instanceof WallEsteticDoor)) continue;

            const neighbor = this._cellInDirection(exitCell, dir);
            if (!neighbor) continue;

            const opposite = this._opposite(dir);
            exitCell.links.set(dir, neighbor);
            neighbor.links.set(opposite, exitCell);
            this._setPassability(exitCell, dir, true);
            this._setPassability(neighbor, opposite, true);

            this._pathCache.clear();
            return;
        }
    }

    update(deltaMS: number): void {
        for (const col of this._cells) {
            for (const cell of col) {
                if (!cell) continue;
                cell.updateWalls(deltaMS);
            }
        }
    }

    /**
     * Closes all interactive doors after grid initialization (port of C# CloseAllDoors).
     * Called once after the grid is built.
     */
    closeAllDoors(): void {
        for (const wall of this.allWalls) {
            if (wall instanceof WallInteractiveDoor) {
                if (wall.isOpen) {
                    wall.close();
                    // Remove graph links for this door
                    this._setDoorPassability(wall, false);
                }
            }
        }
        this._pathCache.clear();
    }

    /**
     * Toggles the interactive door(s) adjacent to the given cell
     * (port of C# GameGrid.SwitchDoorIfAny).
     */
    switchDoorIfAny(cell: GameCell | null): void {
        if (!cell) return;

        const doorsToggled = new Set<WallInteractiveDoor>();

        // A door in the level file is stored only in ONE cell's walls map.
        // Check both this cell's walls AND the mirror wall on each adjacent cell,
        // so the door is reachable from either side.
        const pairs: [LinkDirection, LinkDirection][] = [
            [LinkDirection.NorthLink, LinkDirection.SouthLink],
            [LinkDirection.SouthLink, LinkDirection.NorthLink],
            [LinkDirection.EastLink,  LinkDirection.WestLink],
            [LinkDirection.WestLink,  LinkDirection.EastLink],
        ];

        for (const [dir, opposite] of pairs) {
            // Check this cell's own wall
            const wall = cell.walls.get(dir);
            if (wall instanceof WallInteractiveDoor && !doorsToggled.has(wall)) {
                doorsToggled.add(wall);
                if (wall.isOpen) { wall.close(); this._setDoorPassability(wall, false); }
                else             { wall.open();  this._setDoorPassability(wall, true);  }
            }
            // Check the neighbor's mirror wall (same physical door, stored on the other side)
            const neighbor = this._cellInDirection(cell, dir);
            if (neighbor) {
                const mirrorWall = neighbor.walls.get(opposite);
                if (mirrorWall instanceof WallInteractiveDoor && !doorsToggled.has(mirrorWall)) {
                    doorsToggled.add(mirrorWall);
                    if (mirrorWall.isOpen) { mirrorWall.close(); this._setDoorPassability(mirrorWall, false); }
                    else                  { mirrorWall.open();  this._setDoorPassability(mirrorWall, true);  }
                }
            }
        }

        if (doorsToggled.size > 0) this._pathCache.clear();
    }

    /** Returns true if the cell has at least one interactive door reachable from it (own walls or adjacent mirror walls). */
    hasDoorOnCell(cell: GameCell): boolean {
        const pairs: [LinkDirection, LinkDirection][] = [
            [LinkDirection.NorthLink, LinkDirection.SouthLink],
            [LinkDirection.SouthLink, LinkDirection.NorthLink],
            [LinkDirection.EastLink,  LinkDirection.WestLink],
            [LinkDirection.WestLink,  LinkDirection.EastLink],
        ];
        for (const [dir, opposite] of pairs) {
            if (cell.walls.get(dir) instanceof WallInteractiveDoor) return true;
            const neighbor = this._cellInDirection(cell, dir);
            if (neighbor?.walls.get(opposite) instanceof WallInteractiveDoor) return true;
        }
        return false;
    }

    private _setDoorPassability(door: WallInteractiveDoor, passable: boolean): void {
        // Find which two cells this door separates
        const dir = door.direction;
        const opposite = this._opposite(dir);

        for (const col of this._cells) {
            for (const cell of col) {
                if (!cell) continue;
                if (cell.walls.get(dir) !== door) continue;

                const neighbor = this._cellInDirection(cell, dir);
                if (!neighbor) continue;

                // Update graph links and passability flags
                cell.links.set(dir, passable ? neighbor : null);
                neighbor.links.set(opposite, passable ? cell : null);

                this._setPassability(cell, dir, passable);
                this._setPassability(neighbor, opposite, passable);
                return;
            }
        }
    }

    private _cellInDirection(cell: GameCell, dir: LinkDirection): GameCell | null {
        const [dc, dr] = this._dirDelta(dir);
        return this._cells[cell.col + dc]?.[cell.row + dr] ?? null;
    }

    private _dirDelta(dir: LinkDirection): [number, number] {
        switch (dir) {
            case LinkDirection.NorthLink: return [0, -1];
            case LinkDirection.SouthLink: return [0, 1];
            case LinkDirection.EastLink:  return [1, 0];
            case LinkDirection.WestLink:  return [-1, 0];
        }
    }

    private _opposite(dir: LinkDirection): LinkDirection {
        switch (dir) {
            case LinkDirection.NorthLink: return LinkDirection.SouthLink;
            case LinkDirection.SouthLink: return LinkDirection.NorthLink;
            case LinkDirection.EastLink:  return LinkDirection.WestLink;
            case LinkDirection.WestLink:  return LinkDirection.EastLink;
        }
    }

    private _setPassability(cell: GameCell, dir: LinkDirection, passable: boolean): void {
        switch (dir) {
            case LinkDirection.NorthLink: cell.northPassable = passable; break;
            case LinkDirection.SouthLink: cell.southPassable = passable; break;
            case LinkDirection.EastLink:  cell.eastPassable = passable; break;
            case LinkDirection.WestLink:  cell.westPassable = passable; break;
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
