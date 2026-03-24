import { CharacterActions } from '../interaction/CharacterActions';
import type { IEnemyAI } from './IEnemyAI';
import type { Enemy } from '../characters/Enemy';
import type { GameGrid } from '../grid/GameGrid';
import type { GameCell } from '../grid/GameCell';
import type { CandlesManager } from '../objects/CandlesManager';
import type { Player } from '../characters/Player';
import { KillCandleAI } from './KillCandleAI';
import { RandomMovementAI } from './RandomMovementAI';

/**
 * Cyclic waypoint patrol AI (port of C# PatrolAI).
 * Optionally searches for nearby candles before patrolling.
 */
export class PatrolAI implements IEnemyAI {
    protected readonly _grid: GameGrid;
    protected readonly _candlesManager: CandlesManager;
    protected readonly _player: Player;
    protected readonly _searchesCandles: boolean;
    protected readonly _waypoints: GameCell[];
    protected _waypointIndex: number;
    protected _actions: CharacterActions[] = [];
    /** Kill duration for candles (override in subclasses: Ghost=250, Bubu=500). */
    protected _killDurationMS = 3000;

    constructor(
        waypoints: GameCell[],
        grid: GameGrid,
        candlesManager: CandlesManager,
        player: Player,
        searchesCandles: boolean,
    ) {
        this._waypoints = waypoints;
        this._grid = grid;
        this._candlesManager = candlesManager;
        this._player = player;
        this._searchesCandles = searchesCandles;
        // Start patrolling from waypoint 1 (0 is starting position)
        this._waypointIndex = waypoints.length > 1 ? 1 : 0;
    }

    clearActions(): void {
        this._actions = [];
    }

    update(enemy: Enemy, _deltaMS: number): CharacterActions {
        // Optional candle search
        if (this._searchesCandles) {
            const cell = enemy.currentCell;
            if (cell) {
                const result = this._candlesManager.getNearestCandlePath(cell, enemy.nCellsCandleRange ?? 3, this._grid);
                if (result) {
                    const killAI = new KillCandleAI(result.candle, result.actions, this, this._killDurationMS);
                    return enemy.setNewAI(killAI);
                }
            }
        }

        // Dequeue buffered actions
        if (this._actions.length > 0) {
            return this._actions.shift()!;
        }

        // Calculate path to next waypoint
        if (this._waypoints.length === 0) return CharacterActions.Idle;

        const destWaypoint = this._waypoints[this._waypointIndex]!;
        const fromCell = enemy.currentCell;
        if (fromCell) {
            this._actions = this._grid.findActionsToPath(fromCell, destWaypoint);
        }
        this._waypointIndex = (this._waypointIndex + 1) % this._waypoints.length;

        if (this._actions.length === 0) {
            // Pathfinding failed — fallback
            return enemy.setNewAI(new RandomMovementAI(this));
        }

        return this._actions.shift()!;
    }
}
